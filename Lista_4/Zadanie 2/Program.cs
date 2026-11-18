using System;
using System.Threading;
using System.Diagnostics;

namespace Zadanie_2
{
    internal class Program
    {    
        static int counter = 0;
        static object lockObject = new object();
        static int version = 1;

        static void IncreaseCounter()
        {
            switch (version)
            {
                case 1:
                    for (int i = 0; i < 1000000; i++)
                    {
                        counter++;
                    }
                    break;
                case 2:
                    for (int i = 0; i < 1000000; i++)
                    {
                        lock (lockObject)
                        {
                            counter++;
                        }
                    }
                    break;
                case 3:
                    for (int i = 0; i < 1000000; i++)
                    {
                        Interlocked.Increment(ref counter);
                    }
                    break;
            }
        }
        static void Main(string[] args)
        {
            string[] names = { "no synchronization", "lock", "Interlocked.Increment" };

            Stopwatch czas = Stopwatch.StartNew();
            Thread[] threads = new Thread[8];
            for (int i = 0; i < 8; i++)
            {
                threads[i] = new Thread(IncreaseCounter);
                threads[i].Start();
            }

            for (int i = 0; i < 8; i++)
            {
                threads[i].Join();
            }
            czas.Stop();
            Console.WriteLine($"{names[version - 1]}: counter = {counter}, time = {czas.ElapsedMilliseconds} ms");
        }
    }
}