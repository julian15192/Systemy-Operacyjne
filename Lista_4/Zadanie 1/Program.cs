using System;
using System.Threading;
using System.Diagnostics;

namespace Zadanie_1
{
    internal class Program
    {
        static void Auto()
        {
            string? nazwa = Thread.CurrentThread.Name;
            int id = Thread.CurrentThread.ManagedThreadId;
            Console.WriteLine($"{nazwa} startuje, ManagedThreadID = {id}");

            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"{nazwa}: krok {i + 1}");
                Thread.Sleep(10);
            }
        }

        static void Main(string[] args)
        {
            int n = 20;
            if (args.Length > 0)
            {
                n = int.Parse(args[0]);
            }

            Stopwatch czas = Stopwatch.StartNew();
            Thread[] auta = new Thread[n];
            for (int i = 0; i < n; i++)
            {
                auta[i] = new Thread(Auto);
                if (i < 9)
                {
                    auta[i].Name = $"Auto-0{i + 1}";
                }
                else
                {
                    auta[i].Name = $"Auto-{i + 1}";
                };
                // auta[i].IsBackground = true;
                auta[i].Start();
            }

            /**/
            for (int i = 0; i < n; i++)
            {
                auta[i].Join();
            }
            /**/

            czas.Stop();
            Console.WriteLine($"Łączny czas: {czas.ElapsedMilliseconds} ms");
        }
    }
}
