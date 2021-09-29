using DragonU3DSDK.Asset;
using Home.Core;
using UnityEngine;

namespace HomeResVerify
{
    public class RoomItem
    {
        public RoomNode RoomNode;
        public RoomItemCfg RoomItemCfg;

        public GameObject Instance;
        public Animator animator;
        
        public RoomItem(RoomNode roomNode, RoomItemCfg cfg)
        {
            RoomNode = roomNode;
            RoomItemCfg = cfg;

            string prefabPath = $"Prefabs/Room/Room{roomNode.Room.RoomId}/{cfg.id}";
            GameObject prefab = ResourcesManager.Instance.LoadResource<GameObject>(prefabPath);
            if (null == prefab)
            {
                Debug.LogError($"找不到 : {prefabPath}");
                return;
            }

            Instance = GameObject.Instantiate(prefab, roomNode.root.transform);
            
            Transform oneChild = Instance.transform.Find("01");
            if (null != oneChild)
            {
                foreach (var effect in cfg.animItemCfg.effects)
                {
                    GameObject effectPrefab = ResourcesManager.Instance.LoadResource<GameObject>($"Prefabs/effect/{effect.prefabName}");
                    if (null == effectPrefab) continue;
                    GameObject effectObj = GameObject.Instantiate(effectPrefab);
                    effectObj.name = effect.objectName;
                    effectObj.transform.SetParent(oneChild);
                    effectObj.transform.Reset();
                    effect.Cover(effectObj.transform);
                }
            }
            
            if (!string.IsNullOrEmpty(cfg.animItemCfg.controller))
            {
                animator = Instance.GetComponent<Animator>();
                if(animator == null) animator = Instance.AddComponent<Animator>();
                animator.runtimeAnimatorController =  ResourcesManager.Instance.LoadResource<RuntimeAnimatorController>(
                    $"Animations/{cfg.animItemCfg.controller}", false, true, cfg.animItemCfg.controller);
            }

#if UNITY_EDITOR
            var spriteRenderers = Instance.GetComponentsInChildren<SpriteRenderer>();
            var mat = Resources.Load<Material>("Materials/SpriteCustomDefault");
            foreach (var render in spriteRenderers)
            {
                if (mat)
                {
                    render.material = mat;    
                }
            }
#endif
        }

        public void Destory()
        {
            
        }
        
        public bool TouchTest(Vector2 screenPos)
        {
            PolygonCollider2D[] colliders = Instance.GetComponentsInChildren<PolygonCollider2D>();
            var worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            foreach (var collider in colliders)
            {
                if (collider.OverlapPoint(worldPos)) return true;
            }
            return false;
        }

        public void PlayAnimation(string name)
        {
            animator?.Play(name);
        }
    }
}