// Assets/Scripts/SkillData.cs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillData", menuName = "Data/SkillData")]
public class SkillData : ScriptableObject
{
    [Header("Identity")]
    public string skillName;

    [Header("Icon")]
    public Sprite skillIcon;

    [Header("Description")]
    [TextArea] public string description;

    [Header("Effect Summary")]
    [TextArea] public string effectDescription;

    [Header("Effects")]
    public List<SkillEffect> effects = new List<SkillEffect>();

    [Header("Purchase Costs")]
    public List<TrainingPanelManager.ResourceCost> costs = new List<TrainingPanelManager.ResourceCost>();
}

[System.Serializable]
public class SkillEffect
{
    public enum EffectType
    {
        Health,             // процентное изменение базового здоровья
        HealthFlat,         // фиксированное прибавление к здоровью
        Damage,             // процентное изменение урона
        CriticalHitChance,  // процентное изменение шанса крит.удара
        CriticalHitDamage,  // процентное изменение урона крит.удара
        HealthRegeneration, // процентное изменение регенерации здоровья
        DamageMultiplier    // процентное изменение множителя урона
    }

    public EffectType type;
    public float value;
}
