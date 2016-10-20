
namespace LoggingBlock.EventLog
{
    using Common;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    class Program
    {
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

            //这里的第二个参数的值需要和配置文件的General的Name设置的一样
            Logger.Writer.Write(log, "EventLog");
        }

        static void Main(string[] args)
        {
            Operator.ProcessLog(WriteToSystemEvennt);
        }
    }
}
