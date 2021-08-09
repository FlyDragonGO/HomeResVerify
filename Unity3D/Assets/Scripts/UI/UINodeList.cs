using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HomeResVerify
{
    public class UINodeList : UIBase, IPointerClickHandler, IPointerUpHandler, IDragHandler, IEndDragHandler
    {
        public override void Init(params object[] objs)
        {
            gameObject.AddComponent<EmptyRaycast>();
            transform.Find("Root/Drag/Close").GetComponent<Button>().onClick.AddListener(() => { UIManager.Instance.CloseUI<UINodeList>(); });
            BindDrag("Root/Drag", d =>
            {
                var data = d as PointerEventData;
                var rt = (RectTransform) transform.Find("Root/Drag");
                if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, data.position, data.pressEventCamera,
                    out var globalMousePos))
                    rt.position = globalMousePos;
            });

            GameObject nodeItem = transform.Find("Root/Drag/NodeView/Viewport/Content/NodeItem").gameObject;
            Transform parent = transform.Find("Root/Drag/NodeView/Viewport/Content");
            Room curRoom = RoomManager.Instance.room;
            for (int i = 0; i < curRoom.RoomNodes.Count; i++)
            {
                GameObject item = GameObject.Instantiate(nodeItem, parent);
                item.SetActive(true);
                item.transform.Find("Name").GetComponent<Text>().text = curRoom.RoomNodes[i].RoomNodeCfg.id.ToString();
                item.transform.GetComponent<Toggle>().enabled = curRoom.RoomNodes[i].root.activeSelf;
                int index = i;
                item.transform.GetComponent<Toggle>().onValueChanged.AddListener((b) =>
                {
                    curRoom.RoomNodes[index].root.SetActive(b);
                });
            }
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
            
        }

        public void OnDrag(PointerEventData eventData)
        {
        }

        public void OnEndDrag(PointerEventData eventData)
        {
        }
    }
}