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
    }
}