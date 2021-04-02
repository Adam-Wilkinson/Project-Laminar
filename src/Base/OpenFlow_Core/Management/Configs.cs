namespace OpenFlow_Core.Management
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Xml;

    public class Configs
    {
        private readonly string _xmlDocPath;
        private State _state;
        private XmlNode _configs;

        public Configs(string path)
        {
            _xmlDocPath = path;
            Reload();
        }

        public string ErrorState => _state switch
        {
            State.InvalidPath => "The config file could not be found in " + _xmlDocPath,
            State.InvalidFile => "The config file was found but is not valid",
            State.Unknown => "An unknown error occured loading the config file",
            State.Working => "There were no errors, the config file is valid",
            _ => "wtf bro",
        };

        public bool Valid => _state == State.Working;

        public ReadOnlyCollection<string> PluginPaths { get; private set; }

        public void Reload()
        {
            _state = State.Unknown;
            try
            {
                if (!File.Exists(_xmlDocPath))
                {
                    _state = State.InvalidPath;
                }

                XmlDocument configsDocu = new();
                configsDocu.Load(_xmlDocPath);
                if (configsDocu["Configs"] == null)
                {
                    _state = State.InvalidFile;
                }

                _configs = configsDocu["Configs"];
                _state = State.Working;
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Exception {e}; Configs not found! Invalid Config File");
            }

            LoadPluginDirectories();
        }

        public void LoadPluginDirectories()
        {
            if (Valid)
            {
                List<string> output = new();
                foreach (XmlNode path in _configs["PluginDirectories"])
                {
                    if (Directory.Exists(path.InnerText))
                    {
                        output.AddRange(Directory.GetDirectories(path.InnerText));
                    }
                }

                PluginPaths = new ReadOnlyCollection<string>(output);
            }
            else
            {
                PluginPaths = null;
            }
        }

        private enum State
        {
            InvalidPath,
            InvalidFile,
            Unknown,
            Working,
        }
    }
}
