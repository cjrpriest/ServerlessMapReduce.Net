using NUnit.Framework;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.Helpers;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ServerlessInfrastructureTests.HelperTests
{
    public class StringExtensionTests
    {
        [TestCase("foo bar", 4, "fo... ...ar")]
        [TestCase("Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat", 100, "Lorem ipsum dolor sit amet, consectetur adipiscing... ...co laboris nisi ut aliquip ex ea commodo consequat")]
        public void When_TopAndTail_is_applied_to_a_string__Then_a_concatentation_of_the_top_and_tail_of_the_input_string_is_returned(string intputString, int inputMaxLength, string expectedOutput)
        {
            // Act
            var output = intputString.TopAndTail(inputMaxLength);
            
            // Assert
            output.ShouldBe(expectedOutput);
        }
    }
}