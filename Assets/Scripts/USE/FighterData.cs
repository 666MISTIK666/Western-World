using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "FighterData", menuName = "Data/FighterData")]
public class FighterData : ScriptableObject
{
    [Header("Identity")]
    public string fighterId;
    public string fighterName;
    public GameObject prefab;

    [Header("Description")]
    [TextArea] public string description;

    [Header("Costs")]
    public List<TrainingPanelManager.ResourceCost> costs = new List<TrainingPanelManager.ResourceCost>();

    [Header("Skills")]
    public List<SkillData> skills = new List<SkillData>();
    public List<string> purchasedSkills = new List<string>();

    [Header("Base Stats")]
    public float baseHealth;
    public float baseDamage;
    public float baseCriticalHitChance;
    public float baseCriticalHitDamage;
    public float baseHealthRegeneration;
    public float baseDamageMultiplier;

    [Header("Runtime Stats")]
    [HideInInspector] public float initialHealth;
    [HideInInspector] public float currentHealth;
    [HideInInspector] public float currentDamage;
    [HideInInspector] public float currentCriticalHitChance;
    [HideInInspector] public float currentCriticalHitDamage;
    [HideInInspector] public float currentHealthRegeneration;
    [HideInInspector] public float currentDamageMultiplier;

    /// <summary>
    /// Инициализирует базовые значения на основании переданных статистик
    /// </summary>
    public void InitializeCurrentStats(FighterStats stats)
    {
        baseHealth = stats.health;
        baseDamage = stats.damage;
        baseCriticalHitChance = stats.criticalHitChance;
        baseCriticalHitDamage = stats.criticalHitDamage;
        baseHealthRegeneration = stats.healthRegeneration;
        baseDamageMultiplier = stats.damageMultiplier;
        InitializeCurrentStats();
    }

    /// <summary>
    /// Инициализация и сброс текущих значений до базовых
    /// </summary>
    public void InitializeCurrentStats()
    {
        initialHealth = baseHealth;
        currentHealth = baseHealth;
        currentDamage = baseDamage;
        currentCriticalHitChance = baseCriticalHitChance;
        currentCriticalHitDamage = baseCriticalHitDamage;
        currentHealthRegeneration = baseHealthRegeneration;
        currentDamageMultiplier = baseDamageMultiplier;
        ReapplySkills();
    }

    /// <summary>
    /// Пересчитывает текущие параметры с учётом purchasedSkills
    /// </summary>
    public void ReapplySkills()
    {
        foreach (var skillName in purchasedSkills)
        {
            var skill = skills.FirstOrDefault(s => s.skillName == skillName);
            if (skill == null) continue;

            foreach (var effect in skill.effects)
            {
                switch (effect.type)
                {
                    case SkillEffect.EffectType.Health:
                        initialHealth *= 1 + effect.value;
                        currentHealth *= 1 + effect.value;
                        break;
                    case SkillEffect.EffectType.HealthFlat:
                        initialHealth += effect.value;
                        currentHealth += effect.value;
                        break;
                    case SkillEffect.EffectType.Damage:
                        currentDamage *= 1 + effect.value;
                        break;
                    case SkillEffect.EffectType.CriticalHitChance:
                        currentCriticalHitChance *= 1 + effect.value;
                        break;
                    case SkillEffect.EffectType.CriticalHitDamage:
                        currentCriticalHitDamage *= 1 + effect.value;
                        break;
                    case SkillEffect.EffectType.HealthRegeneration:
                        currentHealthRegeneration *= 1 + effect.value;
                        break;
                    case SkillEffect.EffectType.DamageMultiplier:
                        currentDamageMultiplier *= 1 + effect.value;
                        break;
                }
            }
        }
    }
}
