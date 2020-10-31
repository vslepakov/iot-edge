using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventProcessor.Test
{
    [TestClass]
    public class AlertHandlerTest
    {
        [TestMethod]
        public async Task Brand_New_Alert_Successfully_Recognized_And_Signaled()
        {
            var alertsRepositoryMock = new AlertsRepositoryMock();
            var notificationServiceMock = new Mock<INotificationService>();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();

            var handler = new AlertHandler(alertsRepositoryMock, notificationServiceMock.Object, loggerMock.Object);
            var newAlert = new Alert
            {
                id = "123",
                AlertType = AlertTypes.ThresholdViolation,
                ApplicationUri = "SomeAppUri",
                AverageValue = 56,
                DisplayName = "Temperature",
                Timestamp = DateTime.Now
            };

            await handler.HandleAsync(new List<Alert> { newAlert });

            var createdAlert = alertsRepositoryMock.All.First();
            Assert.AreEqual(1, createdAlert.Occurrences);
            Assert.AreSame(newAlert, createdAlert);
            notificationServiceMock.Verify(mock => mock.NotifyAsync(newAlert), Times.Once);
        }

        [TestMethod]
        public async Task Already_Existing_Alert_Successfully_Recognized()
        {
            const string AlertId = "123";
            var notificationServiceMock = new Mock<INotificationService>();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            var alertsRepositoryMock = new AlertsRepositoryMock();

            alertsRepositoryMock.PrePopulateAlerts(new List<Alert>
            {
                new Alert
                {
                    id = "987",
                    AlertType = AlertTypes.ThresholdViolation,
                    ApplicationUri = "SomeAppUri",
                    AverageValue = 56,
                    DisplayName = "Temperature",
                    Timestamp = DateTime.Now.AddMinutes(-50),
                    Occurrences = 1
                },
                new Alert
                {
                    id = AlertId,
                    AlertType = AlertTypes.ThresholdViolation,
                    ApplicationUri = "SomeAppUri",
                    AverageValue = 56,
                    DisplayName = "Temperature",
                    Timestamp = DateTime.Now.AddMinutes(-44),
                    Occurrences = 1
                },
                new Alert
                {
                    id = "456",
                    AlertType = AlertTypes.ThresholdViolation,
                    ApplicationUri = "DummyUri",
                    AverageValue = 56,
                    DisplayName = "Dummmy",
                    Timestamp = DateTime.Now
                }
            });

            var handler = new AlertHandler(alertsRepositoryMock, notificationServiceMock.Object, loggerMock.Object);

            var newAlert = new Alert
            {
                id = AlertId,
                AlertType = AlertTypes.ThresholdViolation,
                ApplicationUri = "SomeAppUri",
                AverageValue = 56,
                DisplayName = "Temperature",
                Timestamp = DateTime.Now
            };

            await handler.HandleAsync(new List<Alert> { newAlert });

            var updatedalert = alertsRepositoryMock.All.Where(alert => alert.id == AlertId).Single();
            Assert.AreEqual(2, updatedalert.Occurrences);
            Assert.AreEqual(3, alertsRepositoryMock.All.Count());
            notificationServiceMock.Verify(mock => mock.NotifyAsync(newAlert), Times.Never);
        }

        [TestMethod]
        public async Task Successfully_Recognized_That_Current_Alert_Already_Expired_New_One_Created()
        {
            const string AlertId = "123";
            var notificationServiceMock = new Mock<INotificationService>();
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();
            var alertsRepositoryMock = new AlertsRepositoryMock();

            alertsRepositoryMock.PrePopulateAlerts(new List<Alert>
            {
                new Alert
                {
                    id = AlertId,
                    AlertType = AlertTypes.ThresholdViolation,
                    ApplicationUri = "SomeAppUri",
                    AverageValue = 56,
                    DisplayName = "Temperature",
                    Timestamp = DateTime.Now.AddMinutes(-46),
                    Occurrences = 1
                }
            });

            var handler = new AlertHandler(alertsRepositoryMock, notificationServiceMock.Object, loggerMock.Object);

            var newAlert = new Alert
            {
                AlertType = AlertTypes.ThresholdViolation,
                ApplicationUri = "SomeAppUri",
                AverageValue = 56,
                DisplayName = "Temperature",
                Timestamp = DateTime.Now
            };

            await handler.HandleAsync(new List<Alert> { newAlert });

            var addedAlert = alertsRepositoryMock.All.Where(alert => alert.id != AlertId).Single();
            Assert.AreEqual(1, newAlert.Occurrences);
            Assert.AreEqual(2, alertsRepositoryMock.All.Count());
            notificationServiceMock.Verify(mock => mock.NotifyAsync(newAlert), Times.Once);
        }
    }
}
