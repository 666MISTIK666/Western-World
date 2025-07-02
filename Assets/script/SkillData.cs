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
        Health,             // ���������� ��������� �������� ��������
        HealthFlat,         // ������������� ����������� � ��������
        Damage,             // ���������� ��������� �����
        CriticalHitChance,  // ���������� ��������� ����� ����.�����
        CriticalHitDamage,  // ���������� ��������� ����� ����.�����
        HealthRegeneration, // ���������� ��������� ����������� ��������
        DamageMultiplier    // ���������� ��������� ��������� �����
    }

    public EffectType type;
    public float value;
}
