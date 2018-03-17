using NUnit.Framework;
using ServerlessMapReduceDotNet.Model;
using ServerlessMapReduceDotNet.Reducers;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ReducerFunnTests
{
    public class MakeAccidentCountReducerTests
    {
        [Test]
        public void Test()
        {
            // Arrange
            var makeAccidentCountReducer = new MakeAccidentCountReducer();
            var keyValuePairInputs = new KeyValuePairCollection
            {
                new CountKvp("Ford", 1),
                new CountKvp("Vauxhall", 2),
                new CountKvp("Ford", 1),
                new CountKvp("Lotus", 3),
                new CountKvp("Lotus", 1),
            };

            // Act
            var keyValuePairOutputs = makeAccidentCountReducer.Reduce(keyValuePairInputs);

            // Assert
            keyValuePairOutputs.Count.ShouldBe(3);
            keyValuePairOutputs.ShouldContain(kvp => ((CountKvp)kvp).Key == "Ford" && ((CountKvp)kvp).Value == 2);
            keyValuePairOutputs.ShouldContain(kvp => ((CountKvp)kvp).Key == "Vauxhall" && ((CountKvp)kvp).Value == 2);
            keyValuePairOutputs.ShouldContain(kvp => ((CountKvp)kvp).Key == "Lotus" && ((CountKvp)kvp).Value == 4);
        }
    }
}