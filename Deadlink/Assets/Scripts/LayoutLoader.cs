using UnityEngine;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class UIStyle
{
    public string backgroundColor;
    public string textColor;
    public int fontSize;
}

[System.Serializable]
public class RectLayoutData
{
    public string element;
    public Vector2 position;
    public Vector2 size;
    public Vector2 anchorMin;
    public Vector2 anchorMax;
    public Vector2 pivot;
    public UIStyle style;
}

[System.Serializable]
public class LayoutCollection
{
    public List<RectLayoutData> layouts;
}

public class LayoutLoader : MonoBehaviour
{
    public string jsonFileName = "layout.json";

    void Start()
    {
        ApplyLayout();
    }

    public void ApplyLayout()
    {
        string path = Path.Combine(Application.streamingAssetsPath, jsonFileName);

        if (!File.Exists(path))
        {
            Debug.LogError("Layout file not found: " + path);
            return;
        }

        string json = File.ReadAllText(path);
        LayoutCollection collection = JsonUtility.FromJson<LayoutCollection>(json);

        foreach (RectLayoutData data in collection.layouts)
        {
            Transform child = transform.Find(data.element);
            if (child == null)
            {
                Debug.LogWarning($"UI Element '{data.element}' not found.");
                continue;
            }

            RectTransform rt = child.GetComponent<RectTransform>();
            if (rt == null)
            {
                Debug.LogWarning($"No RectTransform on '{data.element}'.");
                continue;
            }

            // Apply layout
            rt.anchorMin = data.anchorMin;
            rt.anchorMax = data.anchorMax;
            rt.pivot = data.pivot;
            rt.anchoredPosition = data.position;
            rt.sizeDelta = data.size;

            // Apply style if any
            if (data.style != null)
            {
                // Background color on Image
                if (!string.IsNullOrEmpty(data.style.backgroundColor))
                {
                    if (ColorUtility.TryParseHtmlString(data.style.backgroundColor, out Color bgColor))
                    {
                        Image img = child.GetComponent<Image>();
                        if (img != null)
                            img.color = bgColor;
                    }
                }

                // TextMeshPro text color and font size
                TextMeshProUGUI tmp = child.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null)
                {
                    if (!string.IsNullOrEmpty(data.style.textColor))
                    {
                        if (ColorUtility.TryParseHtmlString(data.style.textColor, out Color textColor))
                            tmp.color = textColor;
                    }

                    if (data.style.fontSize > 0)
                        tmp.fontSize = data.style.fontSize;
                }
            }
        }

        Debug.Log("Layout applied from JSON.");
    }
}
