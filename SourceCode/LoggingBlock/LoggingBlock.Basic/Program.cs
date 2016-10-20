
namespace LoggingBlock.Basic
{
    using Common;
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using Microsoft.Practices.EnterpriseLibrary.Logging;

    class Program
    {
        private static LogEntry GenerateLog()
        {
            var log = new LogEntry
            {
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
        /// 默认的日志配置，写日志到系统日志
        /// </summary>
        /// <remarks>
        /// Win7及以后的系统需要管理员权限才能写日志到系统日志，相关设定在工程属性的安全性内设置
        /// </remarks>
        private static void WriteToEventLog()
        {
            //这里的第二个参数的值需要和配置文件中Logging Settings中Categories中Name的值一致
            Logger.Writer.Write(GenerateLog(), "EventLog");
        }

        ///// <summary>
        ///// 配置为数据库，写日志到数据库
        ///// </summary>
        ///// <remarks>
        ///// 1）脚本文件在解决方案的Documents目录下
        ///// 2）数据库默认名称为Logging
        ///// 3）可以运行于localdb
        ///// 4）对于不同的数据库需要修改配置文件的连接字符串
        ///// </remarks>
        //private static void WriteToDbLog(string categoryName)
        //{
        //    //这里的第二个参数的值需要和配置文件中Logging Settings中Categories中Name的值一致
        //    Logger.Writer.Write(GenerateLog(), );
        //}


        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        /// <remarks>
        /// 
        /// </remarks>
        static void Main(string[] args)
        {
            var source = ConfigurationSourceFactory.Create();

            var dbFactory = new DatabaseProviderFactory(source);
            DatabaseFactory.SetDatabaseProviderFactory(dbFactory);

            var logFactory = new LogWriterFactory(source);
            var logWriter = logFactory.Create();

            Logger.SetLogWriter(logWriter);

            //Operator.ProcessLog(WriteToEventLog);


            Operator.ProcessLog((s) => Logger.Writer.Write(GenerateLog(), s), "DbLog");
        }
    }
}
