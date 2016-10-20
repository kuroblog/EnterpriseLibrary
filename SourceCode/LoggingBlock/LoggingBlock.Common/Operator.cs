
namespace LoggingBlock.Common
{
    using System;
    using System.Collections.Generic;
    using System.Threading;

    public class Operator
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

            Console.WriteLine("主函数执行完成，按任意键退出……");
            Console.ReadKey();
        }
    }
}
