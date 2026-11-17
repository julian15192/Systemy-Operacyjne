using System;
using System.Threading;
using System.Diagnostics;

namespace Zadanie_6
{
    public sealed class ZegarSymulacji
    {
        private object stateLock = new object();
        private Stopwatch realClock = Stopwatch.StartNew();
        private double startReal = 0;
        private double startSim = 0;
        private double mulitplier = 1;
        private bool paused = false;

        private double Current()
        {
            if (paused)
            {
                return startSim;
            }
            else
            {
                double elapsedReal = realClock.ElapsedMilliseconds - startReal;
                return startSim + elapsedReal * mulitplier;
            }
        }

        private void NewStart()
        {
            startSim = Current();
            startReal = realClock.ElapsedMilliseconds;
        }

        public long Teraz
        {
            get
            {
                lock (stateLock)
                {
                    return (long)Current();
                }
            }
        }
        public double Mnoznik
        {
            get
            {
                lock (stateLock)
                {
                    return mulitplier;
                }
            }
            set
            {
                lock (stateLock)
                {
                    NewStart();
                    mulitplier = Math.Clamp(value, 1, 20);
                }
            }
        }
        public void Pauza()
        {
            lock (stateLock)
            {
                NewStart();
                paused = true;
            }
        }
        public void Wznow()
        {
            lock (stateLock)
            {
                NewStart();
                paused = false;
            }
        }
    }
    internal class Program
    {
        static ZegarSymulacji clock = new ZegarSymulacji();
        static volatile bool stop = false;
        static long reads = 0;
        static long backwards = 0;

        static void Reader()
        {
            long count = 0;
            long last = clock.Teraz;
            while (!stop)
            {
                long now = clock.Teraz;
                if (now < last)
                {
                    Interlocked.Increment(ref backwards);
                }
                last = now;
                count++;
            }
            Interlocked.Add(ref reads, count);
        }
        static void Main(string[] args)
        {
            Thread[] readers = new Thread[8];
            for (int i = 0; i < 8; i++)
            {
                readers[i] = new Thread(Reader);
                readers[i].Start();
            }

            Random random = new Random();
            bool isPaused = false;
            for (int i = 0; i < 33; i++)
            {
                Thread.Sleep(300);
                clock.Mnoznik = random.Next(1, 21);
                if (random.Next(0, 2) == 0)
                {
                    if (isPaused)
                    {
                        clock.Wznow();
                    }
                    else
                    {
                        clock.Pauza();
                    }
                    isPaused = !isPaused;
                }
            }

            stop = true;
            for (int i = 0; i < 8; i++)
            {
                readers[i].Join();
            }

            Console.WriteLine($"Reads: {reads}");
            Console.WriteLine($"Time went backwards: {backwards} times");
            Console.WriteLine($"Simulation time at the end: {clock.Teraz} ms");
        }
    }
}
