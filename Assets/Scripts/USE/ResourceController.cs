using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ResourceEntry
{
    public string resourceName;
    public int amount;
    public int maxAmount;
}

public class ResourceController : MonoBehaviour
{
    public List<ResourceEntry> resourceEntries = new List<ResourceEntry>
    {
        new ResourceEntry { resourceName = "Gold", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Silver", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Copper", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Iron", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Mercury", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Aluminum", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Diamonds", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Wood", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Dollars", amount = 1000, maxAmount = 0 },
        new ResourceEntry { resourceName = "Energy", amount = 100, maxAmount = 100 }
    };

    public ResourceUIController uiController;
    public EnergyUIController energyUIController;

    private float energyRegenTime = 10f; // Время регенерации энергии в секундах
    private float energyTimer = 0f;

    void Start()
    {
        UpdateUI();
        energyTimer = energyRegenTime;
    }

    void Update()
    {
        ResourceEntry energyEntry = resourceEntries.Find(e => e.resourceName == "Energy");
        if (energyEntry != null && energyEntry.amount < energyEntry.maxAmount)
        {
            energyTimer -= Time.deltaTime;
            if (energyTimer <= 0f)
            {
                AddResource("Energy", 1);
                energyTimer = energyRegenTime;
            }
        }
    }

    public void AddResource(string resourceName, int amount)
    {
        ResourceEntry entry = resourceEntries.Find(e => e.resourceName == resourceName);
        if (entry != null)
        {
            entry.amount += amount;
            if (entry.maxAmount > 0)
                entry.amount = Mathf.Clamp(entry.amount, 0, entry.maxAmount);
        }
        else
        {
            ResourceEntry newEntry = new ResourceEntry { resourceName = resourceName, amount = amount, maxAmount = 0 };
            resourceEntries.Add(newEntry);
        }
        UpdateUI(); // Обновляем UI для всех ресурсов
        if (energyUIController != null)
            energyUIController.UpdateEnergyUI(); // Всегда обновляем UI энергии
    }

    public int GetResource(string resourceName)
    {
        ResourceEntry entry = resourceEntries.Find(e => e.resourceName == resourceName);
        return entry != null ? entry.amount : 0;
    }

    public float GetEnergyRegenTimeRemaining()
    {
        return energyTimer;
    }

    public bool IsEnergyFull()
    {
        ResourceEntry energyEntry = resourceEntries.Find(e => e.resourceName == "Energy");
        return energyEntry != null && energyEntry.amount >= energyEntry.maxAmount;
    }

    private void UpdateUI()
    {
        if (uiController != null)
            uiController.UpdateResourceUI(resourceEntries);
    }
}