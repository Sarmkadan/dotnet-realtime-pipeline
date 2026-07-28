using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using System.IO;
using System.Reflection;

namespace DotNetRealtimePipeline.Tests.State
{
    [TestFixture]
    public class PipelineStateManagerExtensionsTests
    {
        [Test]
        public void GetTransitionsTo_Happy_Path_Returns_Transitions()
        {
            // Arrange
            var manager = new PipelineStateManager();
            manager.AddStateTransition(new StateTransition
            {
                FromState = PipelineState.Running,
                ToState = PipelineState.Paused,
                Timestamp = DateTime.UtcNow,
                Reason = "Happy path"
            });
            manager.AddStateTransition(new StateTransition
            {
                FromState = PipelineState.Paused,
                ToState = PipelineState.Running,
                Timestamp = DateTime.UtcNow.AddMinutes(1),
                Reason = "Happy path"
            });

            // Act
            var transitions = PipelineStateManagerExtensions.GetTransitionsTo(manager, PipelineState.Running);

            // Assert
            Assert.AreEqual(1, transitions.Count);
            Assert.IsTrue(transitions.Any(t => t.FromState == PipelineState.Paused && t.ToState == PipelineState.Running));
        }

        [Test]
        public void GetTransitionsTo_Null_Manager_Throws_ArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PipelineStateManagerExtensions.GetTransitionsTo(null, PipelineState.Running));
        }

        [Test]
        public void GetLastTransition_Happy_Path_Returns_Last_Transition()
        {
            // Arrange
            var manager = new PipelineStateManager();
            manager.AddStateTransition(new StateTransition
            {
                FromState = PipelineState.Running,
                ToState = PipelineState.Paused,
                Timestamp = DateTime.UtcNow,
                Reason = "Happy path"
            });
            manager.AddStateTransition(new StateTransition
            {
                FromState = PipelineState.Paused,
                ToState = PipelineState.Running,
                Timestamp = DateTime.UtcNow.AddMinutes(1),
                Reason = "Happy path"
            });

            // Act
            var transition = PipelineStateManagerExtensions.GetLastTransition(manager);

            // Assert
            Assert.IsNotNull(transition);
            Assert.AreEqual(PipelineState.Running, transition.ToState);
        }

        [Test]
        public void GetLastTransition_Null_Manager_Throws_ArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PipelineStateManagerExtensions.GetLastTransition(null));
        }

        [Test]
        public void GetTotalTimeInState_Happy_Path_Returns_Total_Time()
        {
            // Arrange
            var manager = new PipelineStateManager();
            manager.AddStateTransition(new StateTransition
            {
                FromState = PipelineState.Running,
                ToState = PipelineState.Paused,
                Timestamp = DateTime.UtcNow,
                Reason = "Happy path"
            });
            manager.AddStateTransition(new StateTransition
            {
                FromState = PipelineState.Paused,
                ToState = PipelineState.Running,
                Timestamp = DateTime.UtcNow.AddMinutes(1),
                Reason = "Happy path"
            });

            // Act
            var totalTime = PipelineStateManagerExtensions.GetTotalTimeInState(manager, PipelineState.Running);

            // Assert
            Assert.AreEqual(TimeSpan.FromMinutes(1), totalTime);
        }

        [Test]
        public void GetTotalTimeInState_Null_Manager_Throws_ArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PipelineStateManagerExtensions.GetTotalTimeInState(null, PipelineState.Running));
        }

        [Test]
        public void ToHistoryString_Happy_Path_Returns_History_String()
        {
            // Arrange
            var manager = new PipelineStateManager();
            manager.AddStateTransition(new StateTransition
            {
                FromState = PipelineState.Running,
                ToState = PipelineState.Paused,
                Timestamp = DateTime.UtcNow,
                Reason = "Happy path"
            });
            manager.AddStateTransition(new StateTransition
            {
                FromState = PipelineState.Paused,
                ToState = PipelineState.Running,
                Timestamp = DateTime.UtcNow.AddMinutes(1),
                Reason = "Happy path"
            });

            // Act
            var historyString = PipelineStateManagerExtensions.ToHistoryString(manager);

            // Assert
            Assert.IsNotNull(historyString);
            Assert.IsTrue(historyString.Contains("Happy path"));
        }

        [Test]
        public void ToHistoryString_Null_Manager_Throws_ArgumentNullException()
        {
            // Act and Assert
            Assert.Throws<ArgumentNullException>(() => PipelineStateManagerExtensions.ToHistoryString(null));
        }
    }
}