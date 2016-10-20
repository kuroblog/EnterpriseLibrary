
namespace LoggingBlock.Basic
{
    using Microsoft.Practices.EnterpriseLibrary.Logging;
    using System;
    using System.Collections.Generic;

    class Program
    {
        static void Main(string[] args)
        {
            DoTest(() =>
            {
                var i = 0;

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
            });
        }

        private static void DoTest(Action action)
        {
            try
            {
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
    }
}
