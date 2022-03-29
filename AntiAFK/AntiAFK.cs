using Exiled.API.Features;
using System;
using Server = Exiled.Events.Handlers.Server;
using Player = Exiled.Events.Handlers.Player;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiAFK
{
    public class AntiAFK : Plugin<Config>
    {
        private static readonly Lazy<AntiAFK> LazyInstance = new Lazy<AntiAFK>(() => new AntiAFK());
        public static AntiAFK Instance = LazyInstance.Value;

        private EventHandler eventHandler;

        public override void OnEnabled()
        {
            eventHandler = new EventHandler();

            Player.Left += eventHandler.OnLeft;
            Player.Joined += eventHandler.OnJoined;
            Server.RoundStarted += eventHandler.OnRoundStarted;
            Server.RoundEnded += eventHandler.OnRoundEnded;
        }

        public override void OnDisabled()
        {
            Player.Left -= eventHandler.OnLeft;
            Player.Joined -= eventHandler.OnJoined;
            Server.RoundStarted -= eventHandler.OnRoundStarted;
            Server.RoundEnded -= eventHandler.OnRoundEnded;

            eventHandler.Destroy();
            eventHandler = null;
        }
    }
}
