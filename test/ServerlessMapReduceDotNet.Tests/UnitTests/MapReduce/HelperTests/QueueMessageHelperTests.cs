using NUnit.Framework;
using ServerlessMapReduceDotNet.MapReduce.Helpers;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.MapReduce.HelperTests
{
    public class QueueMessageHelperTests
    {
        [TestCase("foo/objectName", "objectName")]
        [TestCase("raw/MakeModel2016.csv", "MakeModel2016.csv")]
        [TestCase("ingested/MakeModel2016.csv-506b2e6e-ad25-4e05-ba0b-07cb609b6266", "MakeModel2016.csv-506b2e6e-ad25-4e05-ba0b-07cb609b6266")]
        [TestCase("ingested/foo/MakeModel2016.csv-506b2e6e-ad25-4e05-ba0b-07cb609b6266", "MakeModel2016.csv-506b2e6e-ad25-4e05-ba0b-07cb609b6266")]
        public void Test(string inputQueueMessage, string expectedOjectName)
        {
            // Act
            var objectName = inputQueueMessage.ObjectName();

            // Assert
            objectName.ShouldBe(expectedOjectName);
        }
    }
}