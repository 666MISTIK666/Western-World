using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(SortingGroup))]
public class IsometricSorting : MonoBehaviour
{
    private SortingGroup sortingGroup;
    [SerializeField] private float sortingMultiplier = 10f; // Уменьшил множитель для больших координат
    private Vector3 lastPosition;

    void Start()
    {
        sortingGroup = GetComponent<SortingGroup>();
        if (sortingGroup == null)
        {
            Debug.LogError("SortingGroup не найден на объекте: " + gameObject.name);
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
            // Получаем мировую позицию объекта
            Vector3 worldPos = transform.position;

            // Формула для изометрии: меньше Z и больше X = ближе к игроку
            float sortValue = (-worldPos.z + worldPos.x);
            int order = Mathf.RoundToInt(sortValue * sortingMultiplier);
            // Убрали жёсткий Clamp, чтобы сортировка работала для больших координат
            sortingGroup.sortingOrder = order;
        }
    }
}