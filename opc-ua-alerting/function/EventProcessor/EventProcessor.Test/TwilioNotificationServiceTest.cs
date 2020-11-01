using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using Moq;
using System;

namespace EventProcessor.Test
{
    [TestClass]
    public class TwilioNotificationServiceTest
    {
        [TestMethod]
        [Ignore]
        public async Task Sms_Successfully_Sent()
        {
            Environment.SetEnvironmentVariable("SEND_TEXT_MESSAGES_FEATURE_ENABLED", "True");

            var twilioConfig = new TwilioConfiguration
            {
                AccountSid = "<ACCOUNT_SID>",
                AuthToken = "<AUTH_TOKEN>",
                FromPhoneNumber = "<FROM_NUMBER>",
                ToPhoneNumber = "<TO_NUMBER>"
            };

            var alert = new Alert
            {
                AlertType = AlertTypes.ThresholdViolation,
                ApplicationUri = "TestUri",
                DisplayName = "Dummy",
                AverageValue = 55
            };
            var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger>();

            try
            {
                var twilioNotificationService = new TwilioNotificationService(twilioConfig, loggerMock.Object);
                await twilioNotificationService.NotifyAsync(alert);
            }
            catch (Exception ex)
            {
                Assert.Fail("Expected no exception, but got: " + ex.Message);
            }
        }
    }
}
