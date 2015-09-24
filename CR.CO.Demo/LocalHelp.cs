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

        public static string GetSendURI(string uriType, 
            hmrcclasses.GovernmentGatewayEnvironment gatewayEnvironment, 
            CR.Infrastructure.Configuration.IConfigurationRepository _configurationRepository)
        {
            string env = gatewayEnvironment.ToString();

            string uri = "";

            switch (uriType)
            {
                case "Test":
                    //uri = ConfigurationManager.AppSettings["SendURIlocaltestservice"];
                    uri = _configurationRepository.GetConfigurationValue<string>("SendURIlocaltestservice");
                    break;
                case "Send":
                    uri = _configurationRepository.GetConfigurationValue<string>(string.Concat("SendURI", env));
                    break;
                case "Poll":
                    uri = _configurationRepository.GetConfigurationValue<string>(string.Concat("PollURI", env));
                    break;
                case "DataRequest":
                    uri = _configurationRepository.GetConfigurationValue<string>(string.Concat("SendURI", env));
                    break;
            }

            return uri;
        }
    }
}
