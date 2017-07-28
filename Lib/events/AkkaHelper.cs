using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Akka;
using Akka.Actor;
using Lib.core;
using Lib.helper;
using Lib.extension;
using System.Threading;

namespace Lib.events
{
    public class Greet
    {
        public Greet(string who)
        {
            Who = who;
        }
        public string Who { get; private set; }
    }

    public class GreetingActor : ReceiveActor
    {
        public GreetingActor()
        {
            this.Receive<Greet>(greet =>
            {
                Console.WriteLine("Hello {0}", greet.Who);
            });
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Create a new actor system (a container for your actors)
            var system = ActorSystem.Create("MySystem");

            // Create your actor and get a reference to it.
            // This will be an "ActorRef", which is not a
            // reference to the actual actor instance
            // but rather a client or proxy to it.
            var greeter = system.ActorOf<GreetingActor>("greeter");

            // Send a message to the actor
            greeter.Tell(new Greet("World"));

            var answer = greeter.Ask<string>(new Greet(""));

            // This prevents the app from exiting
            // before the async work is done
            Console.ReadLine();
        }
    }

    /// <summary>
    /// akka system manager
    /// </summary>
    public class AkkaSystemManager : StaticClientManager<ActorSystem>
    {
        public static readonly AkkaSystemManager Instance = new AkkaSystemManager();

        public override string DefaultKey => "default_actor_system";

        public override bool CheckClient(ActorSystem ins)
        {
            return ins != null;
        }

        public override ActorSystem CreateNewClient(string key)
        {
            return ActorSystem.Create(key);
        }

        public override void DisposeClient(ActorSystem ins)
        {
            ins?.Dispose();
        }
    }

    public static class AkkaHelper<T> where T : ActorBase, new()
    {
        public static ActorSystem Container => AkkaSystemManager.Instance.DefaultClient;

        public static IActorRef GetActor(string name = null)
        {
            if (ValidateHelper.IsPlumpString(name))
            {
                return Container.ActorOf<T>(name);
            }
            else
            {
                return Container.ActorOf<T>();
            }
        }

        public static void Tell(object data, string actor_name = null)
        {
            GetActor(actor_name).Tell(data);
        }

        public static async Task<Answer> Ask<Answer>(object data, string actor_name = null,
            TimeSpan? timeout = null, CancellationToken? cancellationToken = null)
        {
            var actor = GetActor(actor_name);
            if (timeout != null && cancellationToken != null)
            {
                return await actor.Ask<Answer>(data, timeout.Value, cancellationToken.Value);
            }
            if (timeout != null)
            {
                return await actor.Ask<Answer>(data, timeout: timeout.Value);
            }
            if (cancellationToken != null)
            {
                return await actor.Ask<Answer>(data, cancellationToken: cancellationToken.Value);
            }

            return await GetActor(actor_name).Ask<Answer>(data);
        }

    }
}
