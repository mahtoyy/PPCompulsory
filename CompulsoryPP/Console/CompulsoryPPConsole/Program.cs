using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeGenerator
{
    class Generator
    {
        static SemaphoreSlim s = new SemaphoreSlim(1, 1);
        static Mutex mutex = new Mutex();
        static readonly object theLock = new object();
        static int seed = 1234;
        static void Main(string[] args)
        {
            while (true)
            {
                var stopWatch = new Stopwatch();
                Console.Write("LowerLimit: ");
                string lower = Console.ReadLine();
                Console.Write("UpperLimit: ");
                string upper = Console.ReadLine();
                ulong upperLimit = Convert.ToUInt64(upper);
                ulong lowerLimit = Convert.ToUInt64(lower);

                List<ulong> list = new List<ulong>();

                if (upperLimit <= 1 && lowerLimit < 0 && upperLimit >= lowerLimit)
                {
                    Console.WriteLine("There are no primes below 2...");
                    Console.ReadKey();
                    return;
                }

                Console.WriteLine("Sequential");

                ShowTimeInSeconds(() => GeneratePrime(lowerLimit, upperLimit));

                Console.WriteLine("Lock");

                ShowTimeInSeconds(() => GenerateParallelLock(lowerLimit, upperLimit));

                Console.WriteLine("Mutex");

                ShowTimeInSeconds(() => GenerateParallelSemaphore(lowerLimit, upperLimit));

                Console.WriteLine("Semaphore");

                ShowTimeInSeconds(() => GenerateParallelSemaphore(lowerLimit, upperLimit));

                Console.WriteLine("");
            }
            Console.ReadKey();
        }

        private static void ShowTimeInSeconds(Action p)
        {
            Stopwatch sw = Stopwatch.StartNew();
            p.Invoke();
            sw.Stop();
            Console.WriteLine("Time = {0:f5} seconds", sw.ElapsedMilliseconds / 1000d);
        }



        public static bool IsPrime(ulong number)
        {
            if (number == 1) return true;

            if (number % 2 == 0 && number > 2) return false;

            var boundary = (ulong)Math.Floor(Math.Sqrt(number));

            for (ulong i = 3; i <= boundary; i += 2)
                if (number % i == 0)
                    return false;

            return true;
        }

        static List<ulong> GenerateParallelLock(ulong min, ulong max)
        {
            var list = new List<ulong>();
            Parallel.ForEach(Partitioner.Create((int)min, (int)max), (range,loopState) =>
            {
                Random rnd = new Random(Interlocked.Increment(ref seed));
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    if (IsPrime((ulong)i))
                    {
                        lock (theLock)
                        {
                            list.Add((ulong)i);
                        }
                    }
                }
            });
            list.Sort();
            return list;
        }

        static List<ulong> GenerateParallelMutex(ulong min, ulong max)
        {
            List<ulong> list = new List<ulong>();
            Parallel.ForEach(Partitioner.Create((int)min, (int)max), (range, loopState) =>
            {
                Random rnd = new Random(Interlocked.Increment(ref seed));
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    if (IsPrime((ulong)i))
                    {
                        mutex.WaitOne();
                        list.Add((ulong)i);
                        mutex.ReleaseMutex();
                    }
                }
            });
            list.Sort();
            return list;
        }

        static List<ulong> GenerateParallelSemaphore(ulong min, ulong max)
        {
            List<ulong> list = new List<ulong> ();
            Parallel.ForEach(Partitioner.Create((int)min, (int)max), (range, loopState) =>
            {
                Random rnd = new Random(Interlocked.Increment(ref seed));
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    if (IsPrime((ulong)i))
                    {
                        s.Wait();
                        list.Add((ulong)i);
                        s.Release();
                    }
                }
            });
            list.Sort ();
            return list;
        }


        static List<ulong> GeneratePrime(ulong min, ulong max)
        {
            List<ulong> list = new List<ulong> ();
            for (ulong i = min; i <= max; i++)
            {
                if (IsPrime(i))
                    list.Add(i);

            }
            list.Sort();
            return list;
        }
    }
}
