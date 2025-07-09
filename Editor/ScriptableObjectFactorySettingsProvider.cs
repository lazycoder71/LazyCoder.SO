using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace LazyCoder.SO.Editor
{
    public class ScriptableObjectFactorySettingsProvider : SettingsProvider
    {
        private SerializedObject _serializedObject;
        private SerializedProperty _assemblies;

        public ScriptableObjectFactorySettingsProvider(string path, SettingsScope scopes, IEnumerable<string> keywords = null) : base(path, scopes, keywords)
        {
        }

        [SettingsProvider]
        public static SettingsProvider CreateProvider() => new ScriptableObjectFactorySettingsProvider("Project/LazyCoder/ScriptableObject", SettingsScope.Project);

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            var settings = ScriptableObjectFactorySettings.instance;

            settings.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            settings.Save();

            _serializedObject = new SerializedObject(settings);
            _assemblies = _serializedObject.FindProperty(nameof(ScriptableObjectFactorySettings.Assemblies));
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.PropertyField(_assemblies);

            if (GUILayout.Button("Restore Default"))
            {
                ScriptableObjectFactorySettings.instance.RestoreDefaults();
                _serializedObject.Update();
            }

            if (EditorGUI.EndChangeCheck())
            {
                _serializedObject.ApplyModifiedProperties();
                ScriptableObjectFactorySettings.instance.Save();
            }
        }
    }
}
