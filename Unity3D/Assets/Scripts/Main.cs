using UnityEngine;

namespace HomeResVerify
{
    public class Main : MonoBehaviour
    {
        #region Singleton
        public static Main Instance;
        private void Awake()
        {
            if (Instance == null)
            {
                DontDestroyOnLoad(this.gameObject);

                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(this.gameObject);
            }
        }
        #endregion
    
        private void Start()
        {
            SRDebug.Init();
            SRDebug.Instance.LoadPinnedFromPlayerPrefs();
            
            UIManager.Instance.Init();
            UIManager.Instance.OpenUI<UIHall>();
        }
    }
}

