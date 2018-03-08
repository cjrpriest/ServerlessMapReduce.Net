namespace ServerlessMapReduceDotNet.Model
{
    class MostAccidentProneKvp : KeyValuePair<string, AccidentStats>
    {
        public MostAccidentProneKvp(string manufacturer, AccidentStats accidentStats)
        {
            Key = manufacturer;
            Value = accidentStats;
        }
    }

    class AccidentStats
    {
        public int NoOfAccidents { get; set; }
        public int NoOfCarsRegistered { get; set; }
        public double RegistrationsPerAccident { get; set; }
    }
}