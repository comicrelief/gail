﻿using System;
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
            return error.ErrorText;
        }
    }

    public class AuthenticationUserError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            if (error.ErrorNumber.Equals(1046))
                return true;
            else
                return false;
        }

    }

    public class SystemFailureError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            if (error.ErrorNumber.Equals(1000))
                return true;
            else return false;
        }
    }

    public class BadlyFormedXmlError: GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(1001) == true ? true : false;
        }
    }

    public class AuthenticationDigSigError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(1002) == true? true: false;
        }
    }

    public class CorrelationIdPresentError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(1020) == true? true: false;
        }
    }

    public class CorrelationIdInvalidError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(1035) == true? true: false;
        }
    }

    public class StartDateOrderError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(1038) == true? true: false;
        }
    }

    public class DateError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(1039) == true? true: false;
        }
    }

    public class InconsistentValueError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(1040) == true? true : false;
        }
    }

    public class MissingBodyError : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(1042) == true? true : false;
        }
    }

    public class CorrelationIdNotFound : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(2000) == true? true: false;
        }
    }

    public class DocumentTooBig : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(2001) == true? true: false;
        }
    }

    public class MinimumRequiredDataMissing : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(2002) == true? true: false;
        }
    }

    public class InternalProcessTimeOut : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(2005) == true? true: false;
        }
    }

    public class DocumentProcessingFailed : GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(3000) == true? true: false;
        }
    }

    public class BusinessError: GatewayErrors, IErrorReturnStrategy
    {
        public bool IsMatch(GatewayError error)
        {
            return error.ErrorNumber.Equals(3001) == true? true: false;
        }
    }

}
