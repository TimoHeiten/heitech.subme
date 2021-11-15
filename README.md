# heitech.subme
Mini PubSub for decoupling components in your system.
No complex logic, just simple decoupling.

## Bus 
- Publish and PublishAsync
  Works only when any subscribers for any given Type T exist (no type constraints)
  Publish is fire and forget, PublishAsync awaits all subscribers
- Subscribe and Unsubscribe
  Do whatever you guess they would ;)

  (see example below for more details)

## Logging
- Logging is set to log on the console.writeline
- You can easily overwrite it by registering two callbacks that take the formatted logmessage from this framework as input
- ```cs 
    Bus.OverrideLogging(onLost: logMessage => myLoggerInstance.Log(logMessage), onAny: logMessage => myLoggerInstance.Log(logmessage));
- onLost gets called when no Subscriber is registered for the given Type
- onAny is called for every other logging scenario of this framework

# TL;DR
- Use static class Bus.cs (or extensions) to Publish and Subscribe. 
- Subscribe on Custom types that implement ISubscriber, 
- Publish after any calls to Bus.Subscribe where set up.


# Example from Program.cs
```cs
class Program
    {
        static async Task Main(string[] args)
        {
            // no subscribers results in a logged lost message
            var msg1 = new Message1();
            var msg2 = new Message2();
            await Bus.PublishAsync(msg1);
            // create subscribers (recoginzeable with the ISubscriber Interface)
            var one = new Subscriber1();
            var two = new Subscriber2();
            var three = new Subscriber3();
            // subscribe them all to different messages
            one.Subscribe<Message1>();
            two.Subscribe<Message2>();
            // publish to message1, message2 
            await Bus.PublishAsync(msg1);
            await Bus.PublishAsync(msg2);
            System.Console.WriteLine("press enter to go further");
            Console.ReadLine();
            // register Subscriber two & three with Message1 and send all again
            two.Subscribe<Message1>();
            three.Subscribe<Message1>();
            await Bus.PublishAsync(msg1);
            await Bus.PublishAsync(msg2);
            // unsub for one and two then publish all again
            System.Console.WriteLine("now do unsubscribe for sub1 and sub2");
            one.UnSubscribe<Message1>();
            two.UnSubscribe<Message1>();
            await msg1.PublishAsync(); // also as extension method
            await msg2.PublishAsync();
            // stop logging and send message2 again
            Bus.OverrideLogging(null, null);
            await msg1.PublishAsync();
            msg2.Publish();
            await Task.Delay(150);

            // multiple registration und unsub does no harm at all
            one.Subscribe<Message1>();
            one.Subscribe<Message1>();
            one.Subscribe<Message1>();
            one.Subscribe<Message1>();
            one.UnSubscribe<Message1>();
            one.UnSubscribe<Message1>();
            one.UnSubscribe<Message1>();
            await Task.Delay(150);
            System.Console.WriteLine("done testing!");
        }

        private class Message1 { } 
        private class Message2 { } 



        static Task NowReceiving(ISubscriber s, object o)
        {
            System.Console.WriteLine($"{s.GetType().Name} received message of type: '{o.GetType().Name}'");
            return Task.CompletedTask;
        } 

        public class Subscriber1 : ISubscriber
        {
            public Task ReceiveAsync<T>(T msg) => NowReceiving(this, msg);
        }

        public class Subscriber2 : ISubscriber
        {
            public Task ReceiveAsync<T>(T msg) => NowReceiving(this, msg);
        }

        public class Subscriber3 : ISubscriber
        {
            public Task ReceiveAsync<T>(T msg) => NowReceiving(this, msg);
        }
    }
