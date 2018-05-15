using System;
using System.Collections.Generic;
using System.Linq;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Functions.MostAccidentProne
{
    class MostAccidentProneReducer : IReducerFunc
    {
        public KeyValuePairCollection Reduce(KeyValuePairCollection inputKeyValuePairs)
        {
            var reducedMostAccidentProne = new Dictionary<string, AccidentStats>();
            inputKeyValuePairs.ForEach(x =>
            {
                var mostAccidentProneKvp = (MostAccidentProneKvp)x;
                if (!reducedMostAccidentProne.ContainsKey(mostAccidentProneKvp.Key))
                    reducedMostAccidentProne.Add(mostAccidentProneKvp.Key, new AccidentStats());
                reducedMostAccidentProne[mostAccidentProneKvp.Key] =
                    ReduceAccidentStats(reducedMostAccidentProne[mostAccidentProneKvp.Key], mostAccidentProneKvp.Value);
            });
            
            var keyValuePairs = new KeyValuePairCollection();

            reducedMostAccidentProne
                .ToList()
                .ForEach(x => keyValuePairs.Add(new MostAccidentProneKvp(x.Key, x.Value)));
            
            keyValuePairs.Sort(CompareMostAccidentProneKvps);

            return keyValuePairs;
        }

        private AccidentStats ReduceAccidentStats(AccidentStats stats1, AccidentStats stats2)
        {
            var newNoOfAccidents = stats1.NoOfAccidents + stats2.NoOfAccidents;
            var newNoOfCarsRegistered = stats1.NoOfCarsRegistered + stats2.NoOfCarsRegistered;
            return new AccidentStats
            {
                NoOfAccidents = newNoOfAccidents,
                NoOfCarsRegistered = newNoOfCarsRegistered,
                RegistrationsPerAccident = (double) newNoOfCarsRegistered / newNoOfAccidents
            };
        }

        private int CompareMostAccidentProneKvps(object kvp1, object kvp2)
        {
            try
            {
                var mostAccidentProneKvp1 = ((MostAccidentProneKvp) kvp1);
                var mostAccidentProneKvp2 = ((MostAccidentProneKvp) kvp2);
                return mostAccidentProneKvp1.Value.RegistrationsPerAccident
                <= mostAccidentProneKvp2.Value.RegistrationsPerAccident ? 1 : -1;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return 0;
            }
        } 
    }
}