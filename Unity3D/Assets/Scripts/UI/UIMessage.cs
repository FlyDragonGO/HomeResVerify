using UnityEngine.UI;

namespace HomeResVerify
{
    public class UIMessage : UIBase
    {
        public override void Init(params object[] objs)
        {
            string message = objs[0] as string;

            transform.Find("Root/Text").GetComponent<Text>().text = message;
            
            transform.Find("Root/Close").GetComponent<Button>().onClick.AddListener(() =>
            {
                UIManager.Instance.CloseUI<UIMessage>();
            });
        }
    }
}