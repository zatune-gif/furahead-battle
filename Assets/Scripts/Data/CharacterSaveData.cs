using UnityEngine;

public static class CharacterSaveData
{
    static readonly string[] Keys = { "head", "body", "armL", "armR", "legL", "legR" };

    public static void Save(int[] indices)
    {
        for (int i = 0; i < Keys.Length; i++)
            PlayerPrefs.SetInt(Keys[i], indices[i]);
        PlayerPrefs.Save();
    }

    public static int[] Load()
    {
        var result = new int[Keys.Length];
        for (int i = 0; i < Keys.Length; i++)
            result[i] = PlayerPrefs.GetInt(Keys[i], 0);
        return result;
    }

    public static void SaveBg(int index)
    {
        PlayerPrefs.SetInt("bg", index);
        PlayerPrefs.Save();
    }

    public static int LoadBg() => PlayerPrefs.GetInt("bg", 0);
}
