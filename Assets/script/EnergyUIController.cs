using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyUIController : MonoBehaviour
{
    public Image energyBar; // ����� �������
    public TextMeshProUGUI energyText; // ����� �������
    public TextMeshProUGUI energyTimerText; // ����� ������� �������
    private ResourceController resourceController;
    private float targetFillAmount;

    void Start()
    {
        resourceController = FindObjectOfType<ResourceController>();
        if (resourceController == null)
        {
            Debug.LogError("ResourceController not found!");
            return;
        }
        UpdateEnergyUI();
    }

    public void UpdateEnergyUI()
    {
        ResourceEntry energyEntry = resourceController.resourceEntries.Find(e => e.resourceName == "Energy");
        if (energyEntry != null)
        {
            targetFillAmount = (float)energyEntry.amount / energyEntry.maxAmount;
            if (energyText != null)
                energyText.text = energyEntry.amount + " / " + energyEntry.maxAmount;
        }
    }

    void Update()
    {
        if (energyBar != null)
        {
            energyBar.fillAmount = Mathf.Lerp(energyBar.fillAmount, targetFillAmount, Time.deltaTime * 5f);
        }

        if (energyTimerText != null && resourceController != null)
        {
            if (resourceController.IsEnergyFull())
            {
                energyTimerText.gameObject.SetActive(false); // �������� ����� �������
            }
            else
            {
                energyTimerText.gameObject.SetActive(true); // ���������� ����� �������
                float timeRemaining = resourceController.GetEnergyRegenTimeRemaining();
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                energyTimerText.text = $"+1 ����� {minutes:D2}:{seconds:D2}";
            }
        }
    }
}