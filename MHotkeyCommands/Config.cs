using Newtonsoft.Json;
using Rocket.API;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MHotkeyCommands
{
    public class Config : IRocketPluginConfiguration
    {
        public bool Verbose;
        public int MaxCommandsPerBind;
        public List<ConfigDefaultKeys> DefaultBinds;
        public void LoadDefaults()
        {
            Verbose = true;
            MaxCommandsPerBind = 3;
            DefaultBinds = new List<ConfigDefaultKeys>()
            {
                new ConfigDefaultKeys()
                {
                    Key = "PunchLeft",
                    Commands = new List<string>() { "I punched Left!", "You can too!" }
                }
            };
        }
    }

    public class ConfigDefaultKeys
    {
        [XmlAttribute("Key")]
        public string Key;
        public List<string> Commands;
    }

    public class PlayerDB
    {
        private DataStorage<Dictionary<ulong, PlayerBinds>> DataStorage { get; set; }
        public Dictionary<ulong, PlayerBinds> data { get; private set; }
        public PlayerDB()
        {
            DataStorage = new DataStorage<Dictionary<ulong, PlayerBinds>>(MHotkeyCommands.Instance.Directory, "Binds.json");
        }
        public void Reload()
        {
            data = DataStorage.Read();
            if (data == null)
            {
                data = new Dictionary<ulong, PlayerBinds>();
                DataStorage.Save(data);
            }
            MHotkeyCommands.Instance.CLog("Reloaded the binds database");
        }

        public void Save(Dictionary<ulong, PlayerBinds> dict)
        {
            data = dict;
        }

        public void CommitToFile()
        {
            MHotkeyCommands.Instance.CLog("Saved the binds database");
            DataStorage.Save(data);
        }
    }

    public class PlayerBinds
    {
        public List<string> Jump;
        public List<string> Crouch;
        public List<string> Prone;
        public List<string> Sprint;
        public List<string> LeanLeft;
        public List<string> LeanRight;
        public List<string> PluginKey1;
        public List<string> PluginKey2;
        public List<string> PluginKey3;
        public List<string> PluginKey4;
        public List<string> PluginKey5;
        // below is all taken care of by event UnturnedPlayerEvents_OnPlayerUpdateGesture
        public List<string> InventoryOpen;
        public List<string> InventoryClose;
        public List<string> Pickup;
        public List<string> PunchLeft;
        public List<string> PunchRight;
        public List<string> SurrenderStart;
        public List<string> SurrenderStop;
        public List<string> Point;
        public List<string> Wave;
        public List<string> Salute;
        public List<string> Arrest_Start;
        public List<string> Arrest_Stop;
        public List<string> Rest_Start;
        public List<string> Rest_Stop;
        public List<string> Facepalm;
    }
    public class DataStorage<T> where T : class
    {
        public string DataPath { get; private set; }
        public DataStorage(string dir, string fileName)
        {
            DataPath = Path.Combine(dir, fileName);
        }

        public void Save(T obj)
        {
            string objData = JsonConvert.SerializeObject(obj, Formatting.Indented);

            using (StreamWriter stream = new StreamWriter(DataPath, false))
            {
                stream.Write(objData);
            }
        }

        public T Read()
        {
            if (File.Exists(DataPath))
            {
                string dataText;
                using (StreamReader stream = File.OpenText(DataPath))
                {
                    dataText = stream.ReadToEnd();
                }
                return JsonConvert.DeserializeObject<T>(dataText);
            }
            else
            {
                return null;
            }
        }
    }
}
