using Microsoft.VisualBasic;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace MHotkeyCommands
{
    public class MHotkeyCommands : RocketPlugin<Config>
    {
        public static MHotkeyCommands Instance { get; set; }
        public PlayerDB Binds;
        protected override void Load()
        {
            Rocket.Core.Logging.Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!");
            Rocket.Core.Logging.Logger.Log($"Permission for saving binds between sessions is \'Binds.Save\'");
            Instance = this;
            Binds = new PlayerDB();
            Binds.Reload();
            Binds.CommitToFile();
            UnturnedPlayerEvents.OnPlayerUpdateGesture += UnturnedPlayerEvents_OnPlayerUpdateGesture;
            U.Events.OnPlayerConnected += Events_OnPlayerConnected;
            PlayerInputListener.PlayerKeyInput += OnPlayerInput;
            foreach (var cl in Provider.clients)
            {
                UnturnedPlayer p = UnturnedPlayer.FromSteamPlayer(cl);
                AddDefaults(p);
            }
        }

        private void OnPlayerInput(Player player, EPlayerKey key, bool down)
        {
            if (key == EPlayerKey.HotKey1) ExecuteGesture(player, "PluginKey1");
            if (key == EPlayerKey.HotKey2) ExecuteGesture(player, "PluginKey2");
            if (key == EPlayerKey.HotKey3) ExecuteGesture(player, "PluginKey3");
            if (key == EPlayerKey.HotKey4) ExecuteGesture(player, "PluginKey4");
            if (key == EPlayerKey.HotKey5) ExecuteGesture(player, "PluginKey5");
            if (key == EPlayerKey.Jump) ExecuteGesture(player, "Jump");
            if (key == EPlayerKey.Crouch) ExecuteGesture(player, "Crouch");
            if (key == EPlayerKey.Prone) ExecuteGesture(player, "Prone");
            if (key == EPlayerKey.Sprint) ExecuteGesture(player, "Sprint");
            if (key == EPlayerKey.LeanLeft) ExecuteGesture(player, "LeanLeft");
            if (key == EPlayerKey.LeanRight) ExecuteGesture(player, "LeanRight");
            if (key == EPlayerKey.SteadyAim) ExecuteGesture(player, "SteadyAim");
        }

        private void Events_OnPlayerConnected(UnturnedPlayer p)
        {
            var inp = p.Player.gameObject.AddComponent<PlayerInputListener>();
            inp.awake = true;
            AddDefaults(p);
        }

        private void AddDefaults(UnturnedPlayer p)
        {
            if (!Configuration.Instance.ApplyDefaults) return;
            ulong id = (ulong)p.Player.channel.owner.playerID.steamID;
            if (!Binds.data.ContainsKey(id))
            {
                Binds.data[id] = new PlayerBinds();
                foreach (var cmd in Configuration.Instance.DefaultBinds)
                {
                    CLog($"{p.DisplayName} bound {string.Join(", ", cmd.Commands)} to {cmd.Key}");
                    Binds.data[id].GetType().GetField(cmd.Key).SetValue(Binds.data[id], cmd.Commands);
                }
            }
        }

        private void UnturnedPlayerEvents_OnPlayerUpdateGesture(UnturnedPlayer player, UnturnedPlayerEvents.PlayerGesture gesture)
        {
            ExecuteGesture(player.Player, gesture.ToString());
        }

        public void ExecuteGesture(Player p, string gesture)
        {
            ulong id = (ulong)p.channel.owner.playerID.steamID;
            UnturnedPlayer pl = UnturnedPlayer.FromPlayer(p);
            if (!Binds.data.ContainsKey(id)) return;
            var b = Binds.data[id];
            var command = b.GetType().GetField(gesture).GetValue(b);
            if (command == null) return;
            if (!(command is List<string>)) return;
            var cmds = command as List<string>;
            for (int i = 0; i < cmds.Count; i++)
            {
                cmds[i] = AddDynamics(cmds[i], pl, gesture);
            }
            ExecuteCommands(pl, cmds);
        }

        public string AddDynamics(string str, UnturnedPlayer p, string gesture)
        {
            if (p.HasPermission("Binds.Dynamic.GestureName") || p.HasPermission("Binds.Dynamic.*")) str = str.Replace("{G}", gesture);
            if (p.HasPermission("Binds.Dynamic.Bearing") || p.HasPermission("Binds.Dynamic.*")) str = str.Replace("{B}", Math.Floor(p.Rotation).ToString());
            if (p.HasPermission("Binds.Dynamic.Caller") || p.HasPermission("Binds.Dynamic.*"))
            {
                str = str.Replace("{C.ID}", p.CSteamID.ToString());
                str = str.Replace("{C.Pos}", p.Position.ToString());
                str = str.Replace("{C.X}", Math.Round(p.Position.x, 2).ToString());
                str = str.Replace("{C.Y}", Math.Round(p.Position.y, 2).ToString());
                str = str.Replace("{C.Z}", Math.Round(p.Position.z, 2).ToString());
                str = str.Replace("{C.Name}", p.DisplayName);
            }
            if (Regex.IsMatch(str, "[{]T[.a-zA-z]*[}]") && (p.HasPermission("Binds.Dynamic.Target") || p.HasPermission("Binds.Dynamic.*"))){
                int colliders = RayMasks.BARRICADE | RayMasks.STRUCTURE | RayMasks.PLAYER | RayMasks.ENVIRONMENT | RayMasks.GROUND2 | RayMasks.GROUND;
                Physics.Raycast(p.Player.look.aim.position, p.Player.look.aim.forward, out RaycastHit ray, 500, colliders);
                Transform target = ray.collider?.transform;
                List<Player> nearbyPlayers;
                PlayerTool.getPlayersInRadius(target.position, 0.5f, nearbyPlayers = new List<Player>());
                if (nearbyPlayers.Count < 1)
                {
                    return "ERROR_NOPLAYERFOUND";
                }
                else
                {
                    UnturnedPlayer t = UnturnedPlayer.FromPlayer(nearbyPlayers.First());
                    if (t == null) return "ERROR_NOPLAYERFOUND";
                    str = str.Replace("{T.Name}", t.DisplayName);
                    str = str.Replace("{T.ID}", t.CSteamID.ToString());
                }
            }
            return str;
        }

        public async void ExecuteCommands(UnturnedPlayer p, List<string> cmds)
        {
            foreach (var command in cmds.ToArray())
            {
                string cmd = command;
                if (cmd == "ERROR_NOPLAYERFOUND")
                {
                    UnturnedChat.Say(p.CSteamID, "No player found");
                    continue;
                }
                // TODO: make the chat mode default to the player's set chat mode, but this works for now
                byte mode = (byte)EChatMode.GLOBAL;
                if (p.HasPermission("Binds.Chatmode"))
                {
                    if (cmd.Contains("{AREA}"))
                    {
                        cmd = cmd.Replace("{AREA}", "");
                        mode = (byte)EChatMode.LOCAL;
                    }
                    else if (cmd.Contains("{GROUP}"))
                    {
                        cmd = cmd.Replace("{GROUP}", "");
                        mode = (byte)EChatMode.GROUP;
                    }
                }
                ChatManager.instance.askChat(p.CSteamID, mode, cmd);
                await Task.Delay(250); // unturned wont run all commands if i dont set this to around 250 or higher
            }
        }

        protected override void Unload()
        {
            CleanupDatabase();
            Binds.CommitToFile();
            UnturnedPlayerEvents.OnPlayerUpdateGesture -= UnturnedPlayerEvents_OnPlayerUpdateGesture;
            PlayerInputListener.PlayerKeyInput -= OnPlayerInput;
        }

        public void CleanupDatabase()
        {
            foreach(var b in Binds.data.ToArray())
            {
                var p = new RocketPlayer(b.Key.ToString());
                if (p.HasPermission("Binds.Save")) continue;
                Binds.data.Remove(b.Key);
            }
            CLog("Cleaned up database");
        }

        public void CLog(string text)
        {
            if (!Configuration.Instance.Verbose) return;
            Rocket.Core.Logging.Logger.Log(text);
        }
    }

}
