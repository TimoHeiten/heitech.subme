using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace heitech.subme.core
{
    public static class Bus
    {
        #region fields & ctor
        private static Dictionary<Type, List<ISubscriber>> _subscribers;
        private static object _loggingToken = new object();
        private static object _subscriberToken = new object();

        private static Action<string> _onLost;
        private static Action<string> _onAny;

        static Bus()
        {
            _subscribers = new Dictionary<Type, List<ISubscriber>>();
            _onLost = new Action<string>((s) => System.Console.WriteLine(s));
            _onAny = new Action<string>((s) => System.Console.WriteLine(s));
        }
        #endregion

        #region Publish
        ///<summary>
        ///Publish a message to all registered Subscribers. Make sure any exist before publishing, or the message gets lost. 
        /// <para>Method is async void, so all Subscribers are handled in a fire and forget way</para>
        ///</summary>
        public static async void Publish<T>(this T message)
            => await PublishAsync(message);

         ///<summary>
        ///Publish in blocking mode: Publishes a message to all registered Subscribers. Make sure any exist before publishing, or the message gets lost. 
        /// <para>Method is async void, so all Subscribers are handled in a fire and forget way</para>
        ///</summary>
        public static async Task PublishAsync<T>(this T message)
        {
            if (message == null)
                return;

            bool hasSubs = _subscribers.TryGetValue(typeof(T), out List<ISubscriber> subscribers);
            var log = LogMessage.Log(message);
            if (hasSubs)
            {
                _onAny?.Invoke(log.Format(subscribers.Select(x => x.GetType().Name).ToArray()));
                await Task.WhenAll(subscribers.Select(x => x.ReceiveAsync(message)).ToList());
            }
            else
            {
                log = LogMessage.Lost(message);
                _onLost?.Invoke(log.Format());
            }
        }
        #endregion

        #region Subscribe
        ///<summary>
        ///Register the type you want to subscribe for and handle it inside your ReceiveAsync method inside subscriber
        ///<para>Multiple registration is handled gracefully</para>
        ///</summary>
        public static void Subscribe<T>(this ISubscriber receiver)
        {
            if (receiver == null)
                return;

            lock (_subscriberToken)
            {
                if (!_subscribers.TryGetValue(typeof(T), out List<ISubscriber> subs))
                {
                    subs = new List<ISubscriber>();
                    _subscribers.Add(typeof(T), subs);
                }
                if (subs.Any(x => x.GetType() == receiver.GetType()))
                {
                    return;
                }
                _onAny?.Invoke(LogMessage.Subscribe(receiver).Format(new [] { receiver.GetType().ToString() }));
                subs.Add(receiver);
            }
        }

        ///<summary>
        ///Stop Subscription for Subscriber and given Type so you no longer receive those messages
        ///<para>Multiple unsubscription is handled gracefully</para>
        ///</summary>
        public static void UnSubscribe<T>(this ISubscriber receiver)
        {
            if (receiver == null)
                return;

            lock (_subscriberToken)
            {
                if (!_subscribers.TryGetValue(typeof(T), out List<ISubscriber> subs))
                {
                    subs = new List<ISubscriber>();
                    _subscribers.Add(typeof(T), subs);
                }
                var single = subs.SingleOrDefault(x => x.GetType() == receiver.GetType());
                if (single != null)
                {
                    _onAny?.Invoke(LogMessage.Unsub(receiver).Format(new [] { receiver.GetType().ToString() }));
                    subs.Remove(single);
                }
            }
        }
        #endregion

        #region Logging

        ///<summary>
        /// Override Logging for lost and all sent messages. Default writes to console. Assign null to silence logging
        ///</summary>
        public static void OverrideLogging(Action<string> onLost, Action<string> onAny = null)
        {
            lock (_loggingToken)
            {
                _onLost = onLost;
                _onAny = onAny;
            }
        }
        #endregion
    }
}
