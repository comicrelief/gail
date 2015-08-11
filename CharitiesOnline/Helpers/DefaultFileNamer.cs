using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharitiesOnline.Helpers
{
    // Sorry, ran out of time to work out how to make this an interface implementation

    public class DefaultFileNamer
    {
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

            public DefaultFileNamer BuildFileName()
            {
                return new DefaultFileNamer { 
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
            string filename = string.Concat(FilePath, Environment, MessageIntention,Timestamp, MessageQualifier, CorrelationId, CustomNamePart, FILE_EXT);

            // if there are any SEPARATOR chars, remove the last one
            if(filename.Contains(SEPARATOR))
            {
                filename = CommonUtilityHelpers.ReverseString(filename);
                filename = CommonUtilityHelpers.ReplaceFirst(filename, SEPARATOR, String.Empty);
                filename = CommonUtilityHelpers.ReverseString(filename);
            }

            // @TODO: Test if this is a valid filename? 

            return filename;
            //return base.ToString();
        }

    }
}
