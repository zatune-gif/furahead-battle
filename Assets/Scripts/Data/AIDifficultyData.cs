using UnityEngine;

[CreateAssetMenu(fileName = "AIDifficultyData", menuName = "PeraperaBattle/AIDifficultyData")]
public class AIDifficultyData : ScriptableObject
{
    public float approachSpeed = 3f;
    public Vector2 waitTimeRange = new Vector2(0.5f, 1.5f);
    public int lowHPThreshold = 2;
    public float lowHPHeadbuttMultiplier = 1.5f;
}
