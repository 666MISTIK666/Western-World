using UnityEngine;

public class Building : MonoBehaviour
{
    public Vector2Int size = new Vector2Int(3, 3); // ������ ���� � ������� (3x3)
    private GridManager gridManager;
    private SpriteRenderer spriteRenderer;
    private Vector2Int gridPosition; // ������� �� �����

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // ���� � ��������� ������ (����� ������ ����)
        gridPosition = gridManager.WorldToGrid(transform.position);
        Vector3 basePosition = gridManager.GridToWorld(gridPosition, transform.position.y);
        Quaternion rotation = transform.rotation;
        Vector3 offset = rotation * new Vector3(-gridManager.cellSize * (size.x - 1) / 2f, 0, -gridManager.cellSize * (size.y - 1) / 2f);
        transform.position = basePosition + offset;

        // ������������ ������ ��� ������ � �������
        float scaleFactor = size.x;
        transform.localScale = new Vector3(scaleFactor, scaleFactor, 1);

        // ��������� ��� ��������� BoxCollider2D
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null) collider = gameObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(size.x * gridManager.cellSize, size.y * gridManager.cellSize);
    }

    void LateUpdate()
    {
        // ���������� �� Z-������� (��� ���������)
        spriteRenderer.sortingOrder = Mathf.RoundToInt(-transform.position.z * 100);
    }

    // �������� ���������� ������
    public Vector2Int[] GetOccupiedCells()
    {
        Vector2Int[] cells = new Vector2Int[size.x * size.y];
        int index = 0;
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                cells[index] = gridPosition + new Vector2Int(x, y);
                index++;
            }
        }
        return cells;
    }
}