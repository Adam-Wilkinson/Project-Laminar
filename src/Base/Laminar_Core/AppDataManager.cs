using Laminar_Core.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar_Core
{
    class AppDataManager : IUserDataStore
    {
        private static readonly string AppDataLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Project Laminar");
        private static JsonSerializerSettings JsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
        };

        static AppDataManager()
        {
            if (!Directory.Exists(AppDataLocation))
            {
                Directory.CreateDirectory(AppDataLocation);
            }
        }

        public T Load<T>(string fileName)
        {
            if (!File.Exists(Path.Combine(AppDataLocation, fileName)))
            {
                Debug.WriteLine($"File {fileName} does not exist");
                return default;
            }

            string json = File.ReadAllText(Path.Combine(AppDataLocation, fileName));

            return JsonConvert.DeserializeObject<T>(json, new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public void Save(string fileName, object toSave)
        {
            string json = JsonConvert.SerializeObject(toSave, Formatting.Indented, JsonSettings);

            using (var stream = File.CreateText(Path.Combine(AppDataLocation, fileName)))
            {
                stream.Write(json);
            }
        }

        public bool TryLoad<T>(string fileName, out T loaded)
        {
            if (!File.Exists(Path.Combine(AppDataLocation, fileName)))
            {
                loaded = default;
                return false;
            }

            string json = File.ReadAllText(Path.Combine(AppDataLocation, fileName));
            loaded = JsonConvert.DeserializeObject<T>(json, JsonSettings);
            return true;
        }
    }
}
