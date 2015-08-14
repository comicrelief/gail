﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CR.Infrastructure.Logging;

namespace CharitiesOnline.Helpers
{
    // Sorry, ran out of time to work out how to make this an interface implementation

    public class GovTalkMessageFileName
    {
        // public ILoggingService _loggingService { get; private set;}
        private ILoggingService _loggingService;

        public GovTalkMessageFileName(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public ILoggingService LoggingService
        {
            private get
            {
                return _loggingService;
            }
            set
            {
                _loggingService = value;
            }
        }

        public string Timestamp { get; private set; }
        public string Environment { get; private set; }
        public string MessageIntention { get; private set; }
        public string MessageQualifier { get; private set; }
        public string CorrelationId { get; private set; }
        public string CustomNamePart { get; private set; }
        public string FilePath { get; private set; }

        private const string FILE_EXT = ".xml";
        private const string SEPARATOR = "_";
        
        


        public class FileNameBuilder
        {
            private string _timestamp;
            private string _environment;
            private string _messageIntention;
            private string _messageQualifier;
            private string _correlationId;
            private string _customNamePart;
            private string _filePath;
            private ILoggingService _loggingService;

            public FileNameBuilder AddLogger(ILoggingService loggingService)
            {
                _loggingService = loggingService;
                return this;
                
            }
            public FileNameBuilder AddTimestamp(string value)
            {
                _timestamp = string.Concat(value,SEPARATOR);
                return this;
            }

            public FileNameBuilder AddEnvironment(string value)
            {
                _environment = string.Concat(value,SEPARATOR);
                return this;
            }

            public FileNameBuilder AddMessageIntention(string value)
            {
                _messageIntention = string.Concat(value,SEPARATOR);
                return this;
            }

            public FileNameBuilder AddMessageQualifier(string value)
            {
                _messageQualifier = string.Concat(value, SEPARATOR);
                return this;
            }

            public FileNameBuilder AddCorrelationId(string value)
            {
                _correlationId = String.Concat(value, SEPARATOR);
                return this;
            }

            public FileNameBuilder AddCustomNamePart(string value)
            {
                _customNamePart = string.Concat(value, SEPARATOR);
                return this;
            }

            public FileNameBuilder AddFilePath(string value)
            {
                _filePath = value;
                return this;
            }

            public GovTalkMessageFileName BuildFileName()
            {
                return new GovTalkMessageFileName(_loggingService) { 
                    FilePath = _filePath,
                    CorrelationId = _correlationId,
                    Environment = _environment,
                    Timestamp = _timestamp,
                    MessageIntention = _messageIntention,
                    MessageQualifier = _messageQualifier,
                    CustomNamePart = _customNamePart
                };
            }
        }

        public override string ToString()
        {
            string filename = string.Concat(FilePath, Environment, MessageIntention, Timestamp, CorrelationId, MessageQualifier, CustomNamePart, FILE_EXT);

            // if there are any SEPARATOR chars, remove the last one
            if(filename.Contains(SEPARATOR))
            {
                filename = CommonUtilityHelpers.ReverseString(filename);
                filename = CommonUtilityHelpers.ReplaceFirst(filename, SEPARATOR, String.Empty);
                filename = CommonUtilityHelpers.ReverseString(filename);
            }

            return validateFileName(filename);
            //return base.ToString();
        }

        private string validateFileName(string filename)
        {
            if (!isValidFilename(filename))
                return string.Concat(@"C:\Temp\localreply", DateTime.Now.ToString("yyyymmddhhmiss"),".xml");
            else
                return filename;
        }

        private bool isValidFilename(string filename)
        {
            System.IO.FileInfo fileinfo = null;
            try
            {
                fileinfo = new System.IO.FileInfo(filename);
            }
            catch (ArgumentException argEx) { _loggingService.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "FileName exception", argEx); }
            catch (System.IO.PathTooLongException pathTooLongEx) { _loggingService.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "FileName exception", pathTooLongEx); }
            catch (NotSupportedException unsupportedEx) { _loggingService.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "FileName exception", unsupportedEx); }

            if(ReferenceEquals(fileinfo,null))
            {
                return false;
            }

            return true;
        }

    }
}