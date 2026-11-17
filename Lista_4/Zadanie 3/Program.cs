using System;
using System.Threading;

namespace Zadanie_3
{
    internal class Program
    {
        static int version = 1;
        static object[] segments = new object[20];
        static int[] position = new int[10];
        static object stateLock = new object();
        static int checks = 0;
        static int violations = 0;
        static volatile bool finished = false;

        static void SetPosition(int number, int segment)
        {
            lock (stateLock)
            {
                position[number] = segment;
            }
        }

        static void Car(object? id)
        {
            int number = (int)id!;
            Random random = new Random();
            int current = 0;

            Monitor.Enter(segments[0]);
            try
            {
                for (int next = 1; next < 20; next++)
                {
                    if (version == 1)
                    {
                        Monitor.Enter(segments[next]);
                        SetPosition(number, next);
                        Monitor.Exit(segments[current]);
                        current = next;
                    }
                    else
                    {
                        Monitor.Exit(segments[current]);
                        current = -1;
                        Monitor.Enter(segments[next]);
                        current = next;
                        SetPosition(number, next);
                    }
                    Thread.Sleep(random.Next(5, 31));
                }

            }
            finally
            {
                if (current >= 0)
                {
                    Monitor.Exit(segments[current]);
                }
                SetPosition(number, 20);
            }
        }
        static void Controller()
        {
            while (!finished)
            {
                lock (stateLock)
                {
                    for (int k = 1; k < 10; k++)
                    {
                        if (position[k] != -1 && position[k-1] != 20 && position[k] >= position[k - 1])
                        {
                            violations++;
                        }
                    }
                }
                Thread.Sleep(10);
            }
        }
               
        
        static void Main(string[] args)
        {
            for (int i = 0; i < 20; i++)
            {
                segments[i] = new object();
            }
            for (int i = 0; i < 10; i++)
            {
                position[i] = -1;
            }
            Thread controller = new Thread(Controller);
            controller.Start();

            Thread[] cars = new Thread[10];
            for (int i = 0; i < 10; i++)
            {
                cars[i] = new Thread(Car);
                cars[i].Start(i);
                Thread.Sleep(20);
            }
            for (int i = 0; i < 10; i++)
            {
                cars[i].Join();
            }
            finished = true;
            controller.Join();

            Console.WriteLine($"Version {version}, Violations: {violations}");
        }
    }
}
