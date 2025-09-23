using UnityEngine;
[CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/ItemStats", order = 1)]
public class ItemStats : ScriptableObject
{
    public string Name;
    public string Description;
    public int cost;
    public Sprite InvSprite;
}
