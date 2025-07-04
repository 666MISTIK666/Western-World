using UnityEngine;

[CreateAssetMenu(menuName = "WW/FighterDefinition")]
public class FighterDefinition : ScriptableObject
{
    public string fighterID;       // ���������� ID, ����. "Cowboy_Horse"
    public string displayName;     // �������� ��� UI: "������ �� ������"
    public GameObject prefab;      // ��� ������ �����
    public int baseMaxHP;          // ��������� ����� HP
}