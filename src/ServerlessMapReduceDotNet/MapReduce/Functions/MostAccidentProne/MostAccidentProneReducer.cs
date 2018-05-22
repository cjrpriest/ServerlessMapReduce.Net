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

        private int CompareMostAccidentProneKvps(IKeyValuePair x, IKeyValuePair y)
        {
            if (x == null) throw new ArgumentNullException(nameof(x));
            if (y == null) throw new ArgumentNullException(nameof(y));
            
            var mostAccidentProneKvp1 = x as MostAccidentProneKvp;
            if (mostAccidentProneKvp1 == null) throw new ArgumentNullException(nameof(mostAccidentProneKvp1));

            var mostAccidentProneKvp2 = y as MostAccidentProneKvp;
            if (mostAccidentProneKvp2 == null) throw new ArgumentNullException(nameof(mostAccidentProneKvp2));
            
            if (mostAccidentProneKvp1.Value == null) throw new ArgumentNullException($"{nameof(mostAccidentProneKvp1)}.{nameof(mostAccidentProneKvp1.Value)}");
            if (mostAccidentProneKvp2.Value == null) throw new ArgumentNullException($"{nameof(mostAccidentProneKvp2)}.{nameof(mostAccidentProneKvp2.Value)}");

            return mostAccidentProneKvp1.Value.RegistrationsPerAccident.CompareTo(
                mostAccidentProneKvp2.Value.RegistrationsPerAccident);
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
    }
}