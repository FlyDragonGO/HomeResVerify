
using System;
using DragonU3DSDK;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HomeResVerify
{
    public class UIRoom : UIBase,IPointerClickHandler,IPointerUpHandler,IDragHandler,IEndDragHandler
    {
        public override void Init(params object[] objs)
        {
            gameObject.AddComponent<CKEmptyRaycast>();
            
            transform.Find("Root/Set/Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                RoomManager.Instance.Exit();
            });
            transform.Find("Root/Controller/NodeProperity/Hide").GetComponent<Button>().onClick.AddListener(() =>
            {
                transform.Find("Root/Controller/NodeProperity").gameObject.SetActive(false);
            });
            
            BindDrag("Root/Controller/NodeProperity", d =>
            {
                var data = d as PointerEventData;
                var rt = (RectTransform) data.selectedObject.transform.parent;
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, data.position, data.pressEventCamera,
                    out var globalMousePos))
                    rt.position = globalMousePos;
            });
        }

        public void OpenNodeProperty(RoomNode roomNode)
        {
            transform.Find("Root/Controller/NodeProperity").gameObject.SetActive(true);
            
            
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