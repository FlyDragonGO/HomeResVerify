using UnityEngine.EventSystems;

namespace HomeResVerify
{
    public class BaseEvent {

        public string type;
        public object[] datas;

        public BaseEvent(string type)
        {
            this.type = type;
        }

        public BaseEvent(string type,params object[] datas)
        {
            this.type = type;
            this.datas = datas;
        }

    }
    
    public class UIPointerClickEvent : BaseEvent
    {
        private static UIPointerClickEvent OnEvent { get; } = new UIPointerClickEvent();

        private UIPointerClickEvent() : base("UI_CLICK_EVENT")
        {
        }
    
        public static UIPointerClickEvent GetEventItem(PointerEventData data)
        {
            OnEvent.datas = new []{data};
            return OnEvent;
        }
    }
}