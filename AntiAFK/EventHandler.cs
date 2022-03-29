using System;
using System.Collections.Generic;
using System.Linq;
using MEC;
using Server = Exiled.API.Features.Server;
using Player = Exiled.API.Features.Player;
using Exiled.Events.EventArgs;
using System.Text;
using System.Threading.Tasks;
using Exiled.API.Features;

namespace AntiAFK
{
    internal class EventHandler
    {
        private int AFK_Time = AntiAFK.Instance.Config.AFK_Time;
        private Dictionary<Player, List<float[]>> AFKState = new Dictionary<Player, List<float[]>>();
        private List<Player> players = new List<Player>();
        private List<CoroutineHandle> AFKLoop = new List<CoroutineHandle>();

        private void CheckAFK()
        {
            Log.Info("AFK Test");
            foreach (Player player in players)
            {
                if (player.Role == RoleType.Spectator || player.Role == RoleType.Tutorial)
                {
                    AFKState[player][2][0] = 0;
                    Log.Info(player.Nickname + " is a spectator/tutorial");
                }
                else
                {
                    AFKState[player][2][0]++;
                    if (AFKState[player][2][0] >= (float)AFK_Time) player.Kick("You were AFK for too long");
                    /*
                    if (player.Position.x != AFKState[player][0][0])
                        ResetTimer(player.Position.x, out AFKState[player][0][0], out AFKState[player][2][0]);
                    if (player.Position.y != AFKState[player][0][1])
                        ResetTimer(player.Position.y, out AFKState[player][0][1], out AFKState[player][2][0]);
                    if (player.Position.z != AFKState[player][0][2])
                        ResetTimer(player.Position.z, out AFKState[player][0][2], out AFKState[player][2][0]);
                    if (player.Rotation.x != AFKState[player][1][0])
                        ResetTimer(player.Rotation.x, out AFKState[player][1][0], out AFKState[player][2][0]);
                    if (player.Rotation.y != AFKState[player][1][1])
                        ResetTimer(player.Rotation.y, out AFKState[player][1][1], out AFKState[player][2][0]);
                    if (player.Rotation.z != AFKState[player][1][2])
                        ResetTimer(player.Rotation.z, out AFKState[player][1][2], out AFKState[player][2][0]);*/

                    Log.Info(player.Nickname + " has been AFK for " + (int)AFKState[player][2][0] + " seconds");
                }
            };
            AFKLoop.Add(Timing.CallDelayed(1.0f, () => CheckAFK()));
            Timing.KillCoroutines(AFKLoop[0]);
        }

        public void OnRoundStarted()
        {
            AFKLoop.Add(Timing.CallDelayed(2.0f, () => CheckAFK()));
        }

        private void ResetTimer(float newValue, out float oldValue, out float timer)
        {
            oldValue = newValue;
            timer = 0;
        }

        public void Destroy()
        {
            if (AFKLoop != null) { foreach (CoroutineHandle loop in AFKLoop) Timing.KillCoroutines(loop); }
            if (AFKState != null) AFKState.Clear();
            if (players != null) players.Clear();
            players = null;
            AFKLoop = null;
            AFKState = null;
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Destroy();
        }

        public void OnJoined(JoinedEventArgs ev)
        {
            //                                          /-----------Position-----------\  /-----------Rotation-----------\  /-----AFK Time-----\
            AFKState.Add(ev.Player, new List<float[]> { new float[] { 0.0f, 0.0f, 0.0f }, new float[] { 0.0f, 0.0f, 0.0f }, new float[] { 0.0f } });
            players.Add(ev.Player);
        }

        public void OnLeft(LeftEventArgs ev)
        {
            AFKState.Remove(ev.Player);
            players.Remove(ev.Player);
        }
    }
}
