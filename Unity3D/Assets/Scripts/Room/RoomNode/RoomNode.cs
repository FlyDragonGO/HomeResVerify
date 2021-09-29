using System.Collections.Generic;
using Home.Core;
using UnityEngine;

namespace HomeResVerify
{
    public class RoomNode
    {
        public Room Room;
        public RoomNodeCfg RoomNodeCfg;

        public GameObject root;
        
        public List<RoomItem> RoomItems = new List<RoomItem>();
        
        public RoomNode(Room room, RoomNodeCfg cfg)
        {
            Room = room;
            RoomNodeCfg = cfg;
            
            root = new GameObject(cfg.id.ToString());
            root.transform.parent = room.root.transform;

            foreach (var item in cfg.ItemCfgs)
            {
                RoomItem roomItem = new RoomItem(this, item);
                RoomItems.Add(roomItem);
                roomItem.Instance.SetActive(false);
            }
        }

        public void Destory()
        {
            
        }
        
        public bool TouchTest(Vector2 screenPos)
        {
            RoomItem roomItem = RoomItems.Find(x => x.Instance.activeSelf);
            return roomItem != null ? roomItem.TouchTest(screenPos) : false;
        }
        
        public Vector3 getDefaultScreenPos()
        {
            foreach (var item in RoomNodeCfg.ItemCfgs)
            {
                foreach (var offset in item.offsetItemCfg.pos)
                {
                    return ConvertOffsetToScreenPos(Room._camera, offset.value);
                }
            }

            return Vector3.zero;
        }
        
        public  Vector3 ConvertOffsetToScreenPos(Camera camera, Vector2 offset)
        {
            int designSizeIndex = PlayerPrefs.GetInt(UIHall.DesignSizesIndexKey);
            var DesignWidth  = RoomConst.DesignSizes[designSizeIndex].x;
            var DesignHeight = RoomConst.DesignSizes[designSizeIndex].y;
            var CenterPosition = Vector2.zero;
            var dynamicOffset = offset - CenterPosition;

            var scaleX = Screen.width / (float)DesignWidth;
            var scaleY = Screen.height /(float)DesignHeight;

            var maxScale = Mathf.Max(scaleX, scaleY);

            var x = Screen.width / 2f + dynamicOffset.x * maxScale;
            var y = Screen.height / 2f + dynamicOffset.y * maxScale;
            var z = 0f;
            if (!camera.orthographic)
            {
                z = 0f - camera.transform.position.z;
            }

            return new Vector3(x, y, z);
        }
    }
}