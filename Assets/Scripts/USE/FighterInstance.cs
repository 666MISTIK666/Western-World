using System;
using UnityEngine;

[RequireComponent(typeof(Collider2D))] // ��� Collider, � ����������� �� 2D/3D
public class FighterInstance : MonoBehaviour
{
    [Header("��� ����� (��������� ����, ��������� � key � FighterTypeManager)")]
    public string fighterName; // ��������, "������", "������" � �.�.

    private string instanceID;  // ���������� ID ����������� ����������

    [Header("HP ����� ����������")]
    public int maxHP;
    public int currentHP;

    private void OnEnable()
    {
        if (FighterTypeManager.Instance != null)
            FighterTypeManager.Instance.OnTypeHPChanged += HandleTypeHPChanged;
    }

    private void OnDisable()
    {
        if (FighterTypeManager.Instance != null)
            FighterTypeManager.Instance.OnTypeHPChanged -= HandleTypeHPChanged;
    }

    private void Awake()
    {
        // ���������� ���������� ID ��� ��������
        instanceID = Guid.NewGuid().ToString();
    }

    private void Start()
    {
        // �������� ������� ������� HP �� FighterTypeManager
        int baseHP = FighterTypeManager.Instance.GetBaseHP(fighterName);
        maxHP = baseHP;

        // �������, ���� �� ���������� currentHP � PlayerProgress
        int saved = PlayerProgress.Instance.GetSavedHPByID(instanceID);
        if (saved >= 0)
        {
            currentHP = Mathf.Min(saved, maxHP);
        }
        else
        {
            currentHP = maxHP;
            PlayerProgress.Instance.SaveCurrentHP(instanceID, currentHP);
        }
    }

    // ���� � ���� ����� ��������� baseHP (��������, ������ �����), �����������
    private void HandleTypeHPChanged(string typeName, int newBaseHP, int oldBaseHP)
    {
        if (typeName != fighterName) return;

        int delta = newBaseHP - oldBaseHP;
        maxHP = newBaseHP;
        currentHP = Mathf.Min(currentHP + delta, maxHP);
        PlayerProgress.Instance.SaveCurrentHP(instanceID, currentHP);
    }

    // ���������� �� ����� ��� ��������� �����
    public void TakeDamage(int amount)
    {
        currentHP = Mathf.Max(0, currentHP - amount);
        PlayerProgress.Instance.SaveCurrentHP(instanceID, currentHP);
        if (currentHP <= 0)
            Die();
    }

    // ���������� �� ����� ��� �������
    public void Heal(int amount)
    {
        currentHP = Mathf.Min(maxHP, currentHP + amount);
        PlayerProgress.Instance.SaveCurrentHP(instanceID, currentHP);
    }

    private void Die()
    {
        // ����� ����� �������� ������ ��������/�������� �������
        // ��������, Destroy(gameObject) ��� ��������� �������� ������.
        Destroy(gameObject);
    }
}
