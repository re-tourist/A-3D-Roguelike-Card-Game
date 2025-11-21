#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class ESCMenuSceneSetup
{
    [MenuItem("Tools/ESC Menu/Setup In Scene")]
    public static void SetupInScene()
    {
        // 1) 查找或创建 UIManager
        var uiManager = Object.FindAnyObjectByType<UIManager>();
        if (uiManager == null)
        {
            var uiRoot = new GameObject("UI_Root");
            uiManager = uiRoot.AddComponent<UIManager>();
            Debug.Log("[ESCMenu] UIManager created.");
        }

        // 2) 查找或创建 Canvas
        var canvas = Object.FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            var canvasGo = new GameObject("UI_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            Debug.Log("[ESCMenu] Canvas created.");
        }

        // 2.1) 确保 EventSystem 存在
        var eventSystem = Object.FindAnyObjectByType<EventSystem>();
        if (eventSystem == null)
        {
            var esGo = new GameObject("EventSystem", typeof(EventSystem));
            // 默认添加 StandaloneInputModule（旧输入系统）；如使用新输入系统，可手动替换为 InputSystemUIInputModule
            esGo.AddComponent<StandaloneInputModule>();
            Debug.Log("[ESCMenu] EventSystem created.");
        }

        // 3) 加载 ESCMenuView 预制体
        const string prefabPath = "Assets/Prefabs/UI/ESCMenu/ESCMenuView.prefab";
        var escPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (escPrefab == null)
        {
            Debug.LogError($"[ESCMenu] Prefab not found at {prefabPath}. 请先执行 Tools > ESC Menu > Create Prefabs。");
            return;
        }

        // 4) 实例化到 Canvas 下
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(escPrefab);
        var rt = instance.GetComponent<RectTransform>();
        if (rt == null) rt = instance.AddComponent<RectTransform>();
        rt.SetParent(canvas.transform, false);

        // 5) 获取（或添加）各需要的组件
        var rootCg = instance.GetComponent<CanvasGroup>();
        if (rootCg == null) rootCg = instance.AddComponent<CanvasGroup>();

        var darkMask = instance.transform.Find("DarkMask");
        var riftGroup = instance.transform.Find("RiftGroup");
        var menuButtons = instance.transform.Find("MenuButtons");

        if (darkMask == null || riftGroup == null || menuButtons == null)
        {
            Debug.LogError("[ESCMenu] 预制体结构不完整，需包含 DarkMask / RiftGroup / MenuButtons。");
            return;
        }

        var darkMaskCg = darkMask.GetComponent<CanvasGroup>();
        if (darkMaskCg == null) darkMaskCg = darkMask.gameObject.AddComponent<CanvasGroup>();

        var riftGroupCg = riftGroup.GetComponent<CanvasGroup>();
        if (riftGroupCg == null) riftGroupCg = riftGroup.gameObject.AddComponent<CanvasGroup>();

        var menuButtonsCg = menuButtons.GetComponent<CanvasGroup>();
        if (menuButtonsCg == null) menuButtonsCg = menuButtons.gameObject.AddComponent<CanvasGroup>();

        var left = riftGroup.Find("RiftLeft");
        var right = riftGroup.Find("RiftRight");
        var glow = riftGroup.Find("RiftGlow");
        if (left == null || right == null || glow == null)
        {
            Debug.LogWarning("[ESCMenu] RiftLeft/RiftRight/RiftGlow 未找到，将跳过可选 Glow 设置。");
        }

        // 6) 挂载并接线 ESCMenuAnimator
        var animator = instance.GetComponent<ESCMenuAnimator>();
        if (animator == null) animator = instance.AddComponent<ESCMenuAnimator>();

        animator.leftRift = left != null ? left.GetComponent<RectTransform>() : null;
        animator.rightRift = right != null ? right.GetComponent<RectTransform>() : null;
        animator.riftGroup = riftGroupCg;
        animator.darkMask = darkMaskCg;
        animator.menuButtons = menuButtonsCg;
        if (glow != null)
        {
            animator.riftGlow = glow.GetComponent<RectTransform>();
            var glowCg = glow.GetComponent<CanvasGroup>();
            if (glowCg == null) glowCg = glow.gameObject.AddComponent<CanvasGroup>();
            animator.riftGlowGroup = glowCg;
        }

        // 7) 挂载并接线 ESCMenuController
        var controller = instance.GetComponent<ESCMenuController>();
        if (controller == null) controller = instance.AddComponent<ESCMenuController>();
        var soCtrl = new SerializedObject(controller);
        soCtrl.FindProperty("rootCanvasGroup").objectReferenceValue = rootCg;
        soCtrl.FindProperty("animator").objectReferenceValue = animator;
        soCtrl.ApplyModifiedPropertiesWithoutUndo();

        // 8) 挂载并接线 ESCMenuInput
        var input = instance.GetComponent<ESCMenuInput>();
        if (input == null) input = instance.AddComponent<ESCMenuInput>();
        var soInput = new SerializedObject(input);
        soInput.FindProperty("escMenu").objectReferenceValue = controller;
        soInput.ApplyModifiedPropertiesWithoutUndo();

        // 9) 按钮绑定
        BindButton(menuButtons, "ResumeButton", controller.OnResume);
        BindButton(menuButtons, "SettingsButton", controller.OnSettings);
        BindButton(menuButtons, "QuitButton", controller.OnQuit);

        // 10) UIManager 接入 escMenu 字段
        var soUI = new SerializedObject(uiManager);
        soUI.FindProperty("escMenu").objectReferenceValue = controller;
        soUI.ApplyModifiedPropertiesWithoutUndo();

        Selection.activeObject = instance;
        Debug.Log("[ESCMenu] Scene setup completed. ESCMenu 已实例化并完成引用接线。");
    }

    private static void BindButton(Transform parent, string name, UnityEngine.Events.UnityAction handler)
    {
        var t = parent.Find(name);
        if (t == null)
        {
            Debug.LogWarning($"[ESCMenu] 未找到按钮 {name}，跳过绑定。");
            return;
        }
        var btn = t.GetComponent<Button>();
        if (btn == null)
        {
            Debug.LogWarning($"[ESCMenu] {name} 未挂载 Button 组件，跳过绑定。");
            return;
        }
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(handler);
    }
}
#endif