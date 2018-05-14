using System.Collections.Generic;
using ServerlessMapReduceDotNet.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.FinalReducers
{
    class MostAccidentProneFinalReducer : IFinalReduceFunc
    {
        public IReadOnlyCollection<string> FinalReduce(IKeyValuePair keyValuePair)
        {
            var mostAccidentProne = (MostAccidentProneKvp) keyValuePair;
            return new[] {$"{mostAccidentProne.Key},{mostAccidentProne.Value.NoOfCarsRegistered},{mostAccidentProne.Value.NoOfAccidents},{mostAccidentProne.Value.RegistrationsPerAccident:0.0}"};
        }
    }
}