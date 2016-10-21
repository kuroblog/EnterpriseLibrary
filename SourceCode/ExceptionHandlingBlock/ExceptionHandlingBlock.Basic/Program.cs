
namespace ExceptionHandlingBlock.Basic
{
    using Microsoft.Practices.EnterpriseLibrary.Common.Configuration;
    using Microsoft.Practices.EnterpriseLibrary.ExceptionHandling;
    using System;
    using System.Collections.Generic;

    class Program
    {
        private static bool ProcessException(ExceptionManager exceptionManager, Action action)
        {
            try
            {
                exceptionManager.Process(() =>
                {
                    action?.Invoke();
                    Console.WriteLine("此处不该被打印，应为该方法发生了异常");
                }, "Policy");

                return false;
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

                return true;
            }
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

            var isException = false;

            isException = ProcessException(exceptionManager, () => int.Parse("A"));
            Console.WriteLine("主函数执行完成，{0}异常发生，按任意键退出……", isException ? "有" : "无");
            Console.ReadKey();
        }
    }
}
