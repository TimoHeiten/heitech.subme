using System;
using System.Linq;

namespace heitech.subme.core
{
    public class LogMessage
    {
        protected Type LogContentType { get; }
        protected LogMessage(Type t) => LogContentType = t;
        private static string HL(string content)
        {
            int markerWidth = content.Length > 90 ? content.Length : 90;
            string time = $"'heitech.subme' -- {DateTimeOffset.UtcNow.ToString()}";
            return string.Join(Environment.NewLine, new[] { "", "".PadRight(markerWidth, '-'), time, content, "".PadRight(markerWidth, '-'), "" });
        }
        internal string Format(string[] subscriberNames = null)
        {
            var subs = subscriberNames ?? new string[] { };
            string time = DateTimeOffset.UtcNow.ToString();
            if (this is LostMessage lost)
            {
                return HL($"Message of type '{lost.LogContentType}' had no subscribers attached.");
            }
            else if (this is UnSubMessage unSub)
            {
                string subscriber = subscriberNames?.FirstOrDefault() ?? "-no name supplied-";
                return HL($"Subscriber of type '{subscriber}' was unsubscribed from '{unSub.LogContentType}'");
            }
             else if (this is SubscribeMessage sub)
            {
                string subscriber = subscriberNames?.FirstOrDefault() ?? "-no name supplied-";
                return HL($"Subscriber of type '{subscriber}' has subscribed to '{sub.LogContentType}'");
            }
            else
            {
                string formattedSubnames = string.Join(", ", subs);
                return HL($"Message of type '{this.LogContentType}' was sent to the following subscribers:{Environment.NewLine}'{formattedSubnames}'");
            }
        }

        internal static LogMessage Lost(object o) => new LostMessage(o.GetType());
        internal static LogMessage Log(object o) => new LogSentMessage(o.GetType());
        internal static LogMessage Unsub(ISubscriber o) => new UnSubMessage(o.GetType());
        internal static LogMessage Subscribe(ISubscriber o) => new SubscribeMessage(o.GetType());

        private class LostMessage : LogMessage
        { public LostMessage(Type t) : base(t) { } }

        private class LogSentMessage : LogMessage
        { public LogSentMessage(Type t) : base(t) { } }

        private class UnSubMessage : LogMessage
        { public UnSubMessage(Type t) : base(t) { } }

        private class SubscribeMessage : LogMessage
        { public SubscribeMessage(Type t) : base(t) { } }
    }
}
