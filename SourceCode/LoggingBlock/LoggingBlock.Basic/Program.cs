
namespace LoggingBlock.Basic
{
    using Common;
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using System;

    class Program
    {
        /// <summary>
        /// 生成log对象
        /// </summary>
        /// <returns>log对象</returns>
        private static LogEntry GenerateLog()
        {
            var log = new LogEntry
            {
                TimeStamp = DateTime.Now,
                EventId = Operator.GenerateNumber(),
                Priority = Operator.GenerateNumber(maxValue: 10),
                Title = Operator.GenerateChars(),
                Message = Operator.GenerateChars(10)
            };

            log.Categories.Add(Operator.GenerateChars());
            log.Categories.Add(Operator.GenerateChars());

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

            Operator.ProcessLog((s) => Logger.Writer.Write(log, s), "FlatLog");
            Operator.ProcessLog((s) => Logger.Writer.Write(log, s), "RollingLog");

            Console.WriteLine("主函数执行完成，按任意键退出……");
            Console.ReadKey();
        }
    }
}
