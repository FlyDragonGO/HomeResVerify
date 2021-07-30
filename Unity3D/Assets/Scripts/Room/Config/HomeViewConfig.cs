using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Home.Core
{
    #region 裁剪偏移
    [Serializable]
    public class CutOffsetItemCfg
    {
        public string key;
        public Vector2 pos;
    }
    [Serializable]
    public class CutOffsetCfg
    {
        public List<CutOffsetItemCfg> items = new List<CutOffsetItemCfg>();
    }
    #endregion

    #region 房间基础
    [Serializable]
    public class BaseItemCfg
    {
        public string id;
        public List<int> renderQueue = new List<int>();
    }

    [Serializable]
    public class BaseNodeCfg
    {
        public long id;
        public List<BaseItemCfg> items = new List<BaseItemCfg>();
        public int renderQueue;
        public bool renderQueueDisable = false; // 不使用挂点上的渲染次序，使用每个家具上的
    }

    [Serializable]
    public class BaseCfg
    {
        public List<BaseNodeCfg> nodes = new List<BaseNodeCfg>();
    }
    #endregion
    
    #region 偏移
    [Serializable]
    public class OffsetInfoCfg
    {
        public string key;    //用于作为父节点依赖的标志，实际运用中只有1001使用了，故没有在编辑器中实现
        public Vector2 value = new Vector2();
    }
    [Serializable]
    public class OffsetItemCfg
    {
        public string id;
        public List<OffsetInfoCfg> pos = new List<OffsetInfoCfg>();
    }

    [Serializable]
    public class OffsetNodeCfg
    {
        public long id;
        public List<OffsetItemCfg> items = new List<OffsetItemCfg>();
    }

    [Serializable]
    public class OffsetCfg
    {
        public List<OffsetNodeCfg> nodes = new List<OffsetNodeCfg>();
    }
    #endregion
    
    #region 旧家具
    [Serializable]
    public class OldFurItemCfg
    {
        public string id;
        public bool isOld;
    }

    [Serializable]
    public class OldFurNodeCfg
    {
        public long id;
        public List<OldFurItemCfg> items = new List<OldFurItemCfg>();
    }

    [Serializable]
    public class OldFurCfg
    {
        public List<OldFurNodeCfg> nodes = new List<OldFurNodeCfg>();
    }
    #endregion

    #region 动效 节点名称必须唯一，层级不超过2层
    [Serializable]
    public class AnimItemEffect
    {
        public string prefabName;
        public string objectName;

        public List<TransformVariant> transformVariants = new List<TransformVariant>();
        public List<ParticleSystemVariant> particleSystemVariants = new List<ParticleSystemVariant>();

        public void Variants(Transform source, Transform main)
        {
            if (null == source || null == main) { return; }
            //TransformVariant
            {
                transformVariants.Clear();
                //root
                transformVariants.Add(new TransformVariant()
                {
                    path = objectName,
                    active = main.gameObject.activeSelf,
                    pos = main.localPosition,
                    rot = main.localRotation.eulerAngles,
                    scale = main.localScale
                });
            
                //child
                for (int traI1 = 0; traI1 < main.childCount; traI1++)
                {
                    Transform traI1Child = main.GetChild(traI1);
                    transformVariants.Add(new TransformVariant()
                    {
                        path = $"{objectName}/{traI1Child.name}",
                        active = traI1Child.gameObject.activeSelf,
                        pos = traI1Child.localPosition,
                        rot = traI1Child.localRotation.eulerAngles,
                        scale = traI1Child.localScale
                    });

                    for (int traI2 = 0; traI2 < traI1Child.childCount; traI2++)
                    {
                        Transform traI2Child = traI1Child.GetChild(traI2);
                        transformVariants.Add(new TransformVariant()
                        {
                            path = $"{objectName}/{traI1Child.name}/{traI2Child.name}",
                            active = traI2Child.gameObject.activeSelf,
                            pos = traI2Child.localPosition,
                            rot = traI2Child.localRotation.eulerAngles,
                            scale = traI2Child.localScale
                        });
                    }
                } 
            }

            //ParticleSystemVariant
            {
                particleSystemVariants.Clear();
                foreach (var p in transformVariants)
                {
                    Transform sourceNode = GetNode(source, p.path);
                    Transform mainNode = GetNode(main, p.path);
                    if (sourceNode == null || mainNode == null) { continue; }
                    ParticleSystem sourcePS = sourceNode.GetComponent<ParticleSystem>();
                    ParticleSystem mainPS = mainNode.GetComponent<ParticleSystem>();
                    if (sourcePS == null || mainPS == null) { continue; }
                    particleSystemVariants.Add(new ParticleSystemVariant()
                    {
                        path = p.path,
                        json = ParticleSystemVariantUtils.Serialize(sourcePS, mainPS)
                    });
                }
            }
        }

        public void Cover(Transform transform)
        {
            //TransformVariant
            foreach (var p in transformVariants)
            {
                Transform node = GetNode(transform, p.path);
                if (node == null) { continue; }
                node.localPosition = p.pos;
                node.localRotation = Quaternion.Euler(p.rot);
                node.localScale = p.scale;
                node.gameObject.SetActive(p.active);
            }
            
            //ParticleSystemVariant
            bool oldActive = transform.gameObject.activeSelf;
            transform.gameObject.SetActive(false);
            foreach (var p in particleSystemVariants)
            {
                Transform node = GetNode(transform, p.path);
                if (node == null) { continue; }
                ParticleSystem ps = node.GetComponent<ParticleSystem>();
                if(ps == null) { continue; }
                ParticleSystemVariantUtils.Deserialize(ps, JObject.Parse(p.json));
            }
            transform.gameObject.SetActive(oldActive);
        }

        public Transform GetNode(Transform transform, string path)
        {
            string[] names = path.Split('/');
            Transform node = transform;
            for (int i = 1; i < names.Length; i++)
            {
                if (node == null)
                {
                    break;
                }
                node = node.Find(names[i]);
            }

            return node;
        }
    }
    
    [Serializable]
    public class AnimItemCfg
    {
        public string id;
        public string controller;
        public List<AnimItemEffect> effects = new List<AnimItemEffect>();
    }

    [Serializable]
    public class AnimNodeCfg
    {
        public long id;
        public List<AnimItemCfg> items = new List<AnimItemCfg>();
    }

    [Serializable]
    public class AnimCfg
    {
        public List<AnimNodeCfg> nodes = new List<AnimNodeCfg>();
    }
    #endregion
    
    #region 合并
    //以baseCfg为主干
    [Serializable]
    public class RoomNodeCfg
    {
        public long id;
        
        public BaseNodeCfg baseNodeCfg = new BaseNodeCfg();
        public OffsetNodeCfg offsetNodeCfg = new OffsetNodeCfg();
        public OldFurNodeCfg oldFurNodeCfg = new OldFurNodeCfg();
        public AnimNodeCfg animNodeCfg = new AnimNodeCfg();
        
        public List<RoomItemCfg> ItemCfgs = new List<RoomItemCfg>();
    }

    [Serializable]
    public class RoomItemCfg
    {
        public string id;

        public BaseItemCfg baseItemCfg = new BaseItemCfg();
        public OffsetItemCfg offsetItemCfg = new OffsetItemCfg();
        public OldFurItemCfg oldFurItemCfg = new OldFurItemCfg();
        public AnimItemCfg animItemCfg = new AnimItemCfg();
    }
    #endregion

    public enum HomeViewConfigType
    {
        Base         = 1,
        Offset       = 2,
        OldFur       = 4,
        Anim         = 8,
    }
    
    public class HomeViewConfig
    {
        public int viewType;
        
        public List<RoomNodeCfg> roomNodes = new List<RoomNodeCfg>();
        
        private BaseCfg baseCfg = new BaseCfg();
        private OffsetCfg offsetCfg = new OffsetCfg();
        private OldFurCfg oldFurCfg = new OldFurCfg();
        private AnimCfg animCfg = new AnimCfg();

        public HomeViewConfig(int _viewType, string path, string projectName)
        {
            viewType = _viewType;
            
            if ((viewType & (int)HomeViewConfigType.Base) != 0)
            {
                LoadCfg<BaseCfg>(path, projectName, HomeViewConfigType.Base.ToString(), ref baseCfg);
                foreach (var node in baseCfg.nodes)
                {
                    RoomNodeCfg nodeCfg = new RoomNodeCfg()
                    {
                        id = node.id,
                        baseNodeCfg = node
                    };
                    roomNodes.Add(nodeCfg);
                    foreach (var item in node.items)
                    {
                        nodeCfg.ItemCfgs.Add(new RoomItemCfg()
                        {
                            id = item.id,
                            baseItemCfg = item,
                        });
                    }
                }
            }
            
            if ((viewType & (int)HomeViewConfigType.Offset) != 0)
            {
                LoadCfg<OffsetCfg>(path, projectName, HomeViewConfigType.Offset.ToString(), ref offsetCfg);
                foreach (var node in offsetCfg.nodes)
                {
                    RoomNodeCfg roomNodeCfg = roomNodes.Find(x => x.id == node.id);
                    if (null == roomNodeCfg)
                    {
                        continue;
                    }
                    roomNodeCfg.offsetNodeCfg = node;
                    foreach (var item in roomNodeCfg.ItemCfgs)
                    {
                        OffsetItemCfg temp = roomNodeCfg.offsetNodeCfg.items.Find(x => x.id.Equals(item.id));
                        if (null != temp) item.offsetItemCfg = temp;
                    }
                }
            }
            
            if ((viewType & (int)HomeViewConfigType.OldFur) != 0)
            {
                LoadCfg<OldFurCfg>(path, projectName, HomeViewConfigType.OldFur.ToString(), ref oldFurCfg);
                foreach (var node in oldFurCfg.nodes)
                {
                    RoomNodeCfg roomNodeCfg = roomNodes.Find(x => x.id == node.id);
                    if (null == roomNodeCfg)
                    {
                        continue;
                    }
                    roomNodeCfg.oldFurNodeCfg = node;
                    foreach (var item in roomNodeCfg.ItemCfgs)
                    {
                        OldFurItemCfg temp = roomNodeCfg.oldFurNodeCfg.items.Find(x => x.id.Equals(item.id));
                        if (null != temp) item.oldFurItemCfg = temp;
                    }
                }
            }
            
            if ((viewType & (int)HomeViewConfigType.Anim) != 0)
            {
                LoadCfg<AnimCfg>(path, projectName, HomeViewConfigType.Anim.ToString(), ref animCfg);
                foreach (var node in animCfg.nodes)
                {
                    RoomNodeCfg roomNodeCfg = roomNodes.Find(x => x.id == node.id);
                    if (null == roomNodeCfg)
                    {
                        continue;
                    }
                    roomNodeCfg.animNodeCfg = node;
                    foreach (var item in roomNodeCfg.ItemCfgs)
                    {
                        AnimItemCfg temp = roomNodeCfg.animNodeCfg.items.Find(x => x.id.Equals(item.id));
                        if (null != temp) item.animItemCfg = temp;
                    }
                }
            }
        }

        private void LoadCfg<T>(string path, string projectName, string type, ref T cfg)
        {
            string destPath = Path.Combine(path, $"{type}_{projectName}");
            var textAsset = DragonU3DSDK.Asset.ResourcesManager.Instance.LoadResource<TextAsset>(destPath);
            if (textAsset == null)
            {
                destPath = Path.Combine(path, type);
                textAsset = DragonU3DSDK.Asset.ResourcesManager.Instance.LoadResource<TextAsset>(destPath);
            }
            if (textAsset != null)
            {
                cfg = JsonConvert.DeserializeObject<T>(textAsset.text);
            }
        }
    }
}