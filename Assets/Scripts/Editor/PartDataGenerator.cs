using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class PartDataGenerator
{
    const string SpritesRoot  = "Assets/Sprites";
    const string HeadsDir     = "Assets/Sprites/Parts/Heads";
    const string BodiesDir    = "Assets/Sprites/Parts/Bodies";
    const string ArmsDir      = "Assets/Sprites/Parts/Arms";
    const string LegsDir      = "Assets/Sprites/Parts/Legs";
    const string BgDir        = "Assets/Sprites/Backgrounds";
    const string OutputDir    = "Assets/ScriptableObjects/Parts";
    const string BgSettingsPath = "Assets/ScriptableObjects/BackgroundSettings.asset";

    // ─── Step 1: ファイルを適切なフォルダへ移動 ───────────────────────────
    [MenuItem("PeraperaBattle/1. Organize Sprites into Folders")]
    static void OrganizeSprites()
    {
        EnsureFolder("Assets/Sprites", "Parts");
        EnsureFolder("Assets/Sprites/Parts", "Heads");
        EnsureFolder("Assets/Sprites/Parts", "Bodies");
        EnsureFolder("Assets/Sprites/Parts", "Arms");
        EnsureFolder("Assets/Sprites/Parts", "Legs");
        EnsureFolder("Assets/Sprites", "Backgrounds");

        var guids = AssetDatabase.FindAssets("t:Texture2D", new[] { SpritesRoot });
        int moved = 0;
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var dir  = Path.GetDirectoryName(path).Replace('\\', '/');
            if (dir != SpritesRoot) continue;   // すでにサブフォルダにある

            var name = Path.GetFileNameWithoutExtension(path).ToLower();
            string dest = GetDestFolder(name);
            if (dest == null) continue;

            var err = AssetDatabase.MoveAsset(path, dest + "/" + Path.GetFileName(path));
            if (string.IsNullOrEmpty(err)) moved++;
            else Debug.LogWarning($"Move failed: {path} → {err}");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Organize] {moved} ファイルを移動しました。");
    }

    static string GetDestFolder(string nameLower)
    {
        if (nameLower.StartsWith("background"))              return BgDir;
        if (nameLower.StartsWith("head"))                    return HeadsDir;
        if (nameLower.StartsWith("body") || nameLower.StartsWith("back")) return BodiesDir;
        if (nameLower.StartsWith("arm")  || nameLower.StartsWith("hand")) return ArmsDir;
        if (nameLower.StartsWith("leg"))                     return LegsDir;
        return null;
    }

    // ─── Step 2: PartData & BackgroundSettings を生成 ────────────────────
    [MenuItem("PeraperaBattle/2. Generate Part Data from Sprites")]
    static void Generate()
    {
        EnsureFolder("Assets", "ScriptableObjects");
        EnsureFolder("Assets/ScriptableObjects", "Parts");

        var headParts = CreateParts(GetAllSprites(HeadsDir),  PartType.Head,  "Head");
        var bodyParts = CreateParts(GetAllSprites(BodiesDir), PartType.Body,  "Body");
        var armParts  = CreateParts(GetAllSprites(ArmsDir),   PartType.ArmL,  "Arm");
        var legParts  = CreateParts(GetAllSprites(LegsDir),   PartType.LegL,  "Leg");
        var armRParts = CloneParts(armParts, PartType.ArmR, "ArmR");
        var legRParts = CloneParts(legParts, PartType.LegR, "LegR");

        var bgSettings = GenerateBackgroundSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        var bm = Object.FindAnyObjectByType<BattleManager>(FindObjectsInactive.Include);
        if (bm != null)
        {
            bm.headParts = headParts;
            bm.bodyParts = bodyParts;
            bm.armLParts = armParts;
            bm.armRParts = armRParts;
            bm.legLParts = legParts;
            bm.legRParts = legRParts;
            bm.backgroundSettings = bgSettings;
            EditorUtility.SetDirty(bm);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(bm.gameObject.scene);
            Debug.Log($"[Generate] BattleManager アサイン完了 " +
                      $"Head:{headParts.Length} Body:{bodyParts.Length} " +
                      $"Arm:{armParts.Length} Leg:{legParts.Length} Bg:{bgSettings.backgrounds.Length}");
        }
        else
        {
            Debug.LogWarning("[Generate] BattleManager が見つかりません。BattleScene を開いてから実行してください。");
        }
        Debug.Log("[Generate] 完了！");
    }

    static BackgroundSettings GenerateBackgroundSettings()
    {
        var sprites = new List<Sprite>();
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { BgDir }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            EnsureSpriteImport(path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) sprites.Add(sprite);
        }
        var settings = AssetDatabase.LoadAssetAtPath<BackgroundSettings>(BgSettingsPath)
                       ?? CreateAsset<BackgroundSettings>(BgSettingsPath);
        settings.backgrounds = sprites.ToArray();
        EditorUtility.SetDirty(settings);
        return settings;
    }

    static Sprite[] GetAllSprites(string dir)
    {
        var result = new List<Sprite>();
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { dir }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            EnsureSpriteImport(path);
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) result.Add(sprite);
        }
        result.Sort((a, b) => string.Compare(a.name, b.name));
        return result.ToArray();
    }

    static void EnsureSpriteImport(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null || importer.textureType == TextureImporterType.Sprite) return;
        importer.textureType = TextureImporterType.Sprite;
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
    }

    // prefix に完全一致するスプライトのみ取得（head → head / head 1 / head 2 ...）
    static Sprite[] GetSprites(string dir, string prefix)
    {
        var result = new List<Sprite>();
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", new[] { dir }))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var name = Path.GetFileNameWithoutExtension(path).ToLower();
            if (!IsExactMatch(name, prefix)) continue;
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite != null) result.Add(sprite);
        }
        result.Sort((a, b) => string.Compare(a.name, b.name));
        return result.ToArray();
    }

    static bool IsExactMatch(string filename, string prefix)
    {
        if (filename == prefix) return true;
        if (filename.StartsWith(prefix + " "))
            return int.TryParse(filename.Substring(prefix.Length + 1), out _);
        return false;
    }

    static PartData[] CreateParts(Sprite[] sprites, PartType partType, string prefix)
    {
        var parts = new PartData[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            var path = $"{OutputDir}/{prefix}_{i}.asset";
            var part = AssetDatabase.LoadAssetAtPath<PartData>(path) ?? CreateAsset<PartData>(path);
            part.sprite = sprites[i]; part.partType = partType; part.partName = $"{prefix} {i}";
            EditorUtility.SetDirty(part);
            parts[i] = part;
        }
        return parts;
    }

    static PartData[] CloneParts(PartData[] source, PartType partType, string prefix)
    {
        var parts = new PartData[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            var path = $"{OutputDir}/{prefix}_{i}.asset";
            var part = AssetDatabase.LoadAssetAtPath<PartData>(path) ?? CreateAsset<PartData>(path);
            part.sprite = source[i].sprite; part.partType = partType; part.partName = $"{prefix} {i}";
            EditorUtility.SetDirty(part);
            parts[i] = part;
        }
        return parts;
    }

    static void EnsureFolder(string parent, string folder)
    {
        if (!AssetDatabase.IsValidFolder($"{parent}/{folder}"))
            AssetDatabase.CreateFolder(parent, folder);
    }

    static T CreateAsset<T>(string path) where T : ScriptableObject
    {
        var asset = ScriptableObject.CreateInstance<T>();
        AssetDatabase.CreateAsset(asset, path);
        return asset;
    }
}
