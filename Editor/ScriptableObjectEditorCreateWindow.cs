using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace LazyCoder.SO.Editor
{
    public class ScriptableObjectEditorCreateWindow : OdinEditorWindow
    {
        private const string AssemblyAny = "Any Assemblies";

        private static readonly string PrefsKeyAssemblyName = $"{nameof(LazyCoder.SO.Editor)}_AssemblyName";

        private static readonly string PrefsKeyTypeName = $"{nameof(LazyCoder.SO.Editor)}_TypeName";

        [ShowInInspector]
        [HideLabel]
        [HorizontalGroup("Fields")]
        [BoxGroup("Fields/Assembly", CenterLabel = true)]
        [ValueDropdown("@_assemblyNames", NumberOfItemsBeforeEnablingSearch = 0)]
        private string _assemblyName = AssemblyAny;

        [ShowInInspector]
        [HideLabel]
        [HorizontalGroup("Fields")]
        [BoxGroup("Fields/Type", CenterLabel = true)]
        [ValueDropdown("@_typeNames", NumberOfItemsBeforeEnablingSearch = 0)]
        private string _typeName = null;

        private string[] _assemblyNames;
        
        private string[] _typeNames;

        private Dictionary<string, Type[]> _typesByAssemblyName;

        protected override void OnEnable()
        {
            base.OnEnable();

            minSize = new Vector2(500f, 170f);
            maxSize = minSize;
            maxSize = new Vector2(1000f, 170f);

            _typesByAssemblyName = new Dictionary<string, Type[]>();

            foreach (Assembly assembly in GetAllScriptAssemblies())
            {
                Type[] types = assembly.ExportedTypes
                    .Where(SelectType)
                    .ToArray();

                if (types.Length == 0)
                    continue;

                string assemblyName = assembly.GetName().Name;

                _typesByAssemblyName[assemblyName] = types;
            }

            _assemblyNames = new string[_typesByAssemblyName.Count + 1];
            _assemblyNames[0] = AssemblyAny;
            _typesByAssemblyName.Keys.CopyTo(_assemblyNames, 1);

            LoadPreferences();

            bool SelectType(Type type)
            {
                return type.IsSubclassOf(typeof(ScriptableObject)) &&
                       !type.IsAbstract &&
                       !type.IsSubclassOf(typeof(StateMachineBehaviour)) &&
                       !type.IsSubclassOf(typeof(UnityEditor.Editor)) &&
                       !type.IsSubclassOf(typeof(EditorWindow));
            }
        }

        protected override void OnImGUI()
        {
            EditorGUI.BeginChangeCheck();

            base.OnImGUI();

            EditorGUI.EndChangeCheck();

            if (GUI.changed)
            {
                UpdateTypeName();
                SavePreferences();
            }
        }

        private void UpdateTypeName()
        {
            Type[] types = null;

            if (_assemblyName == AssemblyAny)
            {
                types = _typesByAssemblyName.Values
                    .SelectMany(item => item)
                    .ToArray();
            }
            else
            {
                _typesByAssemblyName.TryGetValue(_assemblyName, out types);
            }

            _typeNames = types?.Select(GetTypeDisplayName).ToArray();

            // Update TypeName selection
            if (_typeNames == null)
                _typeName = null;
            else if (!_typeNames.Contains(_typeName))
                _typeName = _typeNames.FirstOrDefault();
        }

        private string GetTypeDisplayName(Type type)
        {
            return $"({type.Namespace}) {type.Name}";
        }
        
        private IEnumerable<Assembly> GetAllScriptAssemblies()
        {
            return Directory.EnumerateFiles($"Library/ScriptAssemblies/", "*.dll", SearchOption.AllDirectories)
                .Select(Assembly.LoadFrom);
        }

        private void LoadPreferences()
        {
            // Load saved assembly name
            _assemblyName = EditorPrefs.GetString(PrefsKeyAssemblyName, AssemblyAny);

            // Load saved type name
            _typeName = EditorPrefs.GetString(PrefsKeyTypeName, null);

            if (!_assemblyNames.Contains(_assemblyName))
                _assemblyName = AssemblyAny;

            UpdateTypeName();
        }

        private void SavePreferences()
        {
            EditorPrefs.SetString(PrefsKeyAssemblyName, _assemblyName);
            EditorPrefs.SetString(PrefsKeyTypeName, _typeName);
        }

        [Title("Create ScriptableObject Asset", TitleAlignment = TitleAlignments.Centered, Bold = true)]
        [Button(ButtonSizes.Medium), GUIColor(1f, 1f, 0f)]
        private void Create()
        {
            CreateScriptableObjectInstance();
        }

        [Button(ButtonSizes.Gigantic), GUIColor(0.1f, 1, 0.1f)]
        private void CreateAndClose()
        {
            CreateScriptableObjectInstance();
            Close();
        }

        private void CreateScriptableObjectInstance()
        {
            Type[] types = null;

            if (_assemblyName == AssemblyAny)
            {
                types = _typesByAssemblyName.Values
                    .SelectMany(item => item)
                    .ToArray();
            }
            else
            {
                _typesByAssemblyName.TryGetValue(_assemblyName, out types);
            }

            Type targetType = types?.FirstOrDefault(x => GetTypeDisplayName(x) == _typeName);

            if (targetType == null)
            {
                Debug.LogError($"Cannot find type: {_typeName} in assembly: {_assemblyName}");
                return;
            }

            var asset = ScriptableObject.CreateInstance(targetType);

            ProjectWindowUtil.CreateAsset(asset, $"{targetType.FullName}.asset");
        }
        
        [MenuItem("Assets/Create/Scriptable Object", false, 0)]
        public static void ShowWindow()
        {
            var window = GetWindow<ScriptableObjectEditorCreateWindow>(true, "Create new ScriptableObject", true);
            window.ShowPopup();
        }
    }
}