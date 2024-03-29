﻿using Laminar_Core.NodeSystem;
using Laminar_Core.Scripting.Advanced.Compilation;
using Laminar_Core.Scripting.Advanced.Editing;
using Laminar_Core.Scripting.Advanced.Instancing;
using Laminar_PluginFramework.Primitives;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Laminar_Core.Scripting.Advanced
{
    public class AdvancedScript : IAdvancedScript
    {
        private readonly ICompiledScriptManager _compilerManager;
        private readonly Instance _instance;
        private Dictionary<string, InputNode> _defaultInputs = new();
        private bool _isBeingEdited;

        public AdvancedScript(Instance instance, IObservableValue<string> name, ICompiledScriptManager compilationManager, IAdvancedScriptEditor editor)
        {
            _instance = instance;
            _compilerManager = compilationManager;
            Inputs = new(_defaultInputs);

            Editor = editor;
            Name = name;
            Name.Value = "Advanced Script";
        }

        public IObservableValue<string> Name { get; }

        public IAdvancedScriptEditor Editor { get; }

        public bool IsBeingEdited
        {
            get => _isBeingEdited;
            set
            {
                if (_isBeingEdited == value)
                {
                    return;
                }

                _isBeingEdited = value;
                if (_isBeingEdited)
                {
                    _compilerManager.DisableAllScripts();
                } 
                else  
                {
                    _compilerManager.Refresh(this);
                    _compilerManager.EnableAllScripts();
                    UpdateInputs();
                    _instance.SaveScript(this);
                }

                Editor.IsLive = _isBeingEdited;
            }
        }

        public ReadOnlyDictionary<string, InputNode> Inputs { get; private set; }

        public IAdvancedScriptInstance CreateInstance()
        {
            IAdvancedScriptInstance newInstance = _compilerManager.CreateInstance();
            return newInstance;
        }

        public void UpdateInputs()
        {
            _defaultInputs = Editor.Inputs.InputNodes.ToDictionary(x => x.GetNameLabel().LabelText.Value);
            Inputs = new(_defaultInputs);
        }
    }
}
