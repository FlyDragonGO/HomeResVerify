using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class UIRoot :  Manager<UIRoot>
{
    public GameObject mRoot;
    // Start is called before the first frame update
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    public Vector2 GetScreenCanvasScale()
    {
        var rectTransform = mRoot.GetComponent<RectTransform>();
        var screenSize = new Vector2(Screen.width, Screen.height);
        return new Vector2(screenSize.x / rectTransform.rect.width, screenSize.y / rectTransform.rect.height);
    }
    
    public bool FitUIPos(GameObject ui, GameObject safeArea)
    {
        if (ui == null)
        {
            return false;
        }

        float marginTop = 0f;
        float marginBottom = 0f;
        float marginLeft = 0f;
        float marginRight = 0f;
        if (safeArea != null)
        {
            var canvasRect = this.mRoot.GetComponent<RectTransform>();
            var safeRect = safeArea.GetComponent<RectTransform>();
            var safeAreaLocalPos = this.mRoot.transform.InverseTransformPoint(safeArea.transform.position);
            float widthGap = Math.Abs(safeRect.rect.width - canvasRect.rect.width);
            float heightGap = Math.Abs(safeRect.rect.height - canvasRect.rect.height);
            if (widthGap > 1)
            {
                marginLeft = widthGap / 2;
                marginRight = widthGap / 2;
                marginLeft += safeAreaLocalPos.x;
                marginRight -= safeAreaLocalPos.x;
            }

            if (heightGap > 1)
            {
                marginTop = heightGap / 2;
                marginBottom = heightGap / 2;
                marginTop -= safeAreaLocalPos.y;
                marginBottom += safeAreaLocalPos.y;
            }
        }

        return FitUIPos(ui, marginTop, marginBottom, marginLeft, marginRight);
    }

    public bool FitUIPos(GameObject ui, float marginTop = 0, float marginBottom = 0, float marginLeft = 0,
        float marginRight = 0)
    {
        bool haveChange = false;
        if (ui == null)
        {
            return haveChange;
        }

        var canvasRect = this.mRoot.GetComponent<RectTransform>();
        var panelRect = ui.GetComponent<RectTransform>();
        float halfWidth = panelRect.rect.width / 2;
        float halfHeight = panelRect.rect.height / 2;

        var localPos = this.mRoot.transform.InverseTransformPoint(ui.transform.position);
        if (localPos.x > 0)
        {
            float safeWidth = canvasRect.rect.width / 2 - marginRight;
            if (localPos.x >= (safeWidth - halfWidth))
            {
                localPos.x = safeWidth - halfWidth;
                haveChange = true;
            }
        }
        else
        {
            float safeWidth = canvasRect.rect.width / 2 - marginLeft;
            if (localPos.x <= -(safeWidth - halfWidth))
            {
                localPos.x = -(safeWidth - halfWidth);
                haveChange = true;
            }
        }

        if (localPos.y > 0)
        {
            float safeHeight = canvasRect.rect.height / 2 - marginTop;
            if (localPos.y >= (safeHeight - halfHeight))
            {
                localPos.y = safeHeight - halfHeight;
                haveChange = true;
            }
        }
        else
        {
            float safeHeight = canvasRect.rect.height / 2 - marginBottom;
            if (localPos.y <= -(safeHeight - halfHeight))
            {
                localPos.y = -(safeHeight - halfHeight);
                haveChange = true;
            }
        }

        ui.transform.position = this.mRoot.transform.TransformPoint(localPos);
        return haveChange;
    }

}
