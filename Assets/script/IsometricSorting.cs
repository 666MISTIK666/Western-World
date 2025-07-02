using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class IsometricSorting : MonoBehaviour
{
    private SortingGroup sortingGroup;
    [SerializeField] private float sortingMultiplier = 10f; // �������� ��������� ��� ������� ���������
    private Vector3 lastPosition;

    void Start()
    {
        sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup == null)
        {
            Debug.LogError("SortingGroup �� ������ �� �������: " + gameObject.name);
        }

        lastPosition = transform.position;
        UpdateSortingOrder();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastPosition) > 0.01f)
        {
            UpdateSortingOrder();
            lastPosition = transform.position;
        }
    }

    private void UpdateSortingOrder()
    {
        if (sortingGroup != null)
        {
            // �������� ������� ������� �������
            Vector3 worldPos = transform.position;

            // ������� ��� ���������: ������ Z � ������ X = ����� � ������
            float sortValue = (-worldPos.z + worldPos.x);
            int order = Mathf.RoundToInt(sortValue * sortingMultiplier);
            // ������ ������ Clamp, ����� ���������� �������� ��� ������� ���������
            sortingGroup.sortingOrder = order;
        }
    }
}