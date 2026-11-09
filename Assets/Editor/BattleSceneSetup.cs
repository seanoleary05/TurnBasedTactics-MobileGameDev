using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

// One-shot scene wiring for the battle: Tools > Setup Battle Scene.
// Removes DebugSpawn, adds a TurnManager, and builds the HUD (banner, End Turn, result panel)
// under SafeArea. Safe to re-run: it reuses anything it already created. Undo-able.
public static class BattleSceneSetup
{
    const string UnitPrefabPath = "Assets/Prefabs/Unit.prefab";

    // Canvas is Scale With Screen Size, 800 wide. On the A15 (1080 px, 450 dpi) one canvas
    // unit is about 1.35 px, so 44 dp (124 px) is about 92 units. Buttons below are 110.
    const float ButtonHeight = 110f;

    [MenuItem("Tools/Setup Battle Scene")]
    static void Setup()
    {
        Undo.SetCurrentGroupName("Setup Battle Scene");

        // 1. Remove DebugSpawn (it spawns its own units and would clash with TurnManager).
        //    Matched by name so this tool keeps compiling once DebugSpawn.cs is deleted.
        foreach (var mb in Object.FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include))
            if (mb != null && mb.GetType().Name == "DebugSpawn")
                Undo.DestroyObjectImmediate(mb);

        // 2. HUD under SafeArea, drawn behind the pause panel.
        // The Canvas also carries a disabled SafeArea; we want the child "SafeArea" object.
        SafeArea safeArea = null;
        foreach (var sa in Object.FindObjectsByType<SafeArea>(FindObjectsInactive.Include))
            if (sa.enabled && sa.GetComponent<Canvas>() == null) safeArea = sa;
        if (safeArea == null)
        {
            Debug.LogError("[Setup] No enabled SafeArea object found under the Canvas.");
            return;
        }
        var root = (RectTransform)safeArea.transform;
        Undo.SetTransformParent(root, root.parent, "Reorder");
        root.SetSiblingIndex(0);   // pause Panel comes after it, so pause draws on top

        var banner = FindOrCreate(root, "TurnBanner", () =>
        {
            var t = NewText("TurnBanner", root, "Turn 1: your move", 44);
            Anchor(t.rectTransform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0.5f, 1),
                   new Vector2(0, -20), new Vector2(-40, 90));
            return t.gameObject;
        }).GetComponent<TMP_Text>();

        var endTurn = FindOrCreate(root, "EndTurnButton", () =>
        {
            var b = NewButton("EndTurnButton", root, "End Turn", new Color(0.15f, 0.45f, 0.85f));
            Anchor((RectTransform)b.transform, new Vector2(0.5f, 0), new Vector2(0.5f, 0),
                   new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(320, ButtonHeight));
            return b.gameObject;
        }).GetComponent<Button>();

        GameObject resultPanel = FindOrCreate(root, "ResultPanel", () =>
        {
            var panel = new GameObject("ResultPanel", typeof(RectTransform), typeof(Image));
            Undo.RegisterCreatedObjectUndo(panel, "Create ResultPanel");
            panel.transform.SetParent(root, false);
            panel.GetComponent<Image>().color = new Color(0, 0, 0, 0.75f);  // also blocks board taps
            Anchor((RectTransform)panel.transform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f),
                   Vector2.zero, Vector2.zero);

            var text = NewText("ResultText", panel.transform, "Victory!", 56);
            Anchor(text.rectTransform, new Vector2(0, 0.5f), new Vector2(1, 0.5f), new Vector2(0.5f, 0),
                   new Vector2(0, 20), new Vector2(-40, 260));

            var retry = NewButton("RetryButton", panel.transform, "Retry", new Color(0.2f, 0.65f, 0.3f));
            Anchor((RectTransform)retry.transform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                   new Vector2(0.5f, 1), new Vector2(0, -20), new Vector2(320, ButtonHeight));
            panel.SetActive(false);
            return panel;
        });
        root.Find("ResultPanel").SetAsLastSibling();
        var resultText = resultPanel.transform.Find("ResultText").GetComponent<TMP_Text>();
        var retryButton = resultPanel.transform.Find("RetryButton").GetComponent<Button>();

        // 3. TurnManager.
        var tm = Object.FindAnyObjectByType<TurnManager>(FindObjectsInactive.Include);
        if (tm == null)
        {
            var go = new GameObject("TurnManager");
            Undo.RegisterCreatedObjectUndo(go, "Create TurnManager");
            tm = go.AddComponent<TurnManager>();
        }

        var so = new SerializedObject(tm);
        so.FindProperty("unitPrefab").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<Unit>(UnitPrefabPath);
        so.FindProperty("endTurnButton").objectReferenceValue = endTurn;
        so.FindProperty("banner").objectReferenceValue = banner;
        so.FindProperty("resultPanel").objectReferenceValue = resultPanel;
        so.FindProperty("resultText").objectReferenceValue = resultText;
        so.ApplyModifiedProperties();

        // 4. Button clicks (persistent, so they show in the Inspector).
        Rewire(endTurn, tm.EndPlayerTurn);
        Rewire(retryButton, tm.Restart);

        EditorSceneManager.MarkSceneDirty(tm.gameObject.scene);
        Selection.activeGameObject = tm.gameObject;
        Debug.Log("[Setup] Battle scene wired. Save the scene (Ctrl+S).");
    }

    // ---------- helpers ----------

    static GameObject FindOrCreate(Transform parent, string name, System.Func<GameObject> create)
    {
        var existing = parent.Find(name);
        return existing != null ? existing.gameObject : create();
    }

    static void Rewire(Button b, UnityEngine.Events.UnityAction action)
    {
        Undo.RecordObject(b, "Wire button");
        for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(b.onClick, i);
        UnityEventTools.AddPersistentListener(b.onClick, action);
    }

    static TMP_Text NewText(string name, Transform parent, string text, float size)
    {
        var go = new GameObject(name, typeof(RectTransform));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        var t = go.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        t.raycastTarget = false;   // don't swallow board taps
        return t;
    }

    static Button NewButton(string name, Transform parent, string label, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        Undo.RegisterCreatedObjectUndo(go, "Create " + name);
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = color;

        var t = NewText("Label", go.transform, label, 42);
        Anchor(t.rectTransform, Vector2.zero, Vector2.one, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
        return go.GetComponent<Button>();
    }

    static void Anchor(RectTransform rt, Vector2 min, Vector2 max, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
    }
}
