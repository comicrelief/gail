using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;

using LumenWorks.Framework.IO.Csv;

namespace CharitiesOnline.Helpers
{
    /// <summary>
    /// A helper class that provides methods for creating datatables and inserting data into them
    /// </summary>
    public class DataHelpers
    {
        /// <summary>
        /// Given a filepath and a header flag will create a datatable from a CSV file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="isFirstRowHeader"></param>
        /// <returns></returns>
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

        public static DataTable MakeErrorTable(CharitiesOnline.Models.GovTalkMessageError[] errors)
        {
            DataTable errorTable = new DataTable("Errors");
            AddErrorResponseColumns(errorTable);
            InsertErrorResponseRows(errorTable, errors);

            return errorTable;

        }

        private static void AddErrorResponseColumns(DataTable inputTable)
        {
            inputTable.Columns.Add("CorrelationId");
            inputTable.Columns.Add("RaisedBy");
            inputTable.Columns.Add("Number");
            inputTable.Columns.Add("Type");
            inputTable.Columns.Add("Text");
            inputTable.Columns.Add("Location");
            inputTable.Columns.Add("Warnings"); //where from?
            inputTable.Columns.Add("Message");
        }

        private static void InsertErrorResponseRows(DataTable errorsTable, CharitiesOnline.Models.GovTalkMessageError[] errors)
        {
            for (int i = 0; i < errors.Length; i++)
            {
                DataRow errorRow = errorsTable.NewRow();
                errorRow["CorrelationId"] = errors[i].CorrelationId;
                errorRow["RaisedBy"] = errors[i].ErrorRaisedBy;
                errorRow["Number"] = errors[i].ErrorNumber;
                errorRow["Type"] = errors[i].ErrorType;
                errorRow["Text"] = errors[i].ErrorText;
                errorRow["Location"] = errors[i].ErrorLocation;
                errorRow["Message"] = errors[i].ErrrorApplicationMessage;

                errorsTable.Rows.Add(errorRow);
            }
        }
    }
}
