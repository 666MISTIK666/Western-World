using UnityEngine;

public class VillageFighter : MonoBehaviour
{
    public string ID { get; private set; }
    public int CurrentHP { get; private set; }
    public int MaxHP { get; private set; }

    /// <summary>
    /// ������������� ����������� ������������� ����� � �������
    /// </summary>
    /// <param name="id">���������� ID ����� (fighterID)</param>
    /// <param name="currentHP">������� HP</param>
    /// <param name="maxHP">������������ HP</param>
    public void Init(string id, int currentHP, int maxHP)
    {
        ID = id;
        CurrentHP = currentHP;
        MaxHP = maxHP;
        // TODO: �������� UI (������, ������ HP � �.�.)
    }
}