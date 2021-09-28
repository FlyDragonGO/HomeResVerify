using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Home.Core
{
    public class HomeHandleRender : MonoBehaviour
    {
        public Camera MainCamera;
        private Camera CustomCamera;
        private GameObject ScreenImage;
        private int ActiveLayer, DisactiveLayer;
        private RenderTexture RT0;
        private RenderTexture RT1;
        private List<List<GameObject>> ItemsQueue = new List<List<GameObject>>();
        
        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            ActiveLayer = LayerMask.NameToLayer("HomeActiveRender");
            DisactiveLayer = LayerMask.NameToLayer("HomeDisActiveRender");
            RT0 = new RenderTexture(Screen.width, Screen.height, 0) { useMipMap = false, autoGenerateMips = false };
            RT1 = new RenderTexture(512, 512, 0) { useMipMap = false, autoGenerateMips = false };
            CreateCamera(RT0);
            CreateScreenImage(RT0);
        }

        private void OnWillRenderObject()
        {
            for (int i = 0; i < ItemsQueue.Count; i++)
            {
                List<GameObject> items = ItemsQueue[i];
                foreach (var p in items) { p.layer = ActiveLayer; }
                if(i > 0) Graphics.Blit(RT0, RT1);
                CustomCamera.Render();
                foreach (var p in items) { p.layer = DisactiveLayer; }
                if (i == 0) CustomCamera.clearFlags = CameraClearFlags.Nothing;
            }
        }

        private void CreateCamera(RenderTexture rt)
        {
            GameObject cameraGameObject = new GameObject();
            cameraGameObject.transform.parent = gameObject.transform;
            cameraGameObject.transform.localPosition = new Vector3(0.5f, 0.5f, 0.0f);
            CustomCamera = cameraGameObject.AddComponent<Camera>();
            CustomCamera.clearFlags = CameraClearFlags.Color;
            CustomCamera.backgroundColor = Color.black;
            CustomCamera.cullingMask = 1 << LayerMask.NameToLayer("HomeActiveRender");
            CustomCamera.orthographic = true;
            CustomCamera.orthographicSize = MainCamera.orthographicSize;
            CustomCamera.nearClipPlane = MainCamera.nearClipPlane;
            CustomCamera.farClipPlane = MainCamera.farClipPlane;
            CustomCamera.rect = new Rect(0,0,1,1);
            CustomCamera.depth = MainCamera.depth - 1;
            CustomCamera.renderingPath = RenderingPath.Forward;
            CustomCamera.targetTexture = rt;
            CustomCamera.useOcclusionCulling = false;
            CustomCamera.allowHDR = false;
            CustomCamera.allowMSAA = false;
            CustomCamera.allowDynamicResolution = false;
            CustomCamera.enabled = false;
        }

        public void SyncCamera()
        {
            CustomCamera.orthographicSize = MainCamera.orthographicSize;
            CustomCamera.nearClipPlane = MainCamera.nearClipPlane;
            CustomCamera.farClipPlane = MainCamera.farClipPlane;
        }

        private void CreateScreenImage(RenderTexture rt)
        {
            ScreenImage = gameObject;
            ScreenImage.transform.parent = transform;
            MeshFilter filter = ScreenImage.AddComponent<MeshFilter>();
            Mesh mesh = new Mesh();
            Vector3[] vertices = new Vector3[4]
            {
                new Vector3(0, 0, 0),
                new Vector3(1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(1, 1, 0)
            };
            mesh.vertices = vertices;
            int[] tris = new int[6]
            {
                0, 2, 1,
                2, 3, 1
            };
            Vector2[] uv = new Vector2[4]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            filter.mesh = mesh;
            mesh.triangles = tris;
            mesh.uv = uv;
            MeshRenderer renderer = ScreenImage.AddComponent<MeshRenderer>();
            Shader shader = Shader.Find("Home/HomSceneFullScreen");
            Material mat = new Material(shader);
            renderer.sharedMaterial = mat;
            mat.SetTexture("_MainTex", rt);
            ScreenImage.transform.localPosition = new Vector3(-HomeResVerify.RoomConst.DesignSizes[0].x * 0.005f, -HomeResVerify.RoomConst.DesignSizes[0].y * 0.005f);
            ScreenImage.transform.localScale = new Vector3(HomeResVerify.RoomConst.DesignSizes[0].x * 0.01f, HomeResVerify.RoomConst.DesignSizes[0].y * 0.01f);
        }

        public void HandleSceneItem(GameObject SceneRoot)
        {
            ItemsQueue.Clear();

            List<SpriteRenderer> sr = new List<SpriteRenderer>();
            sr.AddRange(SceneRoot.GetComponentsInChildren<SpriteRenderer>(true));
            sr.Sort((x, y) => x.sortingOrder - y.sortingOrder);
            List<GameObject> itmes = new List<GameObject>();
            for (int i = 0; i < sr.Count; i++)
            {
                if(!IsActive(sr[i].transform)) continue;
                if (sr[i].sharedMaterial.name.Equals("em_Home_SoftLight") ||
                    sr[i].sharedMaterial.name.Equals("em_Home_ColorBurn") ||
                    sr[i].sharedMaterial.name.Equals("em_Home_Screen"))
                {
                    sr[i].sharedMaterial.SetTexture("_RTTex", RT1);
                    List<GameObject> tempList = new List<GameObject>();
                    foreach (var t in itmes) tempList.Add(t);
                    ItemsQueue.Add(tempList);
                    itmes = new List<GameObject>();
                }
                //sr[i].gameObject.layer = ActiveLayer;
                itmes.Add(sr[i].gameObject);
            }
            if(itmes.Count > 0) ItemsQueue.Add(itmes);
        }

        public bool IsActive(Transform tra)
        {
            if (!tra.gameObject.activeSelf) return false;
            if (tra.parent != null) return IsActive(tra.parent);
            return true;
        }
    }
}