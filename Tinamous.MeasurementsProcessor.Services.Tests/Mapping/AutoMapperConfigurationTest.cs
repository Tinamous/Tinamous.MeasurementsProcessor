using NUnit.Framework;
using Tinamous.MeasurementsProcessor.Services.Mapping;

namespace Tinamous.MeasurementsProcessor.Services.Tests.Mapping
{
    [TestFixture]
    public class AutoMapperConfigurationTest
    {
        [Test]
        public void ConfigureAutoMapper()
        {
            // Arrange 
            var configuration = AutoMapperConfiguration.Configure();

            // Assert
            configuration.AssertConfigurationIsValid();
        }
    }
}