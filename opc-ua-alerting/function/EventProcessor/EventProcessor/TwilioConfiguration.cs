using System;

namespace EventProcessor
{
    public class TwilioConfiguration
    {
        public string FromPhoneNumber { get; set; }

        public string ToPhoneNumber { get; set; }

        public string AccountSid { get; set; }

        public string AuthToken { get; set; }

        public bool SendTextMessagesFeatureEnabled =>
            Environment.GetEnvironmentVariable("SEND_TEXT_MESSAGES_FEATURE_ENABLED").ToLower() == "true";
    }
}
