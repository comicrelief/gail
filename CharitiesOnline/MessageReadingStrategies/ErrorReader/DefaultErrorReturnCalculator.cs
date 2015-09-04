using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CharitiesOnline.Models;
using CharitiesOnline.MessageReadingStrategies.ErrorStrategies;

using hmrcclasses;

namespace CharitiesOnline.MessageReadingStrategies.ErrorReader
{
    public class DefaultErrorReturnCalculator : IErrorReturnCalculator
    {
        private readonly List<IErrorReturnStrategy> _errorRules;

        public DefaultErrorReturnCalculator()
        {
            _errorRules = new List<IErrorReturnStrategy>();
            _errorRules.Add(new AuthenticationUserError());
            _errorRules.Add(new BusinessError());
            _errorRules.Add(new SystemFailureError());
            _errorRules.Add(new BadlyFormedXmlError());
            _errorRules.Add(new AuthenticationDigSigError());
            _errorRules.Add(new CorrelationIdPresentError());
            _errorRules.Add(new CorrelationIdInvalidError());
            _errorRules.Add(new CorrelationIdNotFound());
            _errorRules.Add(new StartDateOrderError());
            _errorRules.Add(new DateError());
            _errorRules.Add(new InconsistentValueError());
            _errorRules.Add(new MissingBodyError());
            _errorRules.Add(new DocumentTooBig());
            _errorRules.Add(new MinimumRequiredDataMissing());
            _errorRules.Add(new InternalProcessTimeOut());
            _errorRules.Add(new DocumentProcessingFailed());
            
        }

        public string CalculateErrorReturn(GovTalkMessageGovTalkDetailsError error)
        {
            return _errorRules.First(e => e.IsMatch(error)).ErrorResponse(error);

            // @TODO: Handle situation where no rule is matched, or there is no errorResponse class
        }
    }
}
