using System;
using System.Data;


namespace WebScrape
{
    class WebScrapeManager
    {
        private BankWebScrape bankWebScrape;
        private IOutputFile outputFile;
        private DateTime startDate;
        private DateTime endDate;


        public WebScrapeManager(string pathOutputFile){
            bankWebScrape = new BankWebScrape();
            outputFile = new OutputTxtFile(pathOutputFile);
            startDate = DateTime.Now.AddDays(-2);
            endDate = DateTime.Now;
        }


        //triger all web scraping logic
        public void execute() {

            //method LoadCurrencyList() returns true if currency list is loaded successfully from web page
            //so if the list is loaded successfully, it can start web scraping of the data
            if (bankWebScrape.LoadCurrencyList())
            {
                 ScrapeTableData(); 
            }
            else Console.WriteLine("Please start aplication again.");

        }//end method


        //for every currency read and write data in output file
        private void ScrapeTableData() {

            //iterate through currency list and read HTML table 
            foreach (string currency in bankWebScrape.Currencies)
            {
                DataTable table;


                //scrape table from the web for provided arguments 
                if ((table=bankWebScrape.ReadAllPages( currency, startDate, endDate))!=null)
                {
                    //add table name
                    table.TableName = currency + startDate.ToString("yyyy-MM-dd") + endDate.ToString("yyyy-MM-dd");

                    //write in output file
                    WriteInOutputFile(table);

                }else Console.WriteLine("Output file for currency "+currency+" is not created!");
            }
        }//end method


        private void WriteInOutputFile(DataTable table) {
            //method write returns true if creating and writing in file is successfully
            if (outputFile.Write(table, ","))
                Console.WriteLine("File " + table.TableName + " is successfully created!");     //write message on Console
            else Console.WriteLine("Error while writing in file " + table.TableName + "!");
        }

    }
}
