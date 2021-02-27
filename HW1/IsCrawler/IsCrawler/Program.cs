using System;

namespace IsCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Fill in link:");

            var crawler = new Crawler.Crawler();
            string lnk = Console.ReadLine();
            crawler.GetCrawl(lnk);

            Console.WriteLine();
        }
    }
}
