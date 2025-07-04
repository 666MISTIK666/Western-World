using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FighterTypeData
{
    public string fighterName; // ��� ���� �����, �������� "������"
    public GameObject prefab;  // ������ �����
    public List<SkillData> skills; // ��������� ������ ��� ����� ����
    public List<string> purchasedSkills; // ��������� ������ ��� ����� ����
}