using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopData", menuName = "Shop/ShopData", order = 1)]
public class ShopData : ScriptableObject
{
    [System.Serializable]
    public class HouseData
    {
        public string name;              // �������� ���� (��������, "��� 1" ��� "������")
        public Sprite image;             // ����������� ��� �����
        public GameObject prefab;        // ������ ���� ��� �������������
        public BuildingPrefabData.BuildingType type; // ��� ���� (�����, ������� � �.�.)
    }

    public List<HouseData> houses = new List<HouseData>(); // ������ ���� �����
}