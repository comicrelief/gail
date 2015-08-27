using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Data;

using CR.Infrastructure.Configuration;

namespace CharitiesOnline
{
    /// <summary>
    /// Reference values for GovTalkMessages are constants that need to be assigned to the properties of the GovTalkMessage objects.
    /// The ReferenceDataManager is a static class to allow these values to be used by the message builders.
    /// </summary>
    public static class ReferenceDataManager
    {
        // A class that wraps access to either a config file or a database as a source of
        // data for reference values

        private static IConfigurationRepository _configurationRepository = new ConfigFileConfigurationRepository();

        private static NameValueCollection _settings;
        private static bool _sourceIsSet;
        private static DataTable _databaseSettings;
        
        private static DatabaseType _databaseType;
        private static SourceTypes _sourceType;

        /// <summary>
        /// If a datatable is used for reference values, set it here
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="dbType"></param>
        public static void SetDataTable(DataTable dt, DatabaseType dbType)
        {
            _databaseType = dbType;
            _databaseSettings = dt;
        }

        /// <summary>
        /// Get the Settings
        /// </summary>
        public static NameValueCollection Settings
        {
            get
            {
                if(_sourceIsSet)
                {
                    return _settings;
                }

                throw new Exception("SourceData Not Set");
            }
        }

        public static bool SourceIsSet
        {
            get
            {
                return _sourceIsSet;
            }
            private set
            {
                // Only this class should be able to set this value.
                _sourceIsSet = value;
            }
        }

        /// <summary>
        ///  Set a config file or a database table as the source of reference data.
        /// </summary>
        /// <param name="sourceType"></param>
        public static void SetSource(SourceTypes sourceType)
        {
            _sourceType = sourceType;

            // Determine whether the Config file is used or the Database connection.

            _settings = new NameValueCollection();

            switch (sourceType)
            {
                case SourceTypes.ConfigFile:
                    foreach (var setting in _configurationRepository.AllEntries().Keys)
                    {
                        _settings.Set(setting.ToString(), _configurationRepository.GetConfigurationValue<string>(setting.ToString()));
                    }
                    SourceIsSet = true;
                    break;
                case SourceTypes.DatabaseKeyValue:
                    foreach(DataRow dr in _databaseSettings.Rows)
                    {
                        _settings.Set(dr["Key"].ToString(), dr["Value"].ToString());
                    }
                    SourceIsSet = true;
                    break;
                case SourceTypes.DatabaseDefaultHeader:
                    foreach(DataColumn dc in _databaseSettings.Columns)
                    {
                        _settings.Set(dc.ColumnName.ToString(), _databaseSettings.Rows[0][dc.ColumnName].ToString());
                    }
                    SourceIsSet = true;
                    break;
            }
        }

        private static void ClearSettings()
        {
            _settings = null;
        }
        
        /// <summary>
        /// The Database types are Key-Value and DefaultHeader
        //  Key-Value means there are two columns in the database table, named Key and Value. Each row contains a Key and a Value
        //  DefaultHeader is a single row where the column names correspond to the keys and the row entries contains the values
        /// </summary>
        public enum SourceTypes
        {
            ConfigFile, DatabaseKeyValue, TextFile, DatabaseDefaultHeader
        }

        // Not sure that this one is used
        public enum DatabaseType
        {
            KeyValue, DefaultHeader
        }
    }
}
