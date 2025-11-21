#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;

public static class ESCMenuPrefabCreator
{
    [MenuItem("Tools/ESC Menu/Create Prefabs")]
    public static void CreatePrefabs()
    {
        var root = new GameObject("ESCMenu", typeof(RectTransform), typeof(CanvasGroup));
        var rootRt = root.GetComponent<RectTransform>();
        rootRt.anchorMin = Vector2.zero;
        rootRt.anchorMax = Vector2.one;
        rootRt.offsetMin = Vector2.zero;
        rootRt.offsetMax = Vector2.zero;

        // DarkMask
        var darkMask = new GameObject("DarkMask", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        var dmRt = darkMask.GetComponent<RectTransform>();
        dmRt.SetParent(root.transform, false);
        dmRt.anchorMin = Vector2.zero;
        dmRt.anchorMax = Vector2.one;
        dmRt.offsetMin = Vector2.zero;
        dmRt.offsetMax = Vector2.zero;
        var dmImg = darkMask.GetComponent<Image>(); dmImg.color = new Color(0, 0, 0, 0.4f);
        var dmCg = darkMask.GetComponent<CanvasGroup>(); dmCg.alpha = 0f;

        // RiftGroup
        var riftGroup = new GameObject("RiftGroup", typeof(RectTransform), typeof(CanvasGroup));
        var rgRt = riftGroup.GetComponent<RectTransform>();
        rgRt.SetParent(root.transform, false);
        rgRt.anchorMin = rgRt.anchorMax = new Vector2(0.5f, 0.5f);
        rgRt.sizeDelta = new Vector2(800, 600);
        var rgCg = riftGroup.GetComponent<CanvasGroup>(); rgCg.alpha = 0f;

        var left = CreateImage("RiftLeft", riftGroup.transform);
        var right = CreateImage("RiftRight", riftGroup.transform);
        var glow = CreateImage("RiftGlow", riftGroup.transform);

        // MenuButtons
        var menuButtons = new GameObject("MenuButtons", typeof(RectTransform), typeof(CanvasGroup), typeof(VerticalLayoutGroup));
        var mbRt = menuButtons.GetComponent<RectTransform>();
        mbRt.SetParent(root.transform, false);
        mbRt.anchorMin = mbRt.anchorMax = new Vector2(0.5f, 0.5f);
        mbRt.sizeDelta = new Vector2(420, 240);
        var mbCg = menuButtons.GetComponent<CanvasGroup>(); mbCg.alpha = 0f;
        var vlg = menuButtons.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 24, 24);
        vlg.spacing = 16;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        CreateButton("ResumeButton", menuButtons.transform, "继续");
        CreateButton("SettingsButton", menuButtons.transform, "Settings");
        CreateButton("QuitButton", menuButtons.transform, "Back to Menu");

        // 保存 Prefab
        var saveDir = "Assets/Prefabs/UI/ESCMenu";
        if (!Directory.Exists(saveDir)) Directory.CreateDirectory(saveDir);
        PrefabUtility.SaveAsPrefabAsset(root, $"{saveDir}/ESCMenuView.prefab");
        PrefabUtility.SaveAsPrefabAsset(left.gameObject, $"{saveDir}/RiftLeft.prefab");
        PrefabUtility.SaveAsPrefabAsset(right.gameObject, $"{saveDir}/RiftRight.prefab");
        PrefabUtility.SaveAsPrefabAsset(glow.gameObject, $"{saveDir}/RiftGlow.prefab");

        Object.DestroyImmediate(root);
        Debug.Log("[ESCMenu] Prefabs created.");
    }

    static Image CreateImage(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(256, 512);
        var img = go.GetComponent<Image>();
        img.color = new Color(0.3f, 0.0f, 0.4f, 0.9f); // 无素材时的占位色
        return img;
    }

    static void CreateButton(string name, Transform parent, string label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(380, 56);
        var img = go.GetComponent<Image>(); img.color = new Color(1, 1, 1, 0.08f);

        var textGo = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(Outline), typeof(Shadow));
        var trt = textGo.GetComponent<RectTransform>();
        trt.SetParent(go.transform, false);
        trt.anchorMin = Vector2.zero; trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(12, 8); trt.offsetMax = new Vector2(-12, -8);
        var text = textGo.GetComponent<TextMeshProUGUI>();
        text.text = label; text.alignment = TextAlignmentOptions.Center; text.fontSize = 28; text.color = Color.white;
        if (TMP_Settings.defaultFontAsset != null) text.font = TMP_Settings.defaultFontAsset;
        var ol = textGo.GetComponent<Outline>(); ol.effectColor = new Color(0.95f, 0.8f, 0.3f);
        var sh = textGo.GetComponent<Shadow>(); sh.effectColor = new Color(0.2f, 0.1f, 0.0f);
    }
}
#endif