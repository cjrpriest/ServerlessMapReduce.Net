using System.Collections.Generic;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.MapReduce.Commands.Reduce;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Functions.MostAccidentProne
{
    class MostAccidentProneFinalReducer : IFinalReduceFunc
    {
        public IReadOnlyCollection<string> FinalReduce(IKeyValuePair keyValuePair)
        {
            var mostAccidentProne = (MostAccidentProneKvp) keyValuePair;
            return new[] {$"{mostAccidentProne.Key},{mostAccidentProne.Value.NoOfCarsRegistered},{mostAccidentProne.Value.NoOfAccidents},{mostAccidentProne.Value.RegistrationsPerAccident:0.0}"};
        }

        public IReadOnlyCollection<string> FinalReduce2(CompressedMostAccidentProneData compressedMostAccidentProneData)
        {
            return new[] {$"{compressedMostAccidentProneData.M},{compressedMostAccidentProneData.S.C},{compressedMostAccidentProneData.S.A},{compressedMostAccidentProneData.S.R:0.0}"};
        }
    }
}