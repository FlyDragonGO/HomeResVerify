using System.Collections.Generic;
using DragonU3DSDK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HomeResVerify
{
    public class UIRoom : UIBase, IPointerClickHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
    {
        private Transform nodeProperity;
        private RoomNode curRoomNode;

        #region 泡泡相关

        private Transform furniturePointRoot;
        private Transform furniturePointBase;

        private Transform safeArea;
        private List<UIFurniturePoint> furniturePointBubbles = new List<UIFurniturePoint>();
        private bool bShowingfurniturePoint = true;

        private Button btnBubbleControl;
        private Text txtBubbleControl;
        
        public class UIFurniturePoint
        {
            private RoomNode _roomNode;
            private Transform _root;

            public void Init(Transform root, RoomNode node)
            {
                _root = root;
                _roomNode = node;
                _root.GetComponent<Button>().onClick.AddListener(OnCLickBubble);

            }

            public void Hide()
            {
                _root.gameObject.SetActive(false);
            }
            public void Show()
            {
                _root.gameObject.SetActive(true);
            }

            public void OnCLickBubble()
            {
                if(null != UIManager.Instance.GetUI<UIRoom>()) 
                    UIManager.Instance.GetUI<UIRoom>().OpenNodeProperty(_roomNode);
            }
        }

        #endregion

        public override void Init(params object[] objs)
        {
            gameObject.AddComponent<EmptyRaycast>();
            transform.Find("Root/Hide").GetComponent<Button>().onClick.AddListener(HideOnClick);
            transform.Find("Root/Controller/Close").GetComponent<Button>().onClick.AddListener(() => { RoomManager.Instance.Exit(); });
            transform.Find("Root/Controller/ViewNormal").GetComponent<Button>().onClick.AddListener(ViewNormal);
            transform.Find("Root/Controller/ViewOld").GetComponent<Button>().onClick.AddListener(ViewOld);
            nodeProperity = transform.Find("Root/Controller/NodeProperity");
            nodeProperity.Find("Hide").GetComponent<Button>().onClick.AddListener(CloseNodeProperty);
            nodeProperity.Find("Change/Left").GetComponent<Button>().onClick.AddListener(() => { ChangeNodeItem(-1); });
            nodeProperity.Find("Change/Right").GetComponent<Button>().onClick.AddListener(() => { ChangeNodeItem(1); });
            nodeProperity.Find("Play").GetComponent<Button>().onClick.AddListener(NodeItemPlayOnClick);
            
            btnBubbleControl = transform.Find("Root/Controller/ViewPointBubble").GetComponent<Button>();
            txtBubbleControl = btnBubbleControl.transform.Find("Text").GetComponent<Text>();
            
            BindDrag("Root/Controller/NodeProperity", d =>
            {
                var data = d as PointerEventData;
                var rt = (RectTransform) nodeProperity;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, data.position, data.pressEventCamera,
                    out var globalMousePos))
                    rt.position = globalMousePos;
            });

            furniturePointRoot = transform.Find("Root/ScreenOrigin");
            furniturePointBase = transform.Find("Root/ScreenOrigin/FurniturePointButton");
            furniturePointBase.gameObject.SetActive(false);
            
            safeArea = transform.Find("Root/SafeArea");
            
            
        }

        private void HideOnClick()
        {
            GameObject controller = transform.Find("Root/Controller").gameObject;
            controller.SetActive(!controller.activeSelf);
        }

        public void ViewNormal()
        {
            CloseNodeProperty();
            RoomManager.Instance.ViewHide();
            RoomManager.Instance.ViewNormal();
        }

        public void ViewOld()
        {
            CloseNodeProperty();
            RoomManager.Instance.ViewHide();
            RoomManager.Instance.ViewOld();
        }

        public void OpenNodeProperty(RoomNode roomNode)
        {
            curRoomNode = roomNode;
            nodeProperity.gameObject.SetActive(true);

            nodeProperity.Find("ItemName/Text").GetComponent<Text>().text = "";
            Dropdown animationDropdown = nodeProperity.Find("Animation").GetComponent<Dropdown>();
            animationDropdown.options.Clear();
            RoomItem item = curRoomNode.RoomItems.Find(x => x.Instance.activeSelf);
            nodeProperity.Find("ItemName/Text").GetComponent<Text>().text = item.RoomItemCfg.id;
            if (item == null || item.animator == null)
            {
                animationDropdown.options.Add(new Dropdown.OptionData("没有动画"));
            }
            else
            {
                foreach (var clip in item.animator.runtimeAnimatorController.animationClips)
                {
                    animationDropdown.options.Add(new Dropdown.OptionData(clip.name));
                }
            }
        }

        private void CloseNodeProperty()
        {
            if (!nodeProperity.gameObject.activeSelf) return;
            
            RoomItem item = curRoomNode.RoomItems.Find(x => x.Instance.activeSelf);
            if (item != null || item.animator != null) item.PlayAnimation("Normal");
            curRoomNode = null;
            nodeProperity.gameObject.SetActive(false);
        }

        //-1-left 1-right
        private void ChangeNodeItem(int dir)
        {
            if(null == curRoomNode) return;
            int index = curRoomNode.RoomItems.FindIndex(x => x.Instance.activeSelf);
            if (index == -1) return;
            index += dir;
            if (index < 0) index = curRoomNode.RoomItems.Count - 1;
            else if (index > curRoomNode.RoomItems.Count - 1) index = 0;
            foreach (var item in curRoomNode.RoomItems) item.Instance.SetActive(false);
            curRoomNode.RoomItems[index].Instance.SetActive(true);
            nodeProperity.Find("ItemName/Text").GetComponent<Text>().text = curRoomNode.RoomItems[index].RoomItemCfg.id;
        }

        private void NodeItemPlayOnClick()
        {
            if(null == curRoomNode) return;
            RoomItem item = curRoomNode.RoomItems.Find(x => x.Instance.activeSelf);
            if (item == null || item.animator == null) return;
            Dropdown animationDropdown = nodeProperity.Find("Animation").GetComponent<Dropdown>();
            Dropdown.OptionData optionData = animationDropdown.options[animationDropdown.value];
            item.PlayAnimation(optionData.text);
        }
        
        private void BindDrag(string target, UnityAction<BaseEventData> action)
        {
            GameObject obj = transform.Find(target)?.gameObject;
            if(obj == null) Debug.LogError("未找到　" + target);
            var trigger = obj.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = obj.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (nodeProperity.gameObject.activeSelf) return;
            RoomNode roomNode = RoomManager.Instance.room.TapTest(eventData.position);
            if (null != roomNode) OpenNodeProperty(roomNode);
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }

        public void InitFurniturePoint()
        {
            InitFurniturePointBubble();
            ShowFurniturePointBubble();
        }


        private void ShowFurniturePointBubble()
        {
            furniturePointBubbles.ForEach((item) =>
            {
                item.Show();
            });
            bShowingfurniturePoint = true;
            btnBubbleControl.onClick.RemoveAllListeners();
            btnBubbleControl.onClick.AddListener(HideFurniturePointBubble);
            txtBubbleControl.text = "隐藏泡泡";
        }

        private void HideFurniturePointBubble()
        {
            furniturePointBubbles.ForEach((item) =>
            {
                item.Hide();
            });
            bShowingfurniturePoint = true;
            btnBubbleControl.onClick.RemoveAllListeners();
            btnBubbleControl.onClick.AddListener(ShowFurniturePointBubble);
            txtBubbleControl.text = "显示泡泡";
        }
        private void InitFurniturePointBubble()
        {
            foreach (var roomNode in RoomManager.Instance.room.RoomNodes)
            {
                UIFurniturePoint uiFurniturePoint = new UIFurniturePoint();
                GameObject bubble = GameObject.Instantiate(furniturePointBase.gameObject, furniturePointRoot);
                var screenPosition = roomNode.getDefaultScreenPos();

                Vector2 position = screenPosition / UIRoot.Instance.GetScreenCanvasScale();
                bubble.transform.localPosition = new Vector3(position.x,position.y,bubble.transform.localPosition.z);
                UIRoot.Instance.FitUIPos(bubble, safeArea.gameObject);
                bubble.gameObject.SetActive(true);
                uiFurniturePoint.Init(bubble.transform,roomNode);
                furniturePointBubbles.Add(uiFurniturePoint);
            }
        }
    }
}