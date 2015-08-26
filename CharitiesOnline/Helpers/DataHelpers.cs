using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;

using LumenWorks.Framework.IO.Csv;

namespace CharitiesOnline.Helpers
{
    public class DataHelpers
    {
        public static DataTable GetDataTableFromCsv(string path, bool isFirstRowHeader)
        {
            //Uses CsvReader from http://www.codeproject.com/Articles/9258/A-Fast-CSV-Reader
            try
            {
                DataTable dataTable = new DataTable();

                using (CsvReader csv =
                    new CsvReader(new StreamReader(path), true))
                {

                    dataTable.Load(csv);
                }

                return dataTable;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }

        }

        public static DataTable MakeRepaymentTable()
        {
            DataTable dt = new DataTable("RepaymentDonors");

            DataColumn Fore = new DataColumn();
            Fore.DataType = System.Type.GetType("System.String");
            Fore.ColumnName = "Fore";
            dt.Columns.Add(Fore);

            DataColumn Sur = new DataColumn();
            Sur.DataType = System.Type.GetType("System.String");
            Sur.ColumnName = "Sur";
            dt.Columns.Add(Sur);

            DataColumn House = new DataColumn();
            House.DataType = System.Type.GetType("System.String");
            House.ColumnName = "House";
            dt.Columns.Add(House);

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Postcode"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.Decimal"),
                ColumnName = "Total"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.Datetime"),
                ColumnName = "Date"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Type"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Overseas"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Description"
            });

            return dt;
        }

        public static DataTable MakeOtherIncomeTable()
        {
            DataTable dt = new DataTable("OtherInc");

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.String"),
                ColumnName = "Payer"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.DateTime"),
                ColumnName = "OIDate"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.Decimal"),
                ColumnName = "Gross"
            });

            dt.Columns.Add(new DataColumn
            {
                DataType = System.Type.GetType("System.Decimal"),
                ColumnName = "Tax"
            });

            return dt;
        }

        public static DataTable MakeStatusReportTable(hmrcclasses.GovTalkMessageBodyStatusReport statusReport)
        {
            DataTable statusReportTable = new DataTable("StatusReport");
            AddStatusReportColumns(statusReportTable);
            InsertStatusReportRows(statusReportTable, statusReport);

            return statusReportTable;            
        }

        private static void AddGatewayColumns(DataTable inputTable)
        {
            inputTable.Columns.Add("EnvelopeVersion");
            inputTable.Columns.Add("Class");
            inputTable.Columns.Add("Qualifier");
            inputTable.Columns.Add("TransactionID");
            inputTable.Columns.Add("PollInterval");
            inputTable.Columns.Add("ResponseEndPoint");
            inputTable.Columns.Add("GatewayTimeStamp");
            inputTable.Columns.Add("Keys");
        }

        private static void AddStatusReportColumns(DataTable inputTable)
        {
            inputTable.Columns.Add("StatusReportSenderID");
            inputTable.Columns.Add("StatusReportStartTimestamp");
            inputTable.Columns.Add("StatusReportEndTimestamp");
            inputTable.Columns.Add("StatusRecordTimestamp");
            inputTable.Columns.Add("StatusRecordCorrelationID");
            inputTable.Columns.Add("StatusRecordTransactionID");
            inputTable.Columns.Add("StatusRecordStatus");
        }

        private static void InsertStatusReportRows(DataTable statusReportTable, hmrcclasses.GovTalkMessageBodyStatusReport statusReport)
        {
            for (int i = 0; i <statusReport.StatusRecord.Length; i++)
            {
                statusReportTable.Rows.Add(new Object[]
                            {                                                                   
                                statusReport.SenderID,
                                statusReport.StartTimeStamp,
                                statusReport.EndTimeStamp,
                                statusReport.StatusRecord[i].TimeStamp,
                                statusReport.StatusRecord[i].CorrelationID,
                                statusReport.StatusRecord[i].TransactionID,
                                statusReport.StatusRecord[i].Status});
            }

        }
    }
}
