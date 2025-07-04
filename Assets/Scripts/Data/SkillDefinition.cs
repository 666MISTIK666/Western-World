using UnityEngine;

[CreateAssetMenu(menuName = "WW/SkillDefinition")]
public class SkillDefinition : ScriptableObject
{
    public string skillID;             // ���������� ID �����, ����. "HP_UP"
    public FighterDefinition fighter;  // ������ �� ����������� �����
    public int bonusMaxHP;             // �������� � maxHP
    public int cost;                   // ��������� � �������
}