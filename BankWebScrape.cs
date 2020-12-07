using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;


using HtmlAgilityPack;
using RestSharp;

namespace WebScrape
{


    //provide methods for scraping data from https://srh.bankofchina.com/search/whpj/searchen.jsp
    class BankWebScrape
    {
        private RestClient client;
        private RestRequest request;
        private HtmlDocument document;
        public List<string> Currencies { get; }

        public BankWebScrape() {
            this.client = new RestClient("https://srh.bankofchina.com/search/whpj/searchen.jsp");
            this.document = new HtmlDocument();
            Currencies = new List<string>();
        }

        //create post request 
        private void CreatePostFormRequest() {
            request = new RestRequest("", Method.POST);
            client.CookieContainer = new CookieContainer();
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
        }//end method

        //create get request 
        private void CreateGetRequest() {
            request = new RestRequest("", Method.GET);
        }//end method


        //submit the request
        private void SubmitRequest() {
            var page = client.Execute(request);
            document.LoadHtml(page.Content);
        }//end method


        //fill all the elements of HTML form 
        private void FillForm(string currency, DateTime startDate, DateTime endDate, int pageNumber)
        {
            //start date
            request.AddParameter("erectDate", startDate.ToString("yyyy-MM-dd"), ParameterType.GetOrPost);
            //end date
            request.AddParameter("nothing", endDate.ToString("yyyy-MM-dd"), ParameterType.GetOrPost);
            //provide currency
            request.AddParameter("pjname", currency, ParameterType.GetOrPost);
            //provide page number
            request.AddParameter("page", pageNumber, ParameterType.GetOrPost);
        }//end method


        //submit all form  elements
        public void SubmitForm(string currency, DateTime startDate, DateTime endDate, int pageNumber) {

            //create post request
            CreatePostFormRequest();

            //fill form elements
            FillForm(currency, startDate, endDate, pageNumber);

            //submit request
            SubmitRequest();
    
        }//end method

        //insert values from HTML Selection in Currencies list
        private void ReadCurrencySelection() {
                //identify HTMLSelect element
                HtmlNodeCollection currenciesNode = document.DocumentNode.SelectNodes("//option");

                //itterate through Collection and add each HTMLOption to Currency list as text
                foreach (HtmlNode node in currenciesNode)
                {
                    Currencies.Add(node.InnerText.Replace("\r\n", "").Replace("\t", ""));
                }
                //remove the first one, it is not currency
                Currencies.RemoveAt(0);

        }

        //load currency list from HTML Select Elements
        public bool LoadCurrencyList() {
            try
            {
                //create get request and submit
                CreateGetRequest();
                SubmitRequest();

                //load all option elements from HTMLSelect in class variable currencies
                ReadCurrencySelection();
            }
            catch (System.NullReferenceException)
            {
                Console.WriteLine("Error while reading currency list from the page");
                return false;
            }

            return true;
        }//end method



        //insert values from Web Table in first parameter table
        //second argument is option for header row reading
        public bool ReadHTMLTable(DataTable table, bool header)  {
            try
            {
                //find HTML table 
                HtmlNodeCollection tableHTML = document.DocumentNode.SelectNodes("/html/body/table[2]//tr[td]");


                //this html table doesn't have defined header row in HTML code
                //so first row of 'rows' is header
                //read header if the parameter header is true
                if (header) {
                    var headerRow = tableHTML.First();
                    foreach (var element in headerRow.SelectNodes("td"))
                        table.Columns.Add(element.InnerText);
                }

                //remove header row, it is either read or should be skipped
                tableHTML.Remove(0);     

                //iterate through rows of HTML table
                foreach (var rowHTML in tableHTML)
                {

                    List<string> lista = new List<string>();
                    
                    //iterate through columns 
                    foreach (var cell in rowHTML.SelectNodes("td"))
                    {
                        lista.Add(cell.InnerText);
                    }
                    
                    table.Rows.Add(lista.ToArray());    //add in table Rows

                }
                return true;            
            }catch (ArgumentException)
            {
                Console.WriteLine("Error while reading data, argument is not valid.");
                return false;
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("Error while reading data, not possible to cast data");
                return false;
            }
            catch (System.Xml.XPath.XPathException)
            {
                Console.WriteLine("Webpage table is not found.");
                return false;
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("Webpage table is not loaded.");
                return false;
            }

        }//end method


        //read page records
        private int ReadPageRecords() {
            try
            {                
                int records;
                //it is read from JS code, so find variable that holds the required value 
                string subStr = document.ParsedText.Substring(document.ParsedText.IndexOf("m_nRecordCount = ") + "m_nRecordCount = ".Count());
                //and convert to int
                records = Convert.ToInt32(subStr.Substring(0, subStr.IndexOf(';')).ToString());
                return records;
            }
            catch (FormatException) {
                Console.WriteLine("Error while reading page number.");
                return 0;
            }
        }

        //read page table size
        private int ReadTableSize()
        {
            try
            {
                int size;
                //it is read from JS code, so find variable that holds the required value 
                string subStr = document.ParsedText.Substring(document.ParsedText.IndexOf("m_nPageSize = ") + "m_nPageSize = ".Count());
                //and convert to int, this throw FormatException
                size = Convert.ToInt32(subStr.Substring(0, subStr.IndexOf(';')).ToString());
                return size;
            }
            catch (FormatException)
            {
                Console.WriteLine("Error while reading page number.");
                return 0;
            }
        }

        //check if table is empty
        private bool IsTableEmpty() {
            //if table is empty, it contains this text
            return document.ParsedText.Contains("sorry, no records");
        }

        
        //read all table pages into first parameter table
        public DataTable ReadAllPages( string currency, DateTime startDate, DateTime endDate) {
            int records;
            int sizeTable;
            bool header = true;
            DataTable table = new DataTable();

            //post request (with all form elements) for the first page
            SubmitForm(currency, startDate, endDate, 1);

            //now we can:
            //check if HTMLtable is empty
            if (IsTableEmpty())
            {
                Console.WriteLine("There is no data for currency "+currency+" available.");
                return null;
            }

            //and check if number of table record is read properly
            records = ReadPageRecords();
            sizeTable = ReadTableSize();
            if (records == 0  || sizeTable == 0)
                return null;


            //iterate through pages; 
            for (int i = 1; (i-1)*sizeTable < records; i++)
            {
                //for the first page, post request is already sent
                if (i != 1)
                {
                    //fill and submit form for the specific page
                    SubmitForm(currency, startDate, endDate, i);
                    header = false;
                }

                //read HTML table in table
                ReadHTMLTable(table, header);

            }

            return table;

            
        }//end method


    }
}

