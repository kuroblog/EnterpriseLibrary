
namespace ConfigurationBlock.Basic
{
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.Data;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using System;
    using System.Collections.Generic;
    using System.Configuration;

    class Program
    {
        private static bool ProcessException(ExceptionManager exceptionManager, Action action)
        {
            var isException = true;

            try
            {
                exceptionManager.Process(() =>
                {
                    action?.Invoke();
                    Console.WriteLine("此处不该被打印，应为该方法发生了异常");
                }, "ExceptionPolicies");

                isException = false;
            }
            catch (Exception ex)
            {
                var errors = new List<Tuple<int, string, string>>();
                var tmp = ex;
                var index = 0;
                do
                {
                    errors.Add(Tuple.Create(++index, ex.GetType().Name, ex.Message.Replace(Environment.NewLine, string.Empty)));
                    ex = tmp.InnerException == null ? null : ex.InnerException;
                } while (ex != null);

                if (errors.Count > 0)
                {
                    Console.WriteLine($"执行完成，发生以下错误：{Environment.NewLine}");
                    errors.ForEach(p => Console.WriteLine($"{p.Item1.ToString().PadLeft(2, '0')}）异常类型:{p.Item2};异常信息:{p.Item3}{Environment.NewLine}"));
                }
            }

            return isException;
        }

        static void Main(string[] args)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var section = config.GetSection(ConfigurationSourceSection.SectionName) as ConfigurationSourceSection;

            var dbConfigSourceElement = section.Sources.Get("DatabaseBlockSettings") as FileConfigurationSourceElement;
            var dbConfigSource = new FileConfigurationSource(dbConfigSourceElement.FilePath);
            var dbFactory = new DatabaseProviderFactory(dbConfigSource);
            DatabaseFactory.SetDatabaseProviderFactory(dbFactory);

            var logConfigSourceElement = section.Sources.Get("LoggingBlockSettings") as FileConfigurationSourceElement;
            var logConfigSource = new FileConfigurationSource(logConfigSourceElement.FilePath);
            var logFactory = new LogWriterFactory(logConfigSource);
            var logWriter = logFactory.Create();
            Logger.SetLogWriter(logWriter);
            logWriter.Write("123333");

            var exceptionConfigSourceElement = section.Sources.Get("ExceptionHandlingBlockSettings") as FileConfigurationSourceElement;
            var exceptionConfigSource = new FileConfigurationSource(exceptionConfigSourceElement.FilePath);
            var exceptionFactory = new ExceptionPolicyFactory(exceptionConfigSource);
            var exceptionManager = exceptionFactory.CreateManager();
            ExceptionPolicy.SetExceptionManager(exceptionManager);

            ProcessException(exceptionManager, () => int.Parse("A"));

            Console.WriteLine("主函数执行完成，按任意键退出……");
            Console.ReadKey();
        }
    }
}
