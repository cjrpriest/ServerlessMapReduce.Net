using System;
using ServerlessMapReduceDotNet.MapReduce.Abstractions;
using ServerlessMapReduceDotNet.Model;

namespace ServerlessMapReduceDotNet.MapReduce.Functions.MostAccidentProne
{
    public class MostAccidentProneMapper : IMapperFunc
    {
        public KeyValuePairCollection Map(string line)
        {
            var data = line.Split(',');
            if (LineIsFromAccidentStats(data))
            {
                if (VehicleWasLessThanOneYearOldAtTimeOfAccident(data))
                {
                    var manufacturer = data[22].ToUpper();
                    var accidentStats = new AccidentStats {NoOfAccidents = 1};
                    var mostAccidentProneKvp = new MostAccidentProneKvp(manufacturer, accidentStats);
                    return new KeyValuePairCollection { mostAccidentProneKvp };
                }
            }
            else
            {
                var manufacturer = data[0].ToUpper();
                var noOfRegistrations = ParseDirtyInt(data[6]);
                var accidentStats = new AccidentStats {NoOfCarsRegistered = noOfRegistrations};
                var mostAccidentProneKvp = new MostAccidentProneKvp(manufacturer, accidentStats);
                return new KeyValuePairCollection { mostAccidentProneKvp };
            }
            
            return new KeyValuePairCollection();
        }

        private bool LineIsFromAccidentStats(string[] data)
        {
            return data.Length > 20;
        }

        private bool VehicleWasLessThanOneYearOldAtTimeOfAccident(string[] data)
        {
            var ageOfVehicleStr = data[19];
            var success = Int32.TryParse(ageOfVehicleStr, out var ageOfVehicle);
            if (!success) return false;
            return ageOfVehicle == 1;
        }

        private int ParseDirtyInt(string dirtyInt)
        {
            var cleanInt = dirtyInt.Replace(",", String.Empty).Replace("\"", String.Empty);
            return Int32.Parse(cleanInt);
        }
    }
}