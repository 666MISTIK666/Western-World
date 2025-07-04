using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] panels; // ������ �������, �������� � Inspector

    private BoxCollider2D[] buildingColliders; // ������������ 2D-���������� ������
    private bool wereCollidersDisabledLastFrame = false; // ��������� ����������� �� ������� �����

    void Awake()
    {
        // �������������� ��������� ������ ������
        UpdateBuildingColliders();
    }

    void Update()
    {
        // ���������, ������� �� ���� ���� ������
        bool anyPanelActive = AreAnyPanelsActive();

        // ��������� ���������� ������ ��� ��������� ��������� �������
        if (anyPanelActive != wereCollidersDisabledLastFrame)
        {
            UpdateBuildingColliders(); // ��������� ������ ������ ����� ������������� �����������
            UpdateColliders(anyPanelActive);
            wereCollidersDisabledLastFrame = anyPanelActive;
            Debug.Log($"���������� ������: {(anyPanelActive ? "���������" : "��������")}");
        }
    }

    // ��������� ������ ����������� ������
    private void UpdateBuildingColliders()
    {
        BuildingPrefabData[] buildings = FindObjectsOfType<BuildingPrefabData>();
        buildingColliders = new BoxCollider2D[buildings.Length];
        for (int i = 0; i < buildings.Length; i++)
        {
            buildingColliders[i] = buildings[i].GetComponent<BoxCollider2D>();
            if (buildingColliders[i] == null)
            {
                Debug.LogWarning($"������ {buildings[i].name} �� ����� BoxCollider2D!");
            }
        }
        Debug.Log($"������� ������ � BuildingPrefabData: {buildings.Length}, �����������: {buildingColliders.Length}");
    }

    // ��������� ��������� �����������
    private void UpdateColliders(bool disableColliders)
    {
        foreach (BoxCollider2D collider in buildingColliders)
        {
            if (collider != null)
            {
                collider.enabled = !disableColliders; // ���������, ���� ������ �������
            }
        }
    }

    // ����� ��� ��������������� ���������� (���� ����� ������� �������)
    public void RefreshBuildings()
    {
        UpdateBuildingColliders();
        UpdateColliders(AreAnyPanelsActive());
    }

    // �������� ��������� �������
    private bool AreAnyPanelsActive()
    {
        foreach (GameObject panel in panels)
        {
            if (panel != null && panel.activeSelf)
            {
                return true;
            }
        }
        return false;
    }
}