
namespace ExceptionHandlingBlock.Basic
{
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;

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
                }, "Policy");

                isException = true;
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
            #region 初始化企业库组件
            var source = ConfigurationSourceFactory.Create();

            //设置exception
            var exceptionFactory = new ExceptionPolicyFactory(source);
            var exceptionManager = exceptionFactory.CreateManager();
            ExceptionPolicy.SetExceptionManager(exceptionManager);
            #endregion

            var exceptions = new List<bool>();

            exceptions.Add(ProcessException(exceptionManager, () => int.Parse("A")));
            exceptions.Add(ProcessException(exceptionManager, () => File.Open("tmp.txt", FileMode.Open)));
            exceptions.Add(ProcessException(exceptionManager, () => new Hashtable()["tmp"].ToString()));

            Console.WriteLine("主函数执行完成，{0}异常发生，按任意键退出……", exceptions.Count > 0 ? "有" : "无");
            Console.ReadKey();
        }
    }
}
