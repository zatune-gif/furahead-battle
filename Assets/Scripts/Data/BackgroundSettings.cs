using UnityEngine;

[CreateAssetMenu(fileName = "BackgroundSettings", menuName = "PeraperaBattle/BackgroundSettings")]
public class BackgroundSettings : ScriptableObject
{
    public Sprite[] backgrounds;

    public int Count => backgrounds != null ? backgrounds.Length : 0;

    public Sprite GetRandom()
    {
        if (backgrounds == null || backgrounds.Length == 0) return null;
        return backgrounds[Random.Range(0, backgrounds.Length)];
    }

    public Sprite GetByIndex(int index)
    {
        if (backgrounds == null || backgrounds.Length == 0) return null;
        return backgrounds[Mathf.Clamp(index, 0, backgrounds.Length - 1)];
    }
}
