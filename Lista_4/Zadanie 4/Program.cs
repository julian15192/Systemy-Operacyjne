using System;
using System.Threading;

namespace Zadanie_4
{
    internal class Program
    {
        static int version = 1;
        static string[] names = { "N", "E", "S", "W" };
        static object[] resources = new object[4];
        static int[] held = new int[4];
        static int[] wanted = new int[4];
        static int finishedCars = 0;
        static int retries = 0;

        static void Car(object? id)
        {
            int d = (int)id!;
            int first = d;
            int second = (d + 1) % 4;
            Random random = new Random();

            if (version == 2 && second < first)
            {
                first = second;
                second = d;
            }

            if (version != 3)
            {
                Monitor.Enter(resources[first]);
                held[d] = first;
                Thread.Sleep(50);
                wanted[d] = second;
                Monitor.Enter(resources[second]);
            }
            else
            {
                while (true)
                {
                    Monitor.Enter(resources[first]);
                    held[d] = first;
                    Thread.Sleep(50);
                    wanted[d] = second;
                    if (Monitor.TryEnter(resources[second], 100))
                    {
                        break;
                    }
                    else
                    {
                        Monitor.Exit(resources[first]);
                        Interlocked.Increment(ref retries);
                        Thread.Sleep(random.Next(0, 100));
                    }
                }
            }
            wanted[d] = -1;
            Thread.Sleep(20);
            Monitor.Exit(resources[second]);
            Monitor.Exit(resources[first]);
            Interlocked.Increment(ref finishedCars);
        }
        static void Watchdog()
        {
            int last = 0;
            while (finishedCars < 4)
            {
                Thread.Sleep(2000);
                if (finishedCars == last)
                {
                    Console.WriteLine($"Deadlock! No progress for 2 s:");
                    for (int d = 0; d < 4; d++)
                    {
                        if (wanted[d] != -1)
                        {
                            Console.WriteLine($"Car {names[d]} holds {names[held[d]]} and waits for {names[wanted[d]]}");
                        }
                    }
                    return;
                }
                last = finishedCars;
            }
            Console.WriteLine($"All cars turned left.");
        }
        static void Main(string[] args)
        {
            for (int i = 0; i < 4; i++)
            {
                resources[i] = new object();
                wanted[i] = -1;
            }

            Thread watchdog = new Thread(Watchdog);
            watchdog.Start();

            Thread[] cars = new Thread[4];
            for (int i = 0; i < 4; i++)
            {
                cars[i] = new Thread(Car);
                cars[i].IsBackground = true;
                cars[i].Start(i);
            }

            watchdog.Join();
            Console.WriteLine($"Version {version}: finished cars = {finishedCars}, retries: {retries}");
        }
    }
}
