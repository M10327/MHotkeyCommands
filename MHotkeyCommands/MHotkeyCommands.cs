using Microsoft.VisualBasic;
using Rocket.API;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned;
using Rocket.Unturned.Events;
using Rocket.Unturned.Player;
using SDG.Unturned;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
        }

        private void Events_OnPlayerConnected(UnturnedPlayer p)
        {
            ulong id = (ulong)p.Player.channel.owner.playerID.steamID;
            var inp = p.Player.gameObject.AddComponent<PlayerInputListener>();
            inp.awake = true;
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
                cmds[i] = AddDynamics(cmds[i], pl);
            }
            ExecuteCommands(pl, cmds);
        }

        public string AddDynamics(string str, UnturnedPlayer p)
        {
            // TODO: dynamics
            return str;
        }

        public async void ExecuteCommands(UnturnedPlayer p, List<string> cmds)
        {
            foreach (var command in cmds.ToArray())
            {
                string cmd = command;
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
