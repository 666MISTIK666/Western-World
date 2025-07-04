using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkillCard : MonoBehaviour
{
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI effectDescriptionText;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    private SkillData skillData;
    private FighterData fighterData;
    private FighterStats fighterStats;
    private ResourceController resourceController;
    private TrainingPanelManager panelManager;
    private Academy academy;

    public void Setup(SkillData skill, FighterData fighter, FighterStats stats, ResourceController resourceCtrl, TrainingPanelManager panelMgr)
    {
        skillData = skill;
        fighterData = fighter;
        fighterStats = stats;
        resourceController = resourceCtrl;
        panelManager = panelMgr;

        academy = panelMgr.GetComponentInParent<Academy>();
        if (academy == null)
        {
            academy = Object.FindObjectOfType<Academy>();
        }

        EnsureFighterDataRegistered();

        if (skillIcon != null && skill.skillIcon != null)
            skillIcon.sprite = skill.skillIcon;
        if (skillNameText != null)
            skillNameText.text = skill.skillName;
        if (effectDescriptionText != null)
            effectDescriptionText.text = skill.effectDescription;

        if (costText != null && skill.costs != null)
        {
            int totalCost = 0;
            foreach (var cost in skill.costs)
                totalCost += cost.amount;
            costText.text = totalCost.ToString();
        }
        else if (costText != null)
            costText.text = "0";

        if (buyButton != null)
            buyButton.onClick.AddListener(BuySkill);
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseCard);

        if (fighterData.purchasedSkills.Contains(skill.skillName))
        {
            buyButton.interactable = false;
            costText.text = "Куплено";
        }
    }

    private void EnsureFighterDataRegistered()
    {
        if (FighterDataManager.Instance == null || fighterData == null || fighterStats == null)
        {
            return;
        }

        var existingData = FighterDataManager.Instance.GetFighterData()
                            .Find(f => f.fighterId == fighterData.fighterId);
        if (existingData == null)
        {
            if (fighterData.prefab == null && academy != null)
            {
                var type = academy.GetAllFighterData().Find(f => f.name == fighterData.name);
                if (type != null)
                    fighterData.prefab = type.prefab;
            }
            FighterDataManager.Instance.UpdateFighterData(fighterData);
        }
    }

    private void BuySkill()
    {
        if (resourceController == null || skillData == null || fighterData == null || fighterStats == null)
        {
            Debug.LogWarning("BuySkill: Один из необходимых компонентов отсутствует!");
            return;
        }

        bool canAfford = true;
        foreach (var cost in skillData.costs)
        {
            string resourceName = cost.resourceType.ToString();
            int currentAmount = resourceController.GetResource(resourceName);
            if (currentAmount < cost.amount)
            {
                canAfford = false;
                break;
            }
        }
        if (!canAfford)
        {
            Debug.Log($"Недостаточно ресурсов для покупки навыка {skillData.skillName}!");
            return;
        }

        foreach (var cost in skillData.costs)
            resourceController.AddResource(cost.resourceType.ToString(), -cost.amount);

        string fighterName = fighterData.name;
        FighterDataManager.Instance.AddPurchasedSkill(fighterName, skillData.skillName);

        // Обновляем локальный fighterData
        if (!fighterData.purchasedSkills.Contains(skillData.skillName))
        {
            fighterData.purchasedSkills.Add(skillData.skillName);
            fighterData.ReapplySkills();
            FighterDataManager.Instance.UpdateFighterData(fighterData);
            Debug.Log($"Локальный fighterData для {fighterName} (ID: {fighterData.fighterId}) обновлён: HP={fighterData.currentHealth}/{fighterData.initialHealth}, Навыки: {string.Join(", ", fighterData.purchasedSkills)}");
        }

        // Синхронизируем всех бойцов в сцене
        var allStats = Object.FindObjectsOfType<FighterStats>();
        foreach (var stats in allStats)
        {
            if (stats.fighterName == fighterName && !stats.isPreviewModel)
                stats.SyncWithFighterDataManager();
        }

        CloseCard();
    }

    private void CloseCard()
    {
        if (panelManager != null)
            panelManager.UpdateShelves();
        Destroy(gameObject);
    }
}