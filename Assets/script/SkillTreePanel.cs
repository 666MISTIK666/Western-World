// SkillTreePanel.cs
using UnityEngine;
using UnityEngine.UI;
using System;

public class SkillTreePanel : MonoBehaviour
{
    [SerializeField] private Button closeButton;       // Кнопка "Закрыть"
    [SerializeField] private Button[] skillButtons;    // Массив кнопок для навыков
    [SerializeField] private GameObject skillCardPrefab; // Префаб карточки навыка

    private Action onCloseCallback;     // Callback для уведомления о закрытии
    private FighterData fighterData;    // Данные бойца с навыками
    private FighterStats fighterStats;  // Статистика бойца
    private TrainingPanelManager panelManager; // Для передачи в SkillCard

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        else
            Debug.LogError("SkillTreePanel: CloseButton не назначен!");
    }

    public void Setup(FighterData fighter, FighterStats stats, TrainingPanelManager panelMgr, Action onClose)
    {
        onCloseCallback = onClose;
        fighterData = fighter;
        fighterStats = stats;
        panelManager = panelMgr;

        if (fighterData == null)
        {
            Debug.LogError("SkillTreePanel: fighterData равен null в Setup!");
            return;
        }

        SetupSkillButtons();
        Debug.Log($"Открыто древо навыков для {stats.fighterName}, навыков: {(fighterData.skills?.Count ?? 0)}, кнопок: {(skillButtons?.Length ?? 0)}");
    }

    private void SetupSkillButtons()
    {
        if (fighterData == null || fighterData.skills == null)
        {
            Debug.LogError("SkillTreePanel: fighterData или fighterData.skills равны null!");
            return;
        }

        if (skillButtons == null || skillButtons.Length == 0)
        {
            Debug.LogError("SkillTreePanel: skillButtons не назначен или пуст!");
            return;
        }

        int buttonCount = Mathf.Min(fighterData.skills.Count, skillButtons.Length);
        if (fighterData.skills.Count > skillButtons.Length)
        {
            Debug.LogWarning($"Количество навыков ({fighterData.skills.Count}) превышает количество кнопок ({skillButtons.Length}). Используются только первые {skillButtons.Length} навыков.");
        }

        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < buttonCount)
            {
                skillButtons[i].gameObject.SetActive(true);
                int index = i;
                skillButtons[i].onClick.RemoveAllListeners();
                skillButtons[i].onClick.AddListener(() => OpenSkillCard(fighterData.skills[index]));
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }

        Debug.Log($"Настроено {buttonCount} кнопок для навыков.");
    }

    private void OpenSkillCard(SkillData skill)
    {
        if (skillCardPrefab == null)
        {
            Debug.LogError("SkillTreePanel: SkillCardPrefab не назначен!");
            return;
        }

        GameObject card = Instantiate(skillCardPrefab, transform);
        SkillCard cardScript = card.GetComponent<SkillCard>();
        if (cardScript != null)
        {
            ResourceController resourceCtrl = FindObjectOfType<ResourceController>();
            if (resourceCtrl == null)
            {
                Debug.LogError("SkillTreePanel: ResourceController не найден!");
                Destroy(card);
                return;
            }
            if (panelManager == null)
            {
                Debug.LogError("SkillTreePanel: TrainingPanelManager не передан!");
                Destroy(card);
                return;
            }
            cardScript.Setup(skill, fighterData, fighterStats, resourceCtrl, panelManager);
            Debug.Log($"Открыта карточка навыка: {skill?.skillName ?? "Unknown"}");
        }
        else
        {
            Debug.LogError("SkillTreePanel: SkillCard скрипт не найден на префабе!");
            Destroy(card);
        }
    }

    private void ClosePanel()
    {
        onCloseCallback?.Invoke();
        Destroy(gameObject);
        Debug.Log("Панель древа навыков закрыта");
    }
}
