using System.Xml.Serialization;

namespace hmrcclasses
{
    //public partial class GovTalkMessageBody
    //{
    //    private GovTalkMessageBodyStatusReport statusReportField;
    //    public GovTalkMessageBodyStatusReport StatusReport
    //    {
    //        get
    //        {
    //            return this.statusReportField;
    //        }
    //        set
    //        {
    //            this.statusReportField = value;
    //        }
    //    }
    //}

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.govtalk.gov.uk/CM/envelope")]
    [System.Xml.Serialization.XmlRootAttribute("StatusReport", Namespace = "http://www.govtalk.gov.uk/CM/envelope", IsNullable = false)]    
    public partial class GovTalkMessageBodyStatusReport
    {
        private string senderIDField;

        private string startTimeStampField;

        private string endTimeStampField;

        private GovTalkMessageBodyStatusReportStatusRecord[] statusRecordField;

        /// <remarks/>
        public string SenderID
        {
            get
            {
                return this.senderIDField;
            }
            set
            {
                this.senderIDField = value;
            }
        }

        /// <remarks/>
        public string StartTimeStamp
        {
            get
            {
                return this.startTimeStampField;
            }
            set
            {
                this.startTimeStampField = value;
            }
        }

        /// <remarks/>
        public string EndTimeStamp
        {
            get
            {
                return this.endTimeStampField;
            }
            set
            {
                this.endTimeStampField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StatusRecord")]
        public GovTalkMessageBodyStatusReportStatusRecord[] StatusRecord
        {
            get
            {
                return this.statusRecordField;
            }
            set
            {
                this.statusRecordField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.govtalk.gov.uk/CM/envelope")]
    public partial class GovTalkMessageBodyStatusReportStatusRecord
    {

        private string timeStampField;

        private string correlationIDField;

        private string transactionIDField;

        private string statusField;

        /// <remarks/>
        public string TimeStamp
        {
            get
            {
                return this.timeStampField;
            }
            set
            {
                this.timeStampField = value;
            }
        }

        /// <remarks/>
        public string CorrelationID
        {
            get
            {
                return this.correlationIDField;
            }
            set
            {
                this.correlationIDField = value;
            }
        }

        /// <remarks/>
        public string TransactionID
        {
            get
            {
                return this.transactionIDField;
            }
            set
            {
                this.transactionIDField = value;
            }
        }

        /// <remarks/>
        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.govtalk.gov.uk/CM/envelope")]
    public partial class GovTalkMessageBodyStatusRecord
    {

        private string timeStampField;

        private string correlationIDField;

        private string transactionIDField;

        private string statusField;

        /// <remarks/>
        public string TimeStamp
        {
            get
            {
                return this.timeStampField;
            }
            set
            {
                this.timeStampField = value;
            }
        }

        /// <remarks/>
        public string CorrelationID
        {
            get
            {
                return this.correlationIDField;
            }
            set
            {
                this.correlationIDField = value;
            }
        }

        /// <remarks/>
        public string TransactionID
        {
            get
            {
                return this.transactionIDField;
            }
            set
            {
                this.transactionIDField = value;
            }
        }

        /// <remarks/>
        public string Status
        {
            get
            {
                return this.statusField;
            }
            set
            {
                this.statusField = value;
            }
        }
    }


    public partial class Application
    {
        private ErrorResponseErrorApplicationMessages messagesField;

        /// <remarks/>
        public ErrorResponseErrorApplicationMessages Messages
        {
            get
            {
                return this.messagesField;
            }
            set
            {
                this.messagesField = value;
            }
        }
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://www.govtalk.gov.uk/CM/errorresponse")]
    public partial class ErrorResponseErrorApplicationMessages
    {

        private string developerMessageField;

        /// <remarks/>
        public string DeveloperMessage
        {
            get
            {
                return this.developerMessageField;
            }
            set
            {
                this.developerMessageField = value;
            }
        }
    }
}
