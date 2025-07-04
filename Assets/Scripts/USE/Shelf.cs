using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shelf : MonoBehaviour
{
    public Image frameImage;         // ����� ��� �����������
    public Button button;            // ������
    public TextMeshProUGUI buttonText; // ����� �� ������

    public void SetHouseData(ShopData.HouseData houseData)
    {
        if (houseData != null)
        {
            if (houseData.image != null)
                frameImage.sprite = houseData.image; // ������������� ��������
            else
                Debug.LogError("houseData.image is null");

            if (houseData.name != null)
                buttonText.text = houseData.name;    // ������������� ��������
            else
                Debug.LogError("houseData.name is null");

            button.onClick.RemoveAllListeners(); // ������� ������ �������� ������
            button.onClick.AddListener(() => FindObjectOfType<ShopManager>().OpenHouseInfo(houseData.prefab, houseData.prefab.GetComponent<BuildingPrefabData>()));
        }
        else
        {
            frameImage.sprite = null; // ������� �����
            buttonText.text = "";     // ������� �����
            button.onClick.RemoveAllListeners(); // ������� ��������
        }
    }
}