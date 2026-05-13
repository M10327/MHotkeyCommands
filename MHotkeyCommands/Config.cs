using Newtonsoft.Json;
using Rocket.API;
using Rocket.Core.Logging;
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
        public List<string> AllowedKeys;
        public List<ConfigDefaultKeys> DefaultBinds;
        public void LoadDefaults()
        {
            Verbose = true;
            MaxCommandsPerBind = 3;
            AllowedKeys = new List<string>()
            {
                "Jump",
                "Crouch",
                "Prone",
                "Sprint",
                "LeanLeft",
                "LeanRight",
                "CodeHotkey1",
                "CodeHotkey2",
                "CodeHotkey3",
                "CodeHotkey4",
                "CodeHotkey5",
                "SteadyAim",
                "InventoryOpen",
                "InventoryClose",
                "Pickup",
                "PunchLeft",
                "PunchRight",
                "SurrenderStart",
                "SurrenderStop",
                "Point",
                "Wave",
                "Salute",
                "Arrest_Start",
                "Arrest_Stop",
                "Rest_Start",
                "Rest_Stop",
                "Facepalm"
            };
            DefaultBinds = new List<ConfigDefaultKeys>()
            {
                new ConfigDefaultKeys()
                {
                    Key = "CodeHotkey1",
                    Commands = new List<string>() { "/tpa a" }
                },
                new ConfigDefaultKeys()
                {
                    Key = "CodeHotkey2",
                    Commands = new List<string>() { "/tpa d" }
                },
                new ConfigDefaultKeys()
                {
                    Key = "CodeHotkey4",
                    Commands = new List<string>() { "/mark" }
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
            Logger.Log("Reloaded the binds database");
        }

        public void Save(Dictionary<ulong, PlayerBinds> dict)
        {
            data = dict;
        }

        public void CommitToFile()
        {
            Logger.Log("Saved the binds database");
            DataStorage.Save(data);
        }
    }

    public class PlayerBinds
    {
        public PlayerBinds()
        {
            Keys = new Dictionary<string, List<string>>();
        }
        public Dictionary<string, List<string>> Keys {  get; set; }
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
