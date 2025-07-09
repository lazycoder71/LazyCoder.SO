using UnityEditor;

namespace LazyCoder.SO.Editor
{
    [FilePath(Path, FilePathAttribute.Location.ProjectFolder)]
    internal class ScriptableObjectFactorySettings : ScriptableSingleton<ScriptableObjectFactorySettings>
    {
        public const string Path = "ProjectSettings/LazyCoder.SO.asset";

        public string[] Assemblies = new string[] {
            "Assembly-CSharp",
            "Game",
            "LazyCoder"
        };

        public void RestoreDefaults()
        {
            Assemblies = new string[] {
                "Assembly-CSharp",
                "Game",
                "LazyCoder"
            };
        }

        public void Save() => Save(true);
    }
}