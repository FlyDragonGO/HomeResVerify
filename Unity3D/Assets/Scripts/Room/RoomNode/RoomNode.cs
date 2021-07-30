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
    }
}