using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class ResourceDisplay
{
    [Tooltip("��� ������� ������ ��������� � ������ � ResourceController")]
    public string resourceName;             // ��� ������� ��� �������������
    public TextMeshProUGUI resourceText;    // UI-������� ��� ����������� ����������
}

public class ResourceUIController : MonoBehaviour
{
    // ������ ��������� UI ��� ����������� ��������; ����������/������������ � ����������
    public List<ResourceDisplay> resourceDisplays;

    // ��������� ����� ��� ������� ������� �� ������ �� ResourceController
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
