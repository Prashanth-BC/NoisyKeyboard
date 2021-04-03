using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoisyKeyboard
{
    class Config
    {
        public double volumeAtStart { get; set; } = 10.0;
        public string soundAtStart { get; set; } ="Mechanical2";
        public Dictionary<String, int> ignoreKeys = new Dictionary<string, int>{{ "CAPS_LOCK", 20 },
       { "ARROW_LEFT", 37 },
       { "ARROW_UP", 38 },
       { "ARROW_RIGHT", 39 },
       { "ARROW_DOWN", 40 },
       { "SHIFT_LEFT", 160 },
       { "SHIFT_RIGHT", 161 },
       { "CTRL_LEFT", 162 },
       { "CTRL_RIGHT", 163 },
       { "ALT_LEFT", 164 },
       { "ALT_GR", 162 },
       { "ALT_RIGHT", 165 },
       { "DRUCK", 44 },
       { "FUNCTION", 255 } };

        public string actionAtStart = "No Sound";

        [JsonIgnore]
        private static Config _inst;
        [JsonIgnore]
        private static string _configFileName = @"./data/config.json";

        private Config() { }

        public static Config GetConfig()
        {
            if(_inst == null)
            {
                _inst = new Config();
                using (StreamReader file = File.OpenText(_configFileName))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    _inst = (Config)serializer.Deserialize(file, typeof(Config));
                }
            }
            
            return _inst;
            
        }
        public static void SaveConfig()
        {
            using (StreamWriter file = File.CreateText(_configFileName)) { 
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, _inst);
            }
        }
    }
}
