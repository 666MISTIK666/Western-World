using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Shelf : MonoBehaviour
{
    public Image frameImage;         // Рамка для изображения
    public Button button;            // Кнопка
    public TextMeshProUGUI buttonText; // Текст на кнопке

    public void SetHouseData(ShopData.HouseData houseData)
    {
        if (houseData != null)
        {
            if (houseData.image != null)
                frameImage.sprite = houseData.image; // Устанавливаем картинку
            else
                Debug.LogError("houseData.image is null");

            if (houseData.name != null)
                buttonText.text = houseData.name;    // Устанавливаем название
            else
                Debug.LogError("houseData.name is null");

            button.onClick.RemoveAllListeners(); // Удаляем старые действия кнопки
            button.onClick.AddListener(() => FindObjectOfType<ShopManager>().OpenHouseInfo(houseData.prefab, houseData.prefab.GetComponent<BuildingPrefabData>()));
        }
        else
        {
            frameImage.sprite = null; // Очищаем рамку
            buttonText.text = "";     // Очищаем текст
            button.onClick.RemoveAllListeners(); // Удаляем действия
        }
    }
}