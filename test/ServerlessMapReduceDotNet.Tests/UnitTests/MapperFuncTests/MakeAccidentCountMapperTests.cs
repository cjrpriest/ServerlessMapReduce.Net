using System.Linq;
using NUnit.Framework;
using ServerlessMapReduceDotNet.MapReduce.Functions.MakeAccidentCount;
using ServerlessMapReduceDotNet.Model;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.MapperFuncTests
{
    public class MakeAccidentCountMapperTests
    {
        [TestCase("2016010000008,2016,1,9,0,18,0,4,5,0,0,0,1,1,6,1,5,1390,1,5,8,1,VOLKSWAGEN,SCIROCCO TSI", "VOLKSWAGEN")]
        [TestCase("2016010000018,2016,2,1,0,18,0,0,0,0,0,0,4,1,6,1,6,-1,-1,-1,6,1,NULL,NULL", "NULL")]
        [TestCase("2016010000005,2016,2,2,0,18,0,0,0,0,0,0,1,1,6,1,5,124,1,4,4,1,YAMAHA,HW 125 XENTER", "YAMAHA")]
        public void Test(string line, string expectedMake)
        {
            // Arrange
            var mapper = new MakeAccidentCountMapper();

            // Act
            var keyValuePairCollection = mapper.Map(line);

            // Assert
            keyValuePairCollection.Count.ShouldBe(1);
            keyValuePairCollection.First().ShouldBeOfType<CountKvp>();
            var countKvp = keyValuePairCollection.First() as CountKvp;
            countKvp.Key.ShouldBe(expectedMake);
            countKvp.Value.ShouldBe(1);
        }
    }
}