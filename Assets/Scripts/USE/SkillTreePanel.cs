// SkillTreePanel.cs
using UnityEngine;
using UnityEngine.UI;
using System;

public class SkillTreePanel : MonoBehaviour
{
    [SerializeField] private Button closeButton;       // ������ "�������"
    [SerializeField] private Button[] skillButtons;    // ������ ������ ��� �������
    [SerializeField] private GameObject skillCardPrefab; // ������ �������� ������

    private Action onCloseCallback;     // Callback ��� ����������� � ��������
    private FighterData fighterData;    // ������ ����� � ��������
    private FighterStats fighterStats;  // ���������� �����
    private TrainingPanelManager panelManager; // ��� �������� � SkillCard

    private void Awake()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(ClosePanel);
        else
            Debug.LogError("SkillTreePanel: CloseButton �� ��������!");
    }

    public void Setup(FighterData fighter, FighterStats stats, TrainingPanelManager panelMgr, Action onClose)
    {
        onCloseCallback = onClose;
        fighterData = fighter;
        fighterStats = stats;
        panelManager = panelMgr;

        if (fighterData == null)
        {
            Debug.LogError("SkillTreePanel: fighterData ����� null � Setup!");
            return;
        }

        SetupSkillButtons();
        Debug.Log($"������� ����� ������� ��� {stats.fighterName}, �������: {(fighterData.skills?.Count ?? 0)}, ������: {(skillButtons?.Length ?? 0)}");
    }

    private void SetupSkillButtons()
    {
        if (fighterData == null || fighterData.skills == null)
        {
            Debug.LogError("SkillTreePanel: fighterData ��� fighterData.skills ����� null!");
            return;
        }

        if (skillButtons == null || skillButtons.Length == 0)
        {
            Debug.LogError("SkillTreePanel: skillButtons �� �������� ��� ����!");
            return;
        }

        int buttonCount = Mathf.Min(fighterData.skills.Count, skillButtons.Length);
        if (fighterData.skills.Count > skillButtons.Length)
        {
            Debug.LogWarning($"���������� ������� ({fighterData.skills.Count}) ��������� ���������� ������ ({skillButtons.Length}). ������������ ������ ������ {skillButtons.Length} �������.");
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

        Debug.Log($"��������� {buttonCount} ������ ��� �������.");
    }

    private void OpenSkillCard(SkillData skill)
    {
        if (skillCardPrefab == null)
        {
            Debug.LogError("SkillTreePanel: SkillCardPrefab �� ��������!");
            return;
        }

        GameObject card = Instantiate(skillCardPrefab, transform);
        SkillCard cardScript = card.GetComponent<SkillCard>();
        if (cardScript != null)
        {
            ResourceController resourceCtrl = FindObjectOfType<ResourceController>();
            if (resourceCtrl == null)
            {
                Debug.LogError("SkillTreePanel: ResourceController �� ������!");
                Destroy(card);
                return;
            }
            if (panelManager == null)
            {
                Debug.LogError("SkillTreePanel: TrainingPanelManager �� �������!");
                Destroy(card);
                return;
            }
            cardScript.Setup(skill, fighterData, fighterStats, resourceCtrl, panelManager);
            Debug.Log($"������� �������� ������: {skill?.skillName ?? "Unknown"}");
        }
        else
        {
            Debug.LogError("SkillTreePanel: SkillCard ������ �� ������ �� �������!");
            Destroy(card);
        }
    }

    private void ClosePanel()
    {
        onCloseCallback?.Invoke();
        Destroy(gameObject);
        Debug.Log("������ ����� ������� �������");
    }
}
