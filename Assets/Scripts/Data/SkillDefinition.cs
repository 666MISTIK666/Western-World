using UnityEngine;

[CreateAssetMenu(menuName = "WW/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    public string skillID;             // уникальный ID скила, напр. "HP_UP"
    public FighterDefinition fighter;  // ссылка на конкретного юнита
    public int bonusMaxHP;             // прибавка к maxHP
    public int cost;                   // стоимость в монетах
}