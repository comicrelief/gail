using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CharitiesOnline.Models;
using hmrcclasses;

namespace CharitiesOnline.MessageReadingStrategies.ErrorReader
{
    public interface IErrorReturnStrategy
    {
        bool IsMatch(GovTalkMessageGovTalkDetailsError error);
        string ErrorResponse(GovTalkMessageGovTalkDetailsError error);
    }
}
