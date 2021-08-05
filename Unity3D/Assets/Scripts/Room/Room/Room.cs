using System.Collections.Generic;
using System.IO;
using DragonU3DSDK.Asset;
using DragonU3DSDK.Network.API.Protocol;
using UnityEngine;
using File = System.IO.File;

namespace HomeResVerify
{
    public class Room
    {
        public int RoomId;
        public Home.Core.HomeViewConfig HomeViewConfig;
        public GameObject root;
        public List<RoomNode> RoomNodes = new List<RoomNode>();
        
        private List<GameObject> cleanInstances = new List<GameObject>();
        
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
            
            CloseClean();
            
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

        public void ViewWClean()
        {
            CloseClean();

            string cleanNameFormat = $"room2d_Room{RoomId}_clean".ToLower();
            AssetBundle ab = ResourcesManager.Instance.AssetBundleCache.GetAssetBundle($"Prefabs/Room/Room{RoomId}.ab".ToLower());
            List<string> prefabClean = new List<string>();
            foreach (var p in ab.GetAllAssetNames())
            {
                string name = Path.GetFileNameWithoutExtension(p);
                if(name.IndexOf(cleanNameFormat) == 0) prefabClean.Add(name);
            }

            foreach (var p in prefabClean)
            {
                string prefabPath = $"Prefabs/Room/Room{RoomId}/{p}";
                GameObject prefab = ResourcesManager.Instance.LoadResource<GameObject>(prefabPath);
                if (null == prefab) Debug.LogError($"找不到 : {prefabPath}");
                else cleanInstances.Add(GameObject.Instantiate(prefab, root.transform));
            }
        }
        
        public void CloseClean()
        {
            if (cleanInstances.Count == 0) return;
            foreach (var p in cleanInstances) GameObject.DestroyImmediate(p);
            cleanInstances.Clear();
        }
    }
}