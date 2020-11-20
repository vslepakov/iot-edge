using Microsoft.Azure.Devices.Client;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace alerting.test
{
    [TestClass]
    public class AlertProcessorTest
    {
        private const string ApplicationUri1 = "TestServer1Uri";
        private const string TempNode = "Temperature1";
        private const string HumidityNode = "Humidity1";

        [TestMethod]
        public async Task Alerts_Generated_For_Monitored_Items_Only_Values_Too_High()
        {
            IList<Alert> alertsToBeSent = null;
            var moduleClientWrapperMock = GetModuleClientWrapperMock(alerts => alertsToBeSent = alerts);

            var monitoredItems = GetMonitoredItems();

            var tempValues = new double[] { 29, 28 };
            var humidityValues = new double[] { 61, 60 };
            var dataPoints = GetDataPoints(tempValues, humidityValues);

            var target = new AlertProcessor(moduleClientWrapperMock.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            target.SetMonitoredItems(monitoredItems);
            target.HandleNewValues(dataPoints);
            await target.CheckForAlertsAsync(TimeSpan.Zero, cts.Token);

            Assert.IsNotNull(alertsToBeSent);
            Assert.AreEqual(2, alertsToBeSent.Count);
            Assert.IsTrue(alertsToBeSent.All(a => a.ApplicationUri == ApplicationUri1));
            Assert.AreEqual(tempValues.Average(), alertsToBeSent.Single(a => a.DisplayName == TempNode).AverageValue, 0.00001);
            Assert.AreEqual(humidityValues.Average(), alertsToBeSent.Single(a => a.DisplayName == HumidityNode).AverageValue, 0.00001);
        }

        [TestMethod]
        public async Task Alerts_Generated_For_Monitored_Items_Only_Values_Too_Low()
        {
            IList<Alert> alertsToBeSent = null;
            var moduleClientWrapperMock = GetModuleClientWrapperMock(alerts => alertsToBeSent = alerts);

            var monitoredItems = GetMonitoredItems();

            var tempValues = new double[] { 22, 21 };
            var humidityValues = new double[] { 61, 60 };
            var dataPoints = GetDataPoints(tempValues, humidityValues);

            var target = new AlertProcessor(moduleClientWrapperMock.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            target.SetMonitoredItems(monitoredItems);
            target.HandleNewValues(dataPoints);
            await target.CheckForAlertsAsync(TimeSpan.Zero, cts.Token);

            Assert.IsNotNull(alertsToBeSent);
            Assert.AreEqual(2, alertsToBeSent.Count);
            Assert.IsTrue(alertsToBeSent.All(a => a.ApplicationUri == ApplicationUri1));
            Assert.AreEqual(tempValues.Average(), alertsToBeSent.Single(a => a.DisplayName == TempNode).AverageValue, 0.00001);
            Assert.AreEqual(humidityValues.Average(), alertsToBeSent.Single(a => a.DisplayName == HumidityNode).AverageValue, 0.00001);
        }

        [TestMethod]
        public async Task Time_Series_Correctly_Cleaned_Up()
        {
            IList<Alert> alertsToBeSent = null;
            var moduleClientWrapperMock = GetModuleClientWrapperMock(alerts => alertsToBeSent = alerts);

            var monitoredItems = GetMonitoredItems();

            var tempValues = new double[] { 22, 21 };
            var humidityValues = new double[] { 61, 60 };
            var dataPoints = GetDataPoints(tempValues, humidityValues);

            var target = new AlertProcessor(moduleClientWrapperMock.Object);
            var cts = new CancellationTokenSource();
            cts.Cancel();

            target.SetMonitoredItems(monitoredItems);
            target.HandleNewValues(dataPoints);
            await target.CheckForAlertsAsync(TimeSpan.Zero, cts.Token);

            Assert.IsNotNull(alertsToBeSent);
            Assert.AreEqual(2, alertsToBeSent.Count);

            target.SetMonitoredItems(monitoredItems.Where(item => item.DisplayName == TempNode).ToList());
            target.HandleNewValues(dataPoints);
            await target.CheckForAlertsAsync(TimeSpan.Zero, cts.Token);

            Assert.IsNotNull(alertsToBeSent);
            Assert.AreEqual(1, alertsToBeSent.Count);
            Assert.AreEqual(ApplicationUri1, alertsToBeSent.First().ApplicationUri);
            Assert.AreEqual(TempNode, alertsToBeSent.First().DisplayName);
        }

        #region Helpers

        private IList<MonitoredItem> GetMonitoredItems()
        {
            return new List<MonitoredItem>
            {
                new MonitoredItem
                {
                    ApplicationUri = ApplicationUri1,
                    NodeId = TempNode,
                    DisplayName = TempNode,
                    ThresholdValue = 25,
                    ToleranceHigh = 3,
                    ToleranceLow = 3
                },
                new MonitoredItem
                {
                    ApplicationUri = ApplicationUri1,
                    NodeId = HumidityNode,
                    DisplayName = HumidityNode,
                    ThresholdValue = 60,
                    ToleranceHigh = 0,
                    ToleranceLow = 0
                }
            };
        }

        private IList<OpcUaDataPoint> GetDataPoints(double[] tempValues, double[] humidityValues)
        {
            var result = new List<OpcUaDataPoint>
            {
                new OpcUaDataPoint
                {
                    ApplicationUri = "TestServer2Uri",
                    NodeId = "FakeNode",
                    DisplayName = "FakeNode",
                    Value = new OpcUaValue
                    {
                        SourceTimestamp = DateTime.Now,
                        Value = 600
                    }
                }
            };

            for (var index = 0; index < tempValues.Length; index++)
            {
                result.Add(new OpcUaDataPoint
                {
                    ApplicationUri = ApplicationUri1,
                    NodeId = TempNode,
                    DisplayName = TempNode,
                    Value = new OpcUaValue
                    {
                        SourceTimestamp = DateTime.Now.AddSeconds(index * -1),
                        Value = tempValues[index]
                    }
                });
            }

            for (var index = 0; index < humidityValues.Length; index++)
            {
                result.Add(new OpcUaDataPoint
                {
                    ApplicationUri = ApplicationUri1,
                    NodeId = HumidityNode,
                    DisplayName = HumidityNode,
                    Value = new OpcUaValue
                    {
                        SourceTimestamp = DateTime.Now.AddSeconds(index * -1),
                        Value = humidityValues[index]
                    }
                });
            }

            return result;
        }

        private Mock<IModuleClientWrapper> GetModuleClientWrapperMock(Action<IList<Alert>> callBackAction)
        {
            var mock = new Mock<IModuleClientWrapper>();
            mock
                .Setup(m => m.SendEventAsync(It.IsAny<string>(), It.IsAny<Message>()))
                .Callback<string, Message>((o, m) =>
                {
                    var body = Encoding.UTF8.GetString(m.GetBytes());
                    var alertsToBeSent = JsonConvert.DeserializeObject<IList<Alert>>(body);

                    callBackAction(alertsToBeSent);
                });

            return mock;
        }

        # endregion
    }
}
