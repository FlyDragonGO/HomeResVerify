using System.Collections.Generic;
using UnityEngine;

namespace HomeResVerify
{
    public abstract class UIBase : MonoBehaviour
    {
        public abstract void Init(params object[] objs);
    }
    
    public class UIManager : Manager<UIManager>
    {
        private GameObject UIRoot;
        private Dictionary<string, GameObject> ui = new Dictionary<string, GameObject>();
        
        public void Init()
        {
            GameObject UIRootPrefab =  Resources.Load<GameObject>("Prefabs/UI/UIRoot");
            UIRoot = GameObject.Instantiate(UIRootPrefab);
            GameObject.DontDestroyOnLoad(UIRoot);
        }

        public void OpenUI<T>(params object[] objs)
            where T : UIBase
        {
            string name = typeof(T).ToString();
            name = name.Substring(name.LastIndexOf(".") + 1);
            string path = $"Prefabs/UI/{name}";
            GameObject prefab = Resources.Load<GameObject>(path);
            GameObject instance = GameObject.Instantiate(prefab, UIRoot.transform.Find("Canvas/Control"));
            T t= instance.AddComponent<T>();
            ui.Add(name, instance);
            t.Init(objs);
        }
        
        public void CloseUI<T>()
            where T : UIBase
        {
            string name = typeof(T).ToString();
            name = name.Substring(name.LastIndexOf(".") + 1);
            GameObject instance;
            if (ui.TryGetValue(name, out instance))
            {
                ui.Remove(name);
                GameObject.DestroyImmediate(instance);
            }
        }
    }
}

