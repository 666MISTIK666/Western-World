using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnergyUIController : MonoBehaviour
{
    public Image energyBar; // Шкала энергии
    public TextMeshProUGUI energyText; // Текст энергии
    public TextMeshProUGUI energyTimerText; // Текст таймера энергии
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
                energyTimerText.gameObject.SetActive(false); // Скрываем текст таймера
            }
            else
            {
                energyTimerText.gameObject.SetActive(true); // Показываем текст таймера
                float timeRemaining = resourceController.GetEnergyRegenTimeRemaining();
                int minutes = Mathf.FloorToInt(timeRemaining / 60f);
                int seconds = Mathf.FloorToInt(timeRemaining % 60f);
                energyTimerText.text = $"+1 через {minutes:D2}:{seconds:D2}";
            }
        }
    }
}