using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CharitiesOnline.Models;

namespace CharitiesOnline.Strategies.ErrorReader
{
    public interface IErrorReturnCalculator
    {
        string CalculateErrorReturn(GatewayError error);
    }
}
