using Amazon.S3;
using NUnit.Framework;
using ServerlessMapReduceDotNet.ServerlessInfrastructure.ObjectStore.AmazonS3;
using ServerlessMapReduceDotNet.Tests.Builders;
using Shouldly;

namespace ServerlessMapReduceDotNet.Tests.UnitTests.ObjectStoreTests.AmazonS3ObjectStoreTests
{
    public class AmazonS3PermissionsProviderTests
    {
        [TestCase("foo", "NoACL")]
        [TestCase("#MonitoringFolder#/index.html", "public-read")]
        [TestCase("#MonitoringFolder#foo/index.html", "NoACL")]
        [TestCase("#MonitoringFolder#/foo.html", "public-read")]
        [TestCase("#MonitoringFolder#/foo", "public-read")]
        [TestCase("mapped/1h3gj12g3h12321", "NoACL")]
        [TestCase("", "NoACL")]
        [TestCase(null, "NoACL")]
        public void Given_an_object_key__When_permissions_are_retrieved__Then_the_permission_is_as_expected(string inputObjectKey,
            string expectedPermissions)
        {
            // Arrange
            var config = new ConfigBuiler().Build();
            
            inputObjectKey = inputObjectKey?.Replace("#MonitoringFolder#", config.MonitoringFolder);

            var amazonS3PermissionsProvider = new AmazonS3PermissionsProvider(config);

            // Act
            var permissions = amazonS3PermissionsProvider.GetPermissions(inputObjectKey);

            // Assert
            permissions.ShouldBe(new S3CannedACL(expectedPermissions));
        }
    }
}