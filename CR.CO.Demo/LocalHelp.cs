using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace CR.CO.Demo
{
    public class LocalHelp
    {
        public static void ConsolePrintDataTable(System.Data.DataTable inputTable)
        {
            foreach (DataColumn column in inputTable.Columns)
                Console.Write("\t{0}", column.ColumnName);

            Console.WriteLine("");
            foreach (DataRow row in inputTable.Rows)
            {
                foreach (DataColumn column in inputTable.Columns)
                    Console.Write("\t{0}", row[column]);
                Console.WriteLine("");
            }
        }
    }
}
