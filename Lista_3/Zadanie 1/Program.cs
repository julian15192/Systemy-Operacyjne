using System;
using System.Threading;
using System.IO;

namespace Zadanie_1
{
    internal class Program
    {
        static int[] numbers = new int[100];
        static void Increase()
        {
            Random random = new Random();
            for (int i = 0; i < 10000; i++)
            {
                numbers[random.Next(0, 100)]++;
            }
        }
        static void Decrease()
        {
            Random random = new Random();
            for (int i = 0; i < 10000; i++)
            {
                numbers[random.Next(0, 100)]--;
            }
        }
        static void Main(string[] args)
        {
            Thread p1 = new Thread(Increase);
            Thread p2 = new Thread(Decrease);

            p1.Start();
            p2.Start();

            p1.Join();
            p2.Join();

            int sum = 0;
            for (int i = 0; i < 100; i++)
            {
                Console.WriteLine($"{numbers[i]}");
                sum += numbers[i];
            }
            Console.WriteLine($"Średnia wartość {sum / 100.0}");

            // Eksport danych do pliku csv
            StreamWriter file = new StreamWriter("wyniki.csv");
            for (int i = 0; i < 100; i++)
            {
                file.WriteLine($"{numbers[i]}");
            }
            file.Close();
        }
    }
}
