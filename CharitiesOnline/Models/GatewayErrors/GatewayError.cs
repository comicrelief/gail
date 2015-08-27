using System;

namespace CharitiesOnline.Models
{
    public class GatewayError : IGatewayError
    {
        public string CorrelationId { get; set; }
        public int ErrorNumber { get; set; }
        public string ErrorText { get; set; }
        public string ErrorRaisedBy { get; set; }
        public string ErrorType { get; set; }
        public string ErrorLocation { get; set; }
        public string ErrrorApplicationMessage { get; set; }
       
    }
}