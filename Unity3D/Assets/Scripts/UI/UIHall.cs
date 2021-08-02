using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using BestHTTP;
using DragonU3DSDK.Asset;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UI;

namespace HomeResVerify
{
    class Commit
    {
        public Commit(DateTime time, string version)
        {
            this.time = time;
            this.version = version;
        }

        public DateTime time;
        public string version;
    }
    
    public class UIHall : UIBase
    {
        public const string RoomIdKey = "RoomIdKey";
        public const string DesignSizesIndexKey = "DesignSizesIndexKey";
        public const string ProjectNameIndexKey = "ProjectNameIndexKey";
        public const string ResTypeIndexKey = "ResTypeIndexKey";
        
        private string ResPubLibraryCommitUrl { get { return $"http://res.starcdn.cn/NewHomeRoom2DRes/{FilePathTools.targetName}/ResPubLibraryCommit.json";} }
        private string VersionURL { get { return $"http://res.starcdn.cn/NewHomeRoom2DRes/{FilePathTools.targetName}/{{0}}/Version.10000.txt"; } }

        private string[] ResTexPaths = 
        {
            $"configs/room/room{{0}}.ab",
            $"animations.ab",
            $"materials/common.ab",
            $"materials/room{{0}}.ab",
            $"prefabs/effect.ab",
            $"prefabs/room/room{{0}}.ab",
            $"spriteatlas/{{0}}/hd.ab",
            //$"spriteatlas/icon{{0}}/hd.ab"
        };
        private string[] ResTexGroups =
        {
            "Data",
            "Scene",
            "Scene",
            "Scene",
            "Scene",
            "Scene",
            "SpriteAtlas",
            //"SpriteAtlas"
        };
        
        private string[] ResSpinePaths = 
        {
            $"configs/room/room{{0}}.ab",
            $"prefabs/room/room{{0}}.ab",
        };
        private string[] ResSpineGroups =
        {
            "Data",
            "Scene",
        };
        
        public override void Init(params object[] objs)
        {
            string roomId = PlayerPrefs.GetString(RoomIdKey);
            InputField roomIdInputFiled = transform.Find("Root/RoomIDInput").GetComponent<InputField>();
            if (!string.IsNullOrEmpty(roomId)) roomIdInputFiled.text = roomId;
            roomIdInputFiled.onValueChanged.AddListener((b) => { PlayerPrefs.SetString(RoomIdKey, b); });
            
            Dropdown designSizeDropdown = transform.Find("Root/DesignSize/Dropdown").GetComponent<Dropdown>();
            designSizeDropdown.options.Clear();
            foreach (var p in RoomConst.DesignSizesName) designSizeDropdown.options.Add(new Dropdown.OptionData(p));
            designSizeDropdown.value = PlayerPrefs.GetInt(DesignSizesIndexKey);
            designSizeDropdown.onValueChanged.AddListener((a) => { PlayerPrefs.SetInt(DesignSizesIndexKey, a); });
            Vector2Int DesignSizes = RoomConst.DesignSizes[designSizeDropdown.value];
            if (DesignSizes.x > DesignSizes.y) Screen.orientation = ScreenOrientation.Landscape;
            else Screen.orientation = ScreenOrientation.Portrait;
            
            Dropdown projectNameDropdown = transform.Find("Root/ProjectName/Dropdown").GetComponent<Dropdown>();
            projectNameDropdown.options.Clear();
            foreach (var p in RoomConst.ProjectName) projectNameDropdown.options.Add(new Dropdown.OptionData(p));
            projectNameDropdown.value = PlayerPrefs.GetInt(ProjectNameIndexKey);
            projectNameDropdown.onValueChanged.AddListener((a) => { PlayerPrefs.SetInt(ProjectNameIndexKey, a); });

            Dropdown resTypeDropdown = transform.Find("Root/ResType/Dropdown").GetComponent<Dropdown>();
            resTypeDropdown.options.Clear();
            resTypeDropdown.options.Add(new Dropdown.OptionData("图片"));
            resTypeDropdown.options.Add(new Dropdown.OptionData("Spine"));
            resTypeDropdown.value = PlayerPrefs.GetInt(ResTypeIndexKey);
            resTypeDropdown.onValueChanged.AddListener((a) => { PlayerPrefs.SetInt(ResTypeIndexKey, a); });
            
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

            StartCoroutine(Load(roomId));
        }

        private IEnumerator Load(int roomId)
        {
            if (LocalVersionFileReady())
            {
                List<string> lackRes = ResReady(roomId);
                if (!transform.Find("Root/LastVersion").GetComponent<Toggle>().isOn)
                {
                    if (lackRes.Count == 0)
                    {
                        yield return StartCoroutine(VersionManager.Instance.LoadLocalVersionFile(() => { }));
                        EnterRoom(roomId);
                        yield break;
                    }
                }
            }
            
            transform.Find("Root/Load").GetComponent<Button>().interactable = false;
            yield return StartCoroutine(DownLoadRes(roomId));
            transform.Find("Root/Load").GetComponent<Button>().interactable = true;

            if (LocalVersionFileReady())
            {
                List<string> lackRes = ResReady(roomId);
                if (lackRes.Count == 0)
                {
                    yield return StartCoroutine(VersionManager.Instance.LoadLocalVersionFile(() => { }));
                    EnterRoom(roomId);
                }
                else
                {
                    string str = "缺少资源：";
                    foreach (var p in lackRes)
                    {
                        str += p + "\r\n";
                    }
                    UIManager.Instance.OpenUI<UIMessage>(str);
                }
            }
        }

        private void EnterRoom(int roomId)
        {
            RoomManager.Instance.Enter(roomId);
        }

        private bool LocalVersionFileReady()
        {
            string localVersionPath = string.Format("{0}/{1}", FilePathTools.persistentDataPath_Platform, "Version.txt");
            return File.Exists(localVersionPath);
        }

        private string[] ResPaths()
        {
            return PlayerPrefs.GetInt(ResTypeIndexKey) == 0 ? ResTexPaths : ResSpinePaths;
        }

        private string[] ResGroups()
        {
            return PlayerPrefs.GetInt(ResTypeIndexKey) == 0 ? ResTexGroups : ResSpineGroups;
        }
        
        private List<string> ResReady(int roomId)
        {
            List<string> lacks = new List<string>();
            foreach (var p in ResPaths())
            {
                string resPath = string.Format(p, roomId);
                string localPath = Path.Combine(Application.persistentDataPath, "DownLoad", FilePathTools.targetName, resPath);
                if(!File.Exists(localPath)) lacks.Add(resPath);
            }
            return lacks;
        }

