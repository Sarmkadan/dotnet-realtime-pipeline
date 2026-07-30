using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Moq;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.UnitTesting;
using System.Runtime.InteropServices;
using System.Runtime;
using System.Collections;
using System.Collections;

namespace dotnet_realtime_pipeline.Tests
{
    [TestClass]
    public class BackgroundProcessingWorkerTests
    {
        [TestMethod]
        public void HappyPath_Start()
        {
            // Arrange
            var worker = new BackgroundProcessingWorker();
            // Act
            worker.Start();
            // Assert
            Assert.IsTrue(worker.IsRunning);
        }

        [TestMethod]
        public void EdgeCase_Start_NullInput()
        {
            // Arrange
            var worker = new BackgroundProcessingWorker();
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => worker.Start());
        }

        [TestMethod]
        public void HappyPath_StopAsync()
        {
            // Arrange
            var worker = new BackgroundProcessingWorker();
            worker.Start();
            // Act
            worker.StopAsync().Wait();
            // Assert
            Assert.IsFalse(worker.IsRunning);
        }

        [TestMethod]
        public void EdgeCase_StopAsync_NullInput()
        {
            // Arrange
            var worker = new BackgroundProcessingWorker();
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => worker.StopAsync());
        }

        [TestMethod]
        public void HappyPath_Dispose()
        {
            // Arrange
            var worker = new BackgroundProcessingWorker();
            worker.Start();
            // Act
            worker.Dispose();
            // Assert
            Assert.IsTrue(worker.IsDisposed);
        }

        [TestMethod]
        public void EdgeCase_Dispose_NullInput()
        {
            // Arrange
            var worker = new BackgroundProcessingWorker();
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => worker.Dispose());
        }
    }
}
