using System;
using System.Threading;

namespace Zadanie_7
{
    internal class Program
    {
        static int version = 1;
        static object[] segments = new object[5];
        static volatile bool stop = false;
        static int passes = 0;
        static int retries = 0;


        static void Car()
        {
            Random random = new Random();
            while (!stop)
            {
                int start = random.Next(0, 5);
                int count = random.Next(1, 5);
                int[] needed = new int[count];
                for (int i = 0; i < count; i++)
                {
                    needed[i] = (start + i) % 5;
                }

                if (version == 1)
                {
                    Array.Sort(needed);
                    for (int i = 0; i < count; i++)
                    {
                        Monitor.Enter(segments[needed[i]]);
                    }
                }
                else
                {
                    while (true)
                    {
                        int taken = 0;
                        for (int i = 0; i < count; i++)
                        {
                            if (!Monitor.TryEnter(segments[needed[i]], 10))
                            {
                                break;
                            }
                            taken++;
                        }
                        if (taken == count)
                        {
                            break;
                        }
                        for (int i = 0; i < taken; i++)
                        {
                            Monitor.Exit(segments[needed[i]]);
                        }
                        Interlocked.Increment(ref retries);
                        Thread.Sleep(random.Next(0, 10));
                    }
                }
                Thread.Sleep(10);
                for (int i = 0; i < count; i++)
                {
                    Monitor.Exit(segments[needed[i]]);
                }
                Interlocked.Increment(ref passes);
            }
        }
        static void Main(string[] args)
        {
            for (int i = 0; i < 5; i++)
            {
                segments[i] = new object();
            }

            Thread[] cars = new Thread[10];
            for (int i = 0; i < 10; i++)
            {
                cars[i] = new Thread(Car);
                cars[i].Start();
            }

            Thread.Sleep(5000);
            stop = true;
            for (int i = 0; i < 10; i++)
            {
                cars[i].Join();
            }

            Console.WriteLine($"Version {version}: passes = {passes}, throughput = {passes / 5.0:F1} passes/s, retries = {retries}");
        }
    }
}
