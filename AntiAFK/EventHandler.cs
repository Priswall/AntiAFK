using System;
using System.Collections.Generic;
using MEC;
using Player = Exiled.API.Features.Player;
using Exiled.Events.EventArgs;
using Exiled.API.Features;
using Exiled.API.Extensions;

namespace AntiAFK
{
    internal class EventHandler
    {
        private Plugin<Config> plugin;
        private Dictionary<Player, List<float[]>> AFKState = new Dictionary<Player, List<float[]>>();
        private List<Player> players = new List<Player>();
        private List<CoroutineHandle> AFKLoop = new List<CoroutineHandle>();

        public EventHandler(Plugin<Config> plugin) => this.plugin = plugin;

        private void ResetAFKTime(float newValue, out float oldValue, out float AFKTime)
        {
            oldValue = newValue;
            AFKTime = 0.0f;
        }

        private void CheckAFK()
        {
            foreach (Player player in players)
            {
                // Skip is the player reference doesn't exist
                if (player is null) continue;

                // Do nothing if player isn't actually playing the game
                if (player.Role == RoleType.Spectator)
                {
                    AFKState[player][2][0]++;
                }
                else
                {
                    AFKState[player][2][0]++;

                    // Reset AFK time if player moves or rotates their camera
                    if (player.Position.x != AFKState[player][0][0]) ResetAFKTime(player.Position.x, out AFKState[player][0][0], out AFKState[player][2][0]);
                    if (player.Position.y != AFKState[player][0][1]) ResetAFKTime(player.Position.y, out AFKState[player][0][1], out AFKState[player][2][0]);
                    if (player.Position.z != AFKState[player][0][2]) ResetAFKTime(player.Position.z, out AFKState[player][0][2], out AFKState[player][2][0]);
                    if (player.CameraTransform.rotation.x != AFKState[player][1][0]) ResetAFKTime(player.CameraTransform.rotation.x, out AFKState[player][1][0], out AFKState[player][2][0]);
                    if (player.CameraTransform.rotation.y != AFKState[player][1][1]) ResetAFKTime(player.CameraTransform.rotation.y, out AFKState[player][1][1], out AFKState[player][2][0]);

                    // Warning broadcasting
                    if(AFKState[player][2][0] >= plugin.Config.AfkTime - (plugin.Config.TimeAfterWarning * plugin.Config.NumWarnings) && AFKState[player][2][0] <= plugin.Config.AfkTime - plugin.Config.TimeAfterWarning)
                    {
                        for(int i = plugin.Config.NumWarnings; i > 0; i--)
                        {
                            if(AFKState[player][2][0] == plugin.Config.AfkTime - (plugin.Config.TimeAfterWarning * i)) {

                                // Format the warning broadcast
                                string message = plugin.Config.WarningMessage.Replace("%afk%", AFKState[player].ToString());
                                message = message.Replace("%seconds%", (plugin.Config.AfkTime - AFKState[player][2][0]).ToString());
                                message = message.Replace("%minutes%", Math.Ceiling((plugin.Config.AfkTime - AFKState[player][2][0]) / 60.0f).ToString());

                                player.Broadcast(plugin.Config.WarningBroadcastDuration, message, Broadcast.BroadcastFlags.Normal, true);
                            }
                        }
                    }
                    // Coundown broadcasting
                    else if (AFKState[player][2][0] > plugin.Config.AfkTime - plugin.Config.CountdownLength)
                    {
                        // Format again for countdown message
                        string message = plugin.Config.CountdownMessage.Replace("%time%", (plugin.Config.AfkTime - AFKState[player][2][0]).ToString());

                        player.Broadcast(2, message, Broadcast.BroadcastFlags.Normal, true);
                    }

                    // Kick and replace with the oldest spectating player
                    if (AFKState[player][2][0] >= plugin.Config.AfkTime)
                    {
                        Player oldestSpectator = null;
                        foreach(Player p in players)
                        {
                            if(p == null) continue;
                            if(oldestSpectator == null && p.Role == RoleType.Spectator)
                            {
                                oldestSpectator = p;
                                continue;
                            }
                            if(p.Role == RoleType.Spectator && AFKState[p][2][0] > AFKState[oldestSpectator][2][0])
                            {
                                oldestSpectator = p;
                            }
                        };
                        if(oldestSpectator != null)
                        {
                            oldestSpectator.SetRole(player.Role);
                            oldestSpectator.Broadcast(5, plugin.Config.AfkReplaceBroadcast);
                        }
                        player.Kill("You were AFK for too long", "");
                        player.CustomInfo += ".K4AFK.";
                        Timing.CallDelayed(0.05f, () => { player.Kick("You were AFK for too long"); player.CustomInfo.Replace(".K4AFK.", ""); });
                    }
                }
            };

            // Keep the cycle going
            AFKLoop.Add(Timing.CallDelayed(1.0f, () => CheckAFK()));
            Timing.KillCoroutines(AFKLoop[0]);
        }

        public void OnRoundStarted()
        {
            // Start the cycle
            AFKLoop.Add(Timing.CallDelayed(2.0f, () => CheckAFK()));
        }

        public void Destroy()
        {
            // Clear everything
            foreach (CoroutineHandle loop in AFKLoop) Timing.KillCoroutines(loop);
            foreach(Player player in players)
            {
                if(player != null) AFKState[player].Clear();
            }
            AFKState.Clear();
            players.Clear();
        }

        public void OnRoundEnded(RoundEndedEventArgs ev)
        {
            Destroy();
        }

        public void OnVerified(VerifiedEventArgs ev)
        {
            //                                          /--------=::Position::=--------\  /-----=::Rotation::=-----\  /--=::AFK time::=--\
            AFKState.Add(ev.Player, new List<float[]> { new float[] { 0.0f, 0.0f, 0.0f }, new float[] { 0.0f, 0.0f }, new float[] { 0.0f } });
            players.Add(ev.Player);
        }

        public void OnLeft(LeftEventArgs ev)
        {
            AFKState[ev.Player].Clear();
            AFKState.Remove(ev.Player);
            players.Remove(ev.Player);
        }

        public void OnDied(DiedEventArgs ev)
        {
            if (AFKState.ContainsKey(ev.Target) && ev.Killer != null)
            {
                if (ev.Killer.Role == RoleType.Scp049)
                {
                    AFKState[ev.Target][2][0] = -10;
                }
                else
                {
                    AFKState[ev.Target][2][0] = 0;
                }
            }
        }

        public void OnSpawning(SpawningEventArgs ev)
        {
            AFKState[ev.Player][2][0] = 0;
        }

        public void OnAnnouncingScpTermination(AnnouncingScpTerminationEventArgs ev)
        {
            if (ev.Player.CustomInfo.Contains(".K4AFK.")) ev.IsAllowed = false;
        }

        public void OnSpawningRagdoll(SpawningRagdollEventArgs ev)
        {
            if (ev.Owner.CustomInfo.Contains(".K4AFK.")) ev.IsAllowed = false;
        }
    }
}
