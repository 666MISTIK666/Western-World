using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ResourceDisplay
{
    [Tooltip("Имя ресурса должно совпадать с именем в ResourceController")]
    public string resourceName;             // Имя ресурса для сопоставления
    public TextMeshProUGUI resourceText;    // UI-элемент для отображения количества
}

public class ResourceUIController : MonoBehaviour
{
    // Список элементов UI для отображения ресурсов; добавляйте/редактируйте в инспекторе
    public List<ResourceDisplay> resourceDisplays;

    // Обновляем текст для каждого ресурса по данным из ResourceController
    public void UpdateResourceUI(List<ResourceEntry> resourceEntries)
    {
        foreach (ResourceDisplay display in resourceDisplays)
        {
            ResourceEntry entry = resourceEntries.Find(e => e.resourceName == display.resourceName);
            if (entry != null)
                display.resourceText.text = entry.amount.ToString();
            else
                display.resourceText.text = "0";
        }
    }
}
