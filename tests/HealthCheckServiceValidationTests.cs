using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.UnitTesting;
using Moq.MvvmQuickStart.QuickStart.Project;
using NUnit.Framework;

namespace dotnet_realtime_pipeline.tests
{
    [TestFixture]
    public class HealthCheckServiceValidationTests
    {
        [Test]
        public void HappyPath_Validate()
        {
            // Arrange
            var validation = new HealthCheckServiceValidation();
            // Act
            var result = validation.Validate();
            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() > 0);
        }

        [Test]
        public void EdgeCase_Validate_NullInput()
        {
            // Arrange
            var validation = new HealthCheckServiceValidation();
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => validation.Validate(null));
        }

        [Test]
        public void EdgeCase_Validate_EmptyCollection()
        {
            // Arrange
            var validation = new HealthCheckServiceValidation();
            // Act and Assert
            Assert.Throws<ArgumentException>(() => validation.Validate(new List<string>()));
        }

        [Test]
        public void ErrorPath_Validate_InvalidInput()
        {
            // Arrange
            var validation = new HealthCheckServiceValidation();
            // Act and Assert
            Assert.Throws<ArgumentException>(() => validation.Validate("invalid input"));
        }
    }
}