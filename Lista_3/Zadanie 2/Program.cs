using System;
using System.Threading;

namespace Zadanie_2
{
    internal class Program
    {
        static int adresat = 0;
        static string wiadomosc = "Wiadomość testowa";
        static void Proces(object? id)
        {
            int numerId = (int)id!;
            Random random = new Random();
            while (true)
            {
                if (numerId == adresat)
                {
                    Console.WriteLine($"Proces {numerId} otrzymał wiadomość od procesu {wiadomosc}");
                    wiadomosc = numerId.ToString();
                    adresat = random.Next(0, 6);
                }
                Thread.Sleep(random.Next(10, 31));
            }
        }
        static void Main(string[] args)
        {
            Thread[] processes = new Thread[6];
            for (int i = 0; i < 6; i++)
            {
                processes[i] = new Thread(Proces);
                processes[i].Start(i);
            }
        }
    }
}
