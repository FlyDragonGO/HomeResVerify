using UnityEngine;

namespace HomeResVerify
{
    public class RoomManager : Manager<RoomManager>
    {
        public Room room;
        
        public void Enter(int roomId)
        {
            UIManager.Instance.CloseUI<UIHall>();
            UIManager.Instance.OpenUI<UIRoom>();

            ResetCamera();
            
            room = new Room(roomId);
            ViewNormal();
            
            if(null != UIManager.Instance.GetUI<UIRoom>()) 
                UIManager.Instance.GetUI<UIRoom>().InitFurniturePoint();
        }

        public void Exit()
        {
            if (null == room) return;
            
            room.Destory();
            room = null;

            UIManager.Instance.CloseUI<UIRoom>();
            UIManager.Instance.OpenUI<UIHall>();
        }

        public void ViewHide()
        {
            foreach (var node in room.RoomNodes)
            {
                foreach (var item in node.RoomItems)
                {
                    item.Instance.SetActive(false);
                }
            }
        }
        
        public void ViewOld()
        {
            foreach (var node in room.RoomNodes)
            {
                foreach (var item in node.RoomItems)
                {
                    item.Instance.SetActive(item.RoomItemCfg.oldFurItemCfg.isOld);
                }
            }
        }

        public void ViewNormal()
        {
            foreach (var node in room.RoomNodes)
            {
                foreach (var item in node.RoomItems)
                {
                    if (!item.RoomItemCfg.oldFurItemCfg.isOld)
                    {
                        item.Instance.SetActive(true);
                        break;
                    }
                }
            }
        }
        
        public Home.Core.HomeViewConfig GetHomeViewConfig(int roomId)
        {
            return new Home.Core.HomeViewConfig(
                ((int) Home.Core.HomeViewConfigType.Base |
                 (int) Home.Core.HomeViewConfigType.Offset |
                 (int) Home.Core.HomeViewConfigType.OldFur |
                 (int) Home.Core.HomeViewConfigType.Anim)
                , string.Format("Configs/Room/Room{0}", roomId), RoomConst.ProjectName[PlayerPrefs.GetInt(UIHall.ProjectNameIndexKey)]);
        }
        private void ResetCamera()
        {
            int designSizeIndex = PlayerPrefs.GetInt(UIHall.DesignSizesIndexKey);
            float size  = CalculateOrthographicSize(RoomConst.DesignSizes[designSizeIndex].x, RoomConst.DesignSizes[designSizeIndex].y);
            Camera.main.orthographicSize = size;
            Camera.main.transform.position = new Vector3(0.0f, 0.0f, -50.0f);
        }
        private float CalculateOrthographicSize(float width, float height)
        {
            var currentX = width / 100f / 2f;
            var currentY = height / 100f / 2f;

            var size = 0f;

            var aspectScreen = Screen.width / (float) Screen.height;
            var aspectDesign = width / height;

            if (aspectScreen > aspectDesign)
            {
                size = currentX / Camera.main.aspect;
            }
            else
            {
                size = currentY;
            }
            //解决某些机型显示边界有蓝色边 
            return size -0.01f;
        }
    }
}