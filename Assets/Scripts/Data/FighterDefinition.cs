using UnityEngine;

[CreateAssetMenu(menuName = "WW/FighterDefinition")]
public class FighterDefinition : ScriptableObject
{
    public string fighterID;       // уникальный ID, напр. "Cowboy_Horse"
    public string displayName;     // название для UI: "Ковбой на лошади"
    public GameObject prefab;      // сам префаб юнита
    public int baseMaxHP;          // стартовый запас HP
}