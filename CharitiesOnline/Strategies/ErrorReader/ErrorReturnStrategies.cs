using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CharitiesOnline.Models;
using CharitiesOnline.Strategies.ErrorReader;

namespace CharitiesOnline.Strategies.ErrorStrategies
{
    public abstract class GatewayErrors
    {
        public string ErrorResponse(GatewayError error)
        {
            return error.ErrorDescription;
        }
    }

    public class AuthenticationUserErrorStrategy : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1046);
        }

    }

    public class SystemFailureError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1000);
        }
    }

    public class BadlyFormedXmlError: GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1001);
        }
    }

    public class AuthenticationDigSigError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1002);
        }
    }

    public class CorrelationIdPresentError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1020);
        }
    }

    public class CorrelationIdInvalidError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1035);
        }
    }

    public class StartDateOrderError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1038);
        }
    }

    public class DateError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1039);
        }
    }

    public class InconsistentValueError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1040);
        }
    }

    public class MissingBodyError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(1042);
        }
    }

    public class CorrelationIdNotFound : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(2000);
        }
    }

    public class DocumentTooBig : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(2001);
        }
    }

    public class MinimumRequiredDataMissing : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(2002);
        }
    }

    public class InternalProcessTimeOut : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(2005);
        }
    }

    public class DocumentProcessingFailed : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(3000);
        }
    }

    public class BusinessError: GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorCode.Equals(3001);
        }
    }

}
