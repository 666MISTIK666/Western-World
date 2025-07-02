using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopData", menuName = "Shop/ShopData", order = 1)]
public class ShopData : ScriptableObject
{
    [System.Serializable]
    public class HouseData
    {
        public string name;              // Название дома (например, "Дом 1" или "Бункер")
        public Sprite image;             // Изображение для рамки
        public GameObject prefab;        // Префаб дома для строительства
        public BuildingPrefabData.BuildingType type; // Тип дома (жилой, военный и т.д.)
    }

    public List<HouseData> houses = new List<HouseData>(); // Список всех домов
}