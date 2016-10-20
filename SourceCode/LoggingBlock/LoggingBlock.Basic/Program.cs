
namespace LoggingBlock.Basic
{
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using System;
    using System.Collections.Generic;

    class Program
    {
        private static void DoTest(Action action)
        {
            try
            {
                var configSource = ConfigurationSourceFactory.Create();
                var logFactory = new LogWriterFactory(configSource);
                Logger.SetLogWriter(logFactory.Create());

                action?.Invoke();
            }
            catch (Exception ex)
            {
                var errors = new List<Tuple<int, string, string>>();
                var tmp = ex;
                var index = 0;
                do
                {
                    errors.Insert(0, Tuple.Create(++index, tmp.GetType().Name, tmp.Message.Replace(Environment.NewLine, string.Empty)));
                    tmp = ex.InnerException == null ? null : ex.InnerException;
                } while (tmp != null);

                if (errors.Count > 0)
                {
                    Console.WriteLine("执行完成，发生以下错误：");
                    errors.ForEach(p => Console.WriteLine($"{p.Item1.ToString().PadLeft(2, '0')}）类型：{p.Item2}；错误：{p.Item3}"));
                }
            }

            Console.WriteLine("主函数执行完成，按任意键退出……");
            Console.ReadKey();
        }

        /// <summary>
        /// 默认的日志配置，写日志到系统日志
        /// </summary>
        /// <remarks>
        /// Win7及以后的系统需要管理员权限才能写日志到系统日志，相关设定在工程属性的安全性内设置
        /// </remarks>
        private static void WriteToSystemEvennt()
        {
            var log = new LogEntry
            {
                EventId = 1,
                Priority = 1,
                Title = "标题",
                Message = "测试消息"
            };

            log.Categories.Add("测试");
            log.Categories.Add("调试");

            Logger.Writer.Write(log, "General");
        }

        static void Main(string[] args)
        {
            DoTest(WriteToSystemEvennt);
        }
    }
}