        private enum DownLoadState
        {
            downloading,
            fail,
            success,
        }
        private IEnumerator DownLoadRes(int roomId)
        {
            #region download ResPubLibraryCommit.json
            DownLoadState downLoadState = DownLoadState.downloading;
            string lastCommit = "";
            DownLoad(ResPubLibraryCommitUrl, (state, value) =>
            {
                if (!state)
                {
                    downLoadState = DownLoadState.fail;
                    UIManager.Instance.OpenUI<UIMessage>($"无法下载：{ResPubLibraryCommitUrl}");
                    return;
                }
                var table = JsonConvert.DeserializeObject<Hashtable>(value);

                List<Commit> commits = new List<Commit>();
                commits.Clear();
                foreach (var key in table.Keys)
                {
                    var timeObj = JsonConvert.DeserializeObject<Hashtable>(table[key].ToString());
                    var timeStr = timeObj["time"] as string;
                    timeStr = timeStr.Remove(10, 1);
                    timeStr = timeStr.Insert(10, " ");
                    var dateTime = Convert.ToDateTime(timeStr);

                    commits.Add(new Commit(dateTime, key as string));
                }
                commits.Sort((a, b) => a.time < b.time ? 1 : -1);
                lastCommit = commits[0].version;
                downLoadState = DownLoadState.success;
            });
            while (DownLoadState.downloading == downLoadState) yield return null;
            if(DownLoadState.fail == downLoadState) yield break;
            #endregion

            #region download Version.10000.txt
            downLoadState = DownLoadState.downloading;
            string versionUrl = string.Format(VersionURL, lastCommit);
            DownLoad(versionUrl, (state, value) =>
            {
                if (!state)
                {
                    downLoadState = DownLoadState.fail;
                    UIManager.Instance.OpenUI<UIMessage>($"无法下载：{versionUrl}");
                    return;
                }
                CreateFile(FilePathTools.persistentDataPath_Platform, "Version.txt", value);
                
                downLoadState = DownLoadState.success;
            });
            while (0 == downLoadState) yield return null;
            if(DownLoadState.fail == downLoadState) yield break;
            yield return StartCoroutine(VersionManager.Instance.LoadLocalVersionFile(() => { }));
            #endregion
            
            #region download res
            Dictionary<string, string> needDownloadFiles = new Dictionary<string, string>();
            for (int i = 0; i < ResPaths().Length; i++)
            {
                string resPath = string.Format(ResPaths()[i], roomId);
                int resState = ResNeedDown(resPath, ResGroups()[i], VersionManager.Instance.GetLocalVersion());
                if(0 == resState) yield break;
                if (2 == resState) needDownloadFiles.Add(resPath, VersionManager.Instance.GetLocalVersion().GetAssetBundleMd5(ResGroups()[i], resPath));
            }
            List<DownloadInfo> allTask = new List<DownloadInfo>();
            bool ResourceCheckedOver = false;
            if (needDownloadFiles.Count > 0) //去下载
            {
                int resCount = needDownloadFiles.Count;
                foreach (KeyValuePair<string, string> kv in needDownloadFiles)
                {
                    DownloadInfo info = DownloadManager.Instance.DownloadInSeconds($"{lastCommit}/v10000_0_1", kv.Key, kv.Value,
                        (downloadinfo) =>
                    {
                        if (downloadinfo.result == DownloadResult.Success)
                        {
                            resCount--;
                            if (resCount <= 0) // 所有文件都成功下载到本地了
                            {
                                ResourceCheckedOver = true;
                            }
                        }
                        else
                        {
                            if (downloadinfo.result != DownloadResult.ForceAbort)
                            {
                                UIManager.Instance.OpenUI<UIMessage>($"无法下载：{kv.Key}");
                            }
                        }
                    });
                    allTask.Add(info);
                }
            }
            else
            {
                ResourceCheckedOver = true;
            }
            while (!ResourceCheckedOver)
            {
                // 更新进度
                int taskCount = allTask.Count;
                if (taskCount > 0)
                {
                    float downloadedBytes = 0f;
                    float totalBytes = 0f;
                    for (int i = 0; i < taskCount; i++)
                    {
                        if (allTask[i].downloadSize > allTask[i].downloadedSize) //确保get httphead之后，才开始算进度
                        {
                            totalBytes += allTask[i].downloadSize;
                            downloadedBytes += allTask[i].downloadedSize;
                        }
                    }

                    if (totalBytes > 0)
                    {
                        float rate = downloadedBytes / totalBytes;
                        transform.Find("Root/DownLoadProgress").GetComponent<Slider>().value = rate;
                    }
                }

                yield return null;
            }
            #endregion
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

        //ret：0-异常 1-不用下载 2-需要下载
        private int ResNeedDown(string path, string group, VersionInfo versionInfo)
        {
            string remoteMd5 = versionInfo.GetAssetBundleMd5(group, path);
            if (string.IsNullOrEmpty(remoteMd5))
            {
                UIManager.Instance.OpenUI<UIMessage>($"服务器缺少资源：{path}");
                return 0;
            }
            string localPath = Path.Combine(Application.persistentDataPath, "DownLoad", FilePathTools.targetName, path);
            string localMd5 = "";
            if (File.Exists(localPath)) localMd5 = AssetUtils.BuildFileMd5(localPath);
            if (!localMd5.Equals(remoteMd5)) return 2;
            return 1;
        }
        
        private void CreateFile(string path, string filename, string info)
        {
            StreamWriter sw;
            FileInfo t = new FileInfo(path + "/" + filename);
            DirectoryInfo dir = t.Directory;
            if (!dir.Exists)
            {
                dir.Create();
            }

            sw = t.CreateText();
            Debug.Log("write version:" + t);
            //以行的形式写入信息
            sw.WriteLine(info);
            //关闭流
            sw.Close();
            //销毁流
            sw.Dispose();
        }
    }
}