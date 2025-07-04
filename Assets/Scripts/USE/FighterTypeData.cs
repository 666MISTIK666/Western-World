using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FighterTypeData
{
    public string fighterName; // Имя типа бойца, например "Ковбой"
    public GameObject prefab;  // Префаб бойца
    public List<SkillData> skills; // Доступные навыки для этого типа
    public List<string> purchasedSkills; // Купленные навыки для этого типа
}