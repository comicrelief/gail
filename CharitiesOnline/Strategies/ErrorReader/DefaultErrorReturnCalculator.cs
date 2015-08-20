using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CharitiesOnline.Models;
using CharitiesOnline.Strategies.ErrorStrategies;

namespace CharitiesOnline.Strategies.ErrorReader
{
    public class DefaultErrorReturnCalculator : IErrorReturnCalculator
    {
        private readonly List<IErrorReturnStrategy> _errorRules;

        public DefaultErrorReturnCalculator()
        {
            _errorRules = new List<IErrorReturnStrategy>();
            _errorRules.Add(new AuthenticationUserErrorStrategy());
        }

        public string CalculateErrorReturn(GatewayError error)
        {
            return _errorRules.First(e => e.IsMatch(error)).ErrorResponse(error);
        }
    }
}
