using System.Diagnostics;
using UnityEditor;
using UnityEngine;

namespace HomeResVerify.Editor
{
    public class Tool
    {
        [MenuItem("Main/打开Persistent文件夹")]
        public static void OpenPersistent()
        {
            Process.Start(Application.persistentDataPath);
        }
    }
}