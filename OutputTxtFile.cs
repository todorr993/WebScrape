using System;
using System.Collections.Generic;
using System.Data;
using System.IO;





namespace WebScrape
{


    class OutputTxtFile : IOutputFile
     {
        private string path;

        public OutputTxtFile(string outputPathFile) {
            this.path = outputPathFile;
        }

        //iterate through table and write in output file
        public bool Write(DataTable table, string delimiter) 
        {
            try
            {
                using (StreamWriter file =
                    new StreamWriter(Path.Combine(path, table.TableName + ".txt")))
                {
                    List<string> list = new List<string>();

                    //read header row
                    foreach (DataColumn column in table.Columns)
                        list.Add(column.ColumnName);

                    //write in file
                    file.WriteLine(string.Join(delimiter, list.ToArray()));

                    //read table rows
                    foreach (DataRow row in table.Rows)
                    {
                        list = new List<string>();
                        foreach (object item in row.ItemArray)                     
                            list.Add((string)item);

                        //write in file
                        file.WriteLine(string.Join(delimiter, list.ToArray()));
                    }
                }
            }catch (DirectoryNotFoundException) {
                Console.WriteLine("Directory can not be found.");
                return false;
            }catch (PathTooLongException)
            {
                Console.WriteLine("'path' exceeds the maxium supported path length.");
                return false;
            }
            catch (IOException)
            {
                Console.WriteLine("Output File error");
                return false;
            }


            return true;
        }



    }
}
