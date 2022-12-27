using Rocket.API;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MHotkeyCommands
{
    public class CommandHotkey : IRocketCommand
    {
        public AllowedCaller AllowedCaller => AllowedCaller.Player;

        public string Name => "Hotkey";

        public string Help => "Manage commands/chat messages bound to gestures";

        public string Syntax => "/Hotkey <delete> <key> | <add/set> <key> <command or msg> | <list> <keys/bound> (key) | /hotkey <unbindall>";

        public List<string> Aliases => new List<string>();

        public List<string> Permissions => new List<string>() { "Hotkey" };

        public void Execute(IRocketPlayer caller, string[] command)
        {
            var id = (ulong)(caller as UnturnedPlayer).CSteamID;
            if (!MHotkeyCommands.Instance.Binds.data.ContainsKey(id))
            {
                MHotkeyCommands.Instance.Binds.data[id] = new PlayerBinds();
                MHotkeyCommands.Instance.Binds.data[id].Settings = new BindsSettings();
                if (caller.HasPermission("Binds.Save"))
                {
                    MHotkeyCommands.Instance.Binds.data[id].Settings.ShouldSave = true;
                }
                else
                {
                    MHotkeyCommands.Instance.Binds.data[id].Settings.ShouldSave = false;
                }
            }
            if (command.Length == 1 && command[0].ToLower() == "unbindall")
            {
                foreach (var k in MHotkeyCommands.Keys)
                {
                    MHotkeyCommands.Instance.Binds.data[id].GetType().GetField(k).SetValue(MHotkeyCommands.Instance.Binds.data[id], null);
                }
                UnturnedChat.Say(caller, $"Removed all keybinds");
                return;
            }
            if (command.Length < 2)
            {
                UnturnedChat.Say(caller, Syntax);
                return;
            }
            if (command[0].ToLower() == "list")
            {
                if (command[1].ToLower() == "keys")
                {
                    UnturnedChat.Say(caller, $"Available hotkeys to bind to: {string.Join(", ", MHotkeyCommands.Keys)}");
                    return;
                }
                else if (command[1].ToLower() == "bound")
                {
                    if (command.Length < 3) // lists all bound keys since none are specified
                    {
                        Dictionary<string, List<string>> boundKeys = new Dictionary<string, List<string>>();
                        foreach (var k in MHotkeyCommands.Keys)
                        {
                            var bind = MHotkeyCommands.Instance.Binds.data[id].GetType().GetField(k).GetValue(MHotkeyCommands.Instance.Binds.data[id]);
                            if (bind != null) boundKeys[k] = (List<string>)bind;
                        }
                        if (boundKeys.Count < 1)
                        {
                            UnturnedChat.Say(caller, "You do not have any keys bound");
                            return;
                        }
                        UnturnedChat.Say(caller, "You have the following keys bound:");
                        foreach(var b in boundKeys)
                        {
                            UnturnedChat.Say(caller, $"<color=#ff0000>{b.Key}</color>: {string.Join(" | ", b.Value)}", true);
                        }
                        return;
                    }
                    if (MHotkeyCommands.Keys.Contains(command[2]))
                    {
                        var myBind = MHotkeyCommands.Instance.Binds.data[id].GetType().GetField(command[2]).GetValue(MHotkeyCommands.Instance.Binds.data[id]);
                        if (myBind == null)
                        {
                            UnturnedChat.Say(caller, $"You do not have anything bound on key {command[2]}");
                            return;
                        }
                        UnturnedChat.Say(caller, $"Commands/messages bound to key {command[2]}: {string.Join(", ", (myBind as List<string>))}");
                        return;
                    }
                    else
                    {
                        UnturnedChat.Say(caller, $"Invalid key name! Use one of the following: {string.Join(", ", MHotkeyCommands.Keys)}");
                        return;
                    }
                }
                else
                {
                    UnturnedChat.Say(caller, Syntax);
                    return;
                }
            }
            else if (command[0].ToLower() == "add" || command[0].ToLower() == "set" || command[0].ToLower() == "bind")
            {
                if (command.Length < 3)
                {
                    UnturnedChat.Say(caller, Syntax);
                    return;
                }
                string cmd = "";
                for (int i = 2; i < command.Length; i++)
                {
                    cmd += " " + command[i];
                }
                if (cmd.ElementAt(0) == ' ') cmd = cmd.Remove(0, 1);
                cmd = cmd.Replace("\'", "\"");
                if (!MHotkeyCommands.Keys.Contains(command[1]))
                {
                    UnturnedChat.Say(caller, $"Invalid key name! Use one of the following: {string.Join(", ", MHotkeyCommands.Keys)}");
                    return;
                }
                List<string> binds;
                var thing = MHotkeyCommands.Instance.Binds.data[id].GetType().GetField(command[1]).GetValue(MHotkeyCommands.Instance.Binds.data[id]);
                if (thing == null)
                {
                    binds = new List<string>();
                }
                else
                {
                    binds = thing as List<string>;
                }
                if (binds.Count >= MHotkeyCommands.Instance.Configuration.Instance.MaxCommandsPerBind)
                {
                    UnturnedChat.Say(caller, "You cannot add any more commands to that key!");
                    return;
                }
                if (command[0].ToLower() == "set") binds.Clear();
                binds.Add(cmd);
                MHotkeyCommands.Instance.Binds.data[id].GetType().GetField(command[1]).SetValue(MHotkeyCommands.Instance.Binds.data[id], binds);
                UnturnedChat.Say(caller, $"{(command[0].ToLower() == "set" ? "Set" : "Added")} the bind \'{cmd}\' to key {command[1]}");
                return;
            }
            else if (command[0].ToLower() == "delete" || command[0].ToLower() == "remove" || command[0].ToLower() == "unbind")
            {
                if (!MHotkeyCommands.Keys.Contains(command[1]))
                {
                    UnturnedChat.Say(caller, $"Invalid key name! Use one of the following: {string.Join(", ", MHotkeyCommands.Keys)}");
                    return;
                }
                MHotkeyCommands.Instance.Binds.data[id].GetType().GetField(command[1]).SetValue(MHotkeyCommands.Instance.Binds.data[id], null);
                UnturnedChat.Say(caller, $"Removed the bind on key {command[1]}");
                return;
            }
            else
            {
                UnturnedChat.Say(caller, Syntax);
                return;
            }
        }
    }
}
