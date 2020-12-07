using System;
using System.IO;


namespace WebScrape
{
    class Program
    {
         static void Main(string[] args)
        {
            

            string pathOutputFile;

            //ask for path to output directory
            while (true) {
                Console.WriteLine("Enter path to output directory:");
                pathOutputFile = Console.ReadLine();
                if (Directory.Exists(pathOutputFile))
                {
                    Console.WriteLine("Path is good, webscraping  is started..");
                    break;
                }
                else continue;
            }
            //if path is good, execute scraping
            WebScrapeManager manager = new WebScrapeManager(pathOutputFile);
            manager.execute();


            Console.Read();

        }
    }
}
