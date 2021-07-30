using System.Collections.Generic;
using DragonU3DSDK.Asset;
using UnityEngine;

namespace HomeResVerify
{
    public class Room
    {
        public int RoomId;
        public Home.Core.HomeViewConfig HomeViewConfig;
        public GameObject root;
        public List<RoomNode> RoomNodes = new List<RoomNode>();
        
        public Room(int roomId)
        {
            RoomId = roomId;
            HomeViewConfig = RoomManager.Instance.GetHomeViewConfig(RoomId);
            
            root = new GameObject($"Room{RoomId}");

            string backPath = $"Prefabs/Room/Room{RoomId}/room2d_Room{RoomId}";
            GameObject backPrefab = ResourcesManager.Instance.LoadResource<GameObject>(backPath);
            if (null == backPrefab) Debug.LogError($"找不到 : {backPath}");
            else GameObject.Instantiate(backPrefab, root.transform);

            foreach (var node in HomeViewConfig.roomNodes) RoomNodes.Add(new RoomNode(this, node));
        }

        public void Destory()
        {
            foreach (var node in RoomNodes) node.Destory();
            RoomNodes.Clear();
            
            GameObject.DestroyImmediate(root);
            root = null;
        }
        
        public RoomNode TapTest(Vector3 screenPos)
        {
            /*Logic：
             * 1，遍历所有节点，找到节点当前显示的RoomItem。
             * 2，将点击按照room相机投射到world中，测试是否点击到该Item的碰撞器。
             * 3，若有多个node被点击到，则对比Node的渲染层级（该层级在公共库配置），
             * 将返回值改为最高层级的，也就是显示叠加的最上层node。
             */

            RoomNode selectedNode = null;
            foreach (var node in RoomNodes)
            {
                if (node.TouchTest(screenPos))
                {
                    if (selectedNode == null)
                    {
                        selectedNode = node;
                    }
                    else if (node.RoomNodeCfg.baseNodeCfg.renderQueue > selectedNode.RoomNodeCfg.baseNodeCfg.renderQueue)
                    {
                        selectedNode = node;
                    }
                }
            }

            return selectedNode;
        }
    }
}