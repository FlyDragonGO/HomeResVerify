using System;
using System.Collections;
using System.Collections.Generic;
using BestHTTP;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace HomeResVerify
{
    class VersionInfo
    {
        public VersionInfo(DateTime time, string version)
        {
            this.time = time;
            this.version = version;
        }

        public DateTime time;
        public string version;
    }
    
    public class UIHall : UIBase
    {
        private const string ResPubLibraryCommitUrl =
#if UNITY_ANDROID
            "http://res.starcdn.cn/NewHomeRoom2DRes/android/ResPubLibraryCommit.json";
#else
            "http://res.starcdn.cn/NewHomeRoom2DRes/iphone/ResPubLibraryCommit.json";
#endif
        
        private const string VersionURL = 
#if UNITY_ANDROID
            "http://res.starcdn.cn/NewHomeRoom2DRes/android/{0}/Version.10000.txt";
#else
            "http://res.starcdn.cn/NewHomeRoom2DRes/iphone/{0}/Version.10000.txt";
#endif

        public override void Init(params object[] objs)
        {
            transform.Find("Root/Load").GetComponent<Button>().onClick.AddListener(LoadOnClick);
        }

        private void LoadOnClick()
        {
            int roomId = GetRoomId();
            if (roomId == 0)
            {
                UIManager.Instance.OpenUI<UIMessage>("无效的房间号。");
                return;
            }

            StartCoroutine(Load());
        }

        private IEnumerator Load()
        {
            transform.Find("Root/Load").GetComponent<Button>().interactable = false;
            yield return StartCoroutine(_Load());
            transform.Find("Root/Load").GetComponent<Button>().interactable = true;
        }
        
        private IEnumerator _Load()
        {
            int downLoadState = 0;//0下载中 1-失败 2-成功
            string lastVersion = "";
            DownLoad(ResPubLibraryCommitUrl, (state, value) =>
            {
                if (!state)
                {
                    downLoadState = 1;
                    UIManager.Instance.OpenUI<UIMessage>($"无法下载：{ResPubLibraryCommitUrl}");
                    return;
                }
                var table = JsonConvert.DeserializeObject<Hashtable>(value);

                List<VersionInfo> versionList = new List<VersionInfo>();
                versionList.Clear();
                foreach (var key in table.Keys)
                {
                    var timeObj = JsonConvert.DeserializeObject<Hashtable>(table[key].ToString());
                    var timeStr = timeObj["time"] as string;
                    timeStr = timeStr.Remove(10, 1);
                    timeStr = timeStr.Insert(10, " ");
                    var dateTime = Convert.ToDateTime(timeStr);

                    versionList.Add(new VersionInfo(dateTime, key as string));
                }
                versionList.Sort((a, b) => a.time < b.time ? 1 : -1);
                lastVersion = versionList[0].version;
                downLoadState = 2;
            });
            while (0 == downLoadState) yield return null;
            if(1 == downLoadState) yield break;

            downLoadState = 0;
            VersionInfo versionInfo = null;
            string versionUrl = string.Format(VersionURL, lastVersion);
            DownLoad(versionUrl, (state, value) =>
            {
                if (!state)
                {
                    downLoadState = 1;
                    UIManager.Instance.OpenUI<UIMessage>($"无法下载：{versionUrl}");
                    return;
                }
                versionInfo = JsonConvert.DeserializeObject<VersionInfo>(value);
                downLoadState = 2;
            });
            while (0 == downLoadState) yield return null;
            if(1 == downLoadState) yield break;
            Debug.LogError(versionInfo.version);
        }

        private int GetRoomId()
        {
            string text = transform.Find("Root/RoomIDInput").GetComponent<InputField>().text;
            if (string.IsNullOrEmpty(text)) return 0;
            return Convert.ToInt32(text);
        }

        private void DownLoad(string url, Action<bool, string> ret)
        {
            new HTTPRequest(new Uri(url), (req, rep) =>
            {
                if (rep == null)
                {
                    Debug.LogError($"can not download : {url}");
                    ret(false, null);
                }
                else if (rep.StatusCode >= 200 && rep.StatusCode < 300)
                {
                    ret(true, rep.DataAsText);
                }
                else
                {
                    Debug.LogError($"can not download : {url}");
                    ret(false, null);
                }
            })
            {
                DisableCache = true,
                IsCookiesEnabled = false,
                ConnectTimeout = TimeSpan.FromSeconds(5),
                Timeout = TimeSpan.FromSeconds(10)
            }.Send();
        }
    }
}