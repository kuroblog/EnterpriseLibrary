
namespace LoggingBlock.Basic
{
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using System;
    using System.Collections.Generic;
    using System.Threading;

    class Program
    {
        /// <summary>
        /// 保证每次生成的随机数都不一样
        /// </summary>
        /// <remarks>
        /// Random是非线程安全的，考虑到多线程的场景是需要使用lock的
        /// 使用lock会影响性能，所以使用ThreadLocal来保证每个线程的Randonm实例都是唯一的
        /// 使用lock的例子
        /// private static readonly Random random = new Random();
        /// private static readonly object syncObject = new object();
        /// public static GenerateNumber(int minValue, int maxValue){
        ///     lock (syncObject){
        ///         return random.Next(minValue, maxValue);
        ///     }
        /// }
        /// </remarks>
        private static readonly ThreadLocal<Random> ThreadRandom = new ThreadLocal<Random>(() => new Random());

        /// <summary>
        /// 生成随机数
        /// </summary>
        /// <param name="minValue">最小值</param>
        /// <param name="maxValue">最大值</param>
        /// <returns>整型随机数</returns>
        public static int GenerateNumber(int minValue = 1, int maxValue = 100)
        {
            return ThreadRandom.Value.Next(minValue, maxValue);
        }

        /// <summary>
        /// 生成随机字符串
        /// </summary>
        /// <param name="length">字符串长度</param>
        /// <returns>随机字符串</returns>
        /// <remarks>
        /// A(65)~Z(90)
        /// a(97)~z(122)
        /// </remarks>
        public static string GenerateChars(int length = 4)
        {
            if (length == 0)
                return string.Empty;

            var chars = string.Empty;
            for (var i = 0; i < length; i++)
            {
                var num = GenerateNumber(65, 90);
                var ascii = Convert.ToChar(num).ToString();
                var isUpper = ThreadRandom.Value.Next(0, 2) == 1;
                chars += isUpper ? ascii : ascii.ToLower();
            }

            return chars;
        }

        /// <summary>
        /// 处理日志操作
        /// </summary>
        /// <param name="action">日志的操作</param>
        /// <param name="categoryName">日志配置的名称，必须和配置文件中categorySources节点里的Name的值一致</param>
        public static void ProcessLog(Action<string> action, string categoryName)
        {
            try
            {
                action?.Invoke(categoryName);
            }
            catch (Exception ex)
            {
                var errors = new List<Tuple<int, string, string>>();
                var tmp = ex;
                var index = 0;
                do
                {
                    errors.Add(Tuple.Create(++index, tmp.GetType().Name, tmp.Message.Replace(Environment.NewLine, string.Empty)));
                    ex = tmp.InnerException == null ? null : ex.InnerException;
                } while (ex != null);

                if (errors.Count > 0)
                {
                    Console.WriteLine($"执行完成，发生以下错误：{Environment.NewLine}");
                    errors.ForEach(p => Console.WriteLine($"{p.Item1.ToString().PadLeft(2, '0')}）类型：{p.Item2}；错误：{p.Item3}{Environment.NewLine}"));
                }
            }
        }

        /// <summary>
        /// 生成log对象
        /// </summary>
        /// <returns>log对象</returns>
        private static LogEntry GenerateLog()
        {
            var log = new LogEntry
            {
                TimeStamp = DateTime.Now,
                EventId = GenerateNumber(),
                Priority = GenerateNumber(maxValue: 10),
                Title = GenerateChars(),
                Message = GenerateChars(10)
            };

            log.Categories.Add(GenerateChars());
            log.Categories.Add(GenerateChars());

            return log;
        }

        /// <summary>
        /// 演示如何使用企业库的日志组件
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        /// 注意：
        /// 1）配置文件中所有的PublicKeyToken=null必须删除，原因未知，有待调查
        /// 2）Logger.Writer.Write方法的category参数必须和配置文件中categorySources节点里的Name的值一致
        /// 3）支持相对目录，设置配置文件中listeners节点里的fileName的值为相对路径，如：logs\debug.log
        /// ----------------------------------------------------------------------------------------------
        /// 配置为系统日志：
        /// 1）Win7及以后的系统需要管理员权限才能写日志到系统日志，相关设定在工程属性的安全性内设置
        /// ----------------------------------------------------------------------------------------------
        /// 配置为数据库：
        /// 1）脚本文件在解决方案的Documents目录下，cmd文件需要开发环境有安装sqlcmd组件
        /// 2）数据库默认名称为Logging，对于数据库名称是否可以自定义有待测试
        /// 3）可以运行于localdb，使用这种格式((localdb)\数据库名称)来访问
        /// 4）对于不同的数据库需要修改配置文件的连接字符串
        /// </remarks>
        static void Main(string[] args)
        {
            #region 初始化企业库组件
            var source = ConfigurationSourceFactory.Create();

            //设置db
            var dbFactory = new DatabaseProviderFactory(source);
            DatabaseFactory.SetDatabaseProviderFactory(dbFactory);

            //设置log
            var logFactory = new LogWriterFactory(source);
            //如果有db的设置，必须在调用此方法前设置db
            var logWriter = logFactory.Create();
            Logger.SetLogWriter(logWriter);

            //设置exception
            //var exceptionFactory = new ExceptionPolicyFactory(source);
            //var exceptionManager = exceptionFactory.CreateManager();
            //ExceptionPolicy.SetExceptionManager(exceptionManager);
            #endregion

            var log = GenerateLog();

            //写系统日志
            //Operator.ProcessLog((s) => Logger.Writer.Write(log, s), "EventLog");

            //写db日志
            //Operator.ProcessLog((s) => Logger.Writer.Write(log, s), "DbLog");

            //写XML，这里有问题，生成的xml文件格式不对，待解决
            //Operator.ProcessLog((s) => Logger.Writer.Write(log, s), "XmlLog");

            //文本日志
            ProcessLog((s) => Logger.Writer.Write(log, s), "FlatLog");
            //滚动的文本日志
            ProcessLog((s) => Logger.Writer.Write(log, s), "RollingLog");

            Console.WriteLine("主函数执行完成，按任意键退出……");
            Console.ReadKey();
        }
    }
}
