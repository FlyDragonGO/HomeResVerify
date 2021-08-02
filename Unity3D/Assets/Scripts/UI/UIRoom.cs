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
            
            BindDrag("Root/Controller/NodeProperity", d =>
            {
                var data = d as PointerEventData;
                var rt = (RectTransform) nodeProperity;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, data.position, data.pressEventCamera,
                    out var globalMousePos))
                    rt.position = globalMousePos;
            });
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

        private void OpenNodeProperty(RoomNode roomNode)
        {
            curRoomNode = roomNode;
            nodeProperity.gameObject.SetActive(true);

            nodeProperity.Find("ItemName/Text").GetComponent<Text>().text = "";
            Dropdown animationDropdown = nodeProperity.Find("Animation").GetComponent<Dropdown>();
            animationDropdown.options.Clear();
            RoomItem item = curRoomNode.RoomItems.Find(x => x.Instance.activeSelf);
            if (item == null || item.animator == null)
            {
                animationDropdown.options.Add(new Dropdown.OptionData("没有动画"));
            }
            else
            {
                nodeProperity.Find("ItemName/Text").GetComponent<Text>().text = item.RoomItemCfg.id;
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
            DebugUtil.Assert(obj != null,"未找到　" + target );
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
    }
}