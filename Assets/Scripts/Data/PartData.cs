using UnityEngine;

[CreateAssetMenu(fileName = "PartData", menuName = "PeraperaBattle/PartData")]
public class PartData : ScriptableObject
{
    public string partName;
    public Sprite sprite;
    public PartType partType;
    public Vector2 anchorOffset;
}
