using System;
using System.Threading;
using System.Diagnostics;

namespace Zadanie_5
{
    internal class Program
    {
        static int version = 1;
        static volatile bool paused = false;
        static volatile bool stop = false;
        static object pauseLock = new object();
        static double[] delay = new double[16];
        static Stopwatch clock = Stopwatch.StartNew();
        static double resumeTime = 0;

        static void Worker(object? id)
        {
            int number = (int)id!;
            while (!stop)
            {
                if (paused)
                {
                    if (version == 1)
                    {
                        while (paused) { }
                    }
                    else if (version == 2)
                    {
                        while (paused)
                        {
                            Thread.Sleep(1);
                        }
                    }
                    else
                    {
                        lock (pauseLock)
                        {
                            if (version == 3)
                            {
                                while (paused)
                                {
                                    Monitor.Wait(pauseLock);
                                }
                            }
                            else
                            {
                                if (paused)
                                {
                                    Monitor.Wait(pauseLock);
                                }
                            }
                        }
                    }

                    if (paused)
                    {
                        Console.WriteLine($"Worker {number} woke up although the pause is still on!");
                    }
                    else
                    {
                        delay[number] = clock.Elapsed.TotalMilliseconds - resumeTime;
                    }
                }
                Thread.SpinWait(100);
            }
        }
        static void Main(string[] args)
        {
            Thread[] workers = new Thread[16];
            for (int i = 0; i < 16; i++)
            {
                workers[i] = new Thread(Worker);
                workers[i].IsBackground = true;
                workers[i].Start(i);
            }
            Thread.Sleep(1000);

            paused = true;
            Thread.Sleep(100);
            TimeSpan cpuBefore = Process.GetCurrentProcess().TotalProcessorTime;

            Thread.Sleep(2500);
            lock (pauseLock)
            {
                Monitor.PulseAll(pauseLock);
            }
            Thread.Sleep(2500);

            TimeSpan cpuAfter = Process.GetCurrentProcess().TotalProcessorTime;

            resumeTime = clock.Elapsed.TotalMilliseconds;
            lock (pauseLock)
            {
                paused = false;
                Monitor.PulseAll(pauseLock);
            }
            Thread.Sleep(500);

            stop = true;
            int stuck = 0;
            for (int i = 0; i < 16; i++)
            {
                if (!workers[i].Join(2000))
                {
                    stuck++;
                }
            }

            double maxDelay = 0;
            for (int i = 0; i < 16; i++)
            {
                if (delay[i] > maxDelay)
                {
                    maxDelay = delay[i];
                }
            }
            Console.WriteLine($"Version {version}");
            Console.WriteLine($"CPU time during the 5 s pause: {(cpuAfter - cpuBefore).TotalMilliseconds:F0} ms");
            Console.WriteLine($"Longest reaction to the resume: {maxDelay:F2} ms");
            Console.WriteLine($"Workers that did not see the resume: {stuck}");
        }
    }
}
