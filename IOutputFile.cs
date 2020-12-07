using System.Data;


namespace WebScrape
{
    interface IOutputFile
    {

        //write cells from table using delimiter
        bool Write(DataTable table, string delimiter);
    }
}
