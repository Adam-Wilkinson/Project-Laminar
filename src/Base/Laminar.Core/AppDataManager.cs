using Laminar_Core.Serialization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Laminar.Core
{
    class AppDataManager : IUserDataStore
    {
        private static readonly string AppDataLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Project Laminar");
        private static readonly JsonSerializerSettings JsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full,
        };

        static AppDataManager()
        {
            if (!Directory.Exists(AppDataLocation))
            {
                Directory.CreateDirectory(AppDataLocation);
            }
        }

        public IEnumerable<T> LoadAllFromFolder<T>(string folder, string fileType)
        {
            if (Directory.Exists(Path.Combine(AppDataLocation, folder))) 
            {
                foreach (string dir in Directory.EnumerateFiles(Path.Combine(AppDataLocation, folder), $"*.{fileType}"))
                {
                    yield return Load<T>(dir);
                }
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
            string savePath = Path.Combine(AppDataLocation, fileName);
            if (!Directory.Exists(Path.GetDirectoryName(savePath)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(savePath));
            }

            string json = JsonConvert.SerializeObject(toSave, Formatting.Indented, JsonSettings);

            using var stream = File.CreateText(Path.Combine(AppDataLocation, fileName));
            stream.Write(json);
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
