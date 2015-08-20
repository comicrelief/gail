using System;

namespace CharitiesOnline.Models
{
    public class GatewayError : IGatewayError
    {
        public int ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
        public string ErrorRaisedBy { get; set; }
        public string ErrorType { get; set; }
        public string ErrorLocation { get; set; }
       
    }
}