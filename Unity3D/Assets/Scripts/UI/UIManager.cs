using System;
using System.Collections.Generic;
using DragonU3DSDK;
using DragonU3DSDK.Asset;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.U2D;
using UnityEngine.UI;

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
        
        private void Start()
        {
            SpriteAtlasManager.atlasRequested += OnLoadAtlas;
        }


        private void OnDestroy()
        {
            SpriteAtlasManager.atlasRequested -= OnLoadAtlas;
        }
        
        private void OnLoadAtlas(string atlasName, Action<SpriteAtlas> act)
        {
            string path = $"SpriteAtlas/{atlasName}/hd/{atlasName}";
            var sa =ResourcesManager.Instance.LoadResource<SpriteAtlas>(path);
            if (sa == null) Debug.LogError($"图集加载失败：{path}");
            act(sa);
        }
        
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

        private void Update()
        {
            if (Screen.width > Screen.height)
            {
                CanvasScaler canvasScaler = UIRoot.transform.Find("Canvas").GetComponent<CanvasScaler>();
                canvasScaler.referenceResolution = new Vector2(1365, 768);
                canvasScaler.matchWidthOrHeight = 1;
            }
            else
            {
                CanvasScaler canvasScaler = UIRoot.transform.Find("Canvas").GetComponent<CanvasScaler>();
                canvasScaler.referenceResolution = new Vector2(768, 1365);
                canvasScaler.matchWidthOrHeight = 0;
            }
        }
    }
}

