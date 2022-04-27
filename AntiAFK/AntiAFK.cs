using Exiled.API.Features;
using System;
using Server = Exiled.Events.Handlers.Server;
using Player = Exiled.Events.Handlers.Player;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Enums;

namespace AntiAFK
{
    public class AntiAFK : Plugin<Config>
    {
        public override string Author { get; } = "Priswall";
        public override string Name { get; } = "AntiAFK";
        public override string Prefix { get; } = "AntiAFK";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override PluginPriority Priority => PluginPriority.Higher;
        private EventHandler eventHandler;
        
        public override void OnEnabled()
        {
            eventHandler = new EventHandler(this);

            Player.Left += eventHandler.OnLeft;
            Player.Verified += eventHandler.OnVerified;
            Server.RoundStarted += eventHandler.OnRoundStarted;
            Server.RoundEnded += eventHandler.OnRoundEnded;
            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            Player.Left -= eventHandler.OnLeft;
            Player.Verified -= eventHandler.OnVerified;
            Server.RoundStarted -= eventHandler.OnRoundStarted;
            Server.RoundEnded -= eventHandler.OnRoundEnded;

            eventHandler.Destroy();
            eventHandler = null;
            base.OnDisabled();
        }
    }
}
