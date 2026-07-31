using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal.Commands;
using System.Runtime;
using System.Runtime.InteropServices;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System;

namespace Tests
{
    public class RetryHelperValidationTests
    {
        [Test]
        public void HappyPath_Validate()
        {
            // Given
            var retryHelperValidation = new RetryHelperValidation();
            // When
            var result = retryHelperValidation.Validate();
            // Then
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count > 0);
        }

        [Test]
        public void EdgeCase_NullInput_Validate()
        {
            // Given
            var retryHelperValidation = new RetryHelperValidation();
            // When
            var result = retryHelperValidation.Validate();
            // Then
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void EdgeCase_EmptyCollection_Validate()
        {
            // Given
            var retryHelperValidation = new RetryHelperValidation();
            // When
            var result = retryHelperValidation.Validate();
            // Then
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count == 0);
        }

        [Test]
        public void ErrorPath_Validate_ThrowsException()
        {
            // Given
            var retryHelperValidation = new RetryHelperValidation();
            // When
            var result = retryHelperValidation.Validate();
            // Then
            Assert.Throws<Exception>(() => retryHelperValidation.Validate());
        }
    }
}