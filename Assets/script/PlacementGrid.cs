using UnityEngine;

public class PlacementGrid : MonoBehaviour
{
    public Transform indicatorsContainer;
    private GridManager gridManager;

    void Awake()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (indicatorsContainer == null)
        {
            GameObject temp = new GameObject("TempIndicators");
            temp.transform.SetParent(transform);
            indicatorsContainer = temp.transform;
        }
    }

    public void ShowCells(Vector2Int gridPosition, Vector2Int buildingSize)
    {
        HideCells();

        for (int x = 0; x < buildingSize.x; x++)
        {
            for (int y = 0; y < buildingSize.y; y++)
            {
                Vector2Int cellPos = gridPosition + new Vector2Int(x, y);
                if (cellPos.x >= 0 && cellPos.x < gridManager.width && cellPos.y >= 0 && cellPos.y < gridManager.height)
                {
                    GameObject cellObj = new GameObject("CellIndicator_" + x + "_" + y);
                    cellObj.transform.SetParent(indicatorsContainer, false);

                    LineRenderer line = cellObj.AddComponent<LineRenderer>();
                    Material cellMat = new Material(Shader.Find("Unlit/Color"));
                    Color cellColor = gridManager.IsCellOccupied(cellPos) ? new Color(1, 0, 0, 0.5f) : new Color(0, 1, 0, 0.5f);
                    cellMat.color = cellColor;
                    line.material = cellMat;

                    line.startWidth = 0.5f; // Уменьшено для большей чёткости
                    line.endWidth = 0.5f;
                    line.positionCount = 5;

                    float cellSize = gridManager.cellSize;
                    float halfCellSize = cellSize / 2f;
                    Vector3 worldPos = gridManager.GridToWorld(cellPos, 0f);
                    Vector3 offset = new Vector3(0, 0.02f, 0);

                    Vector3 bottomLeft = worldPos + offset + new Vector3(-halfCellSize, 0, -halfCellSize);
                    Vector3 bottomRight = worldPos + offset + new Vector3(halfCellSize, 0, -halfCellSize);
                    Vector3 topRight = worldPos + offset + new Vector3(halfCellSize, 0, halfCellSize);
                    Vector3 topLeft = worldPos + offset + new Vector3(-halfCellSize, 0, halfCellSize);

                    line.SetPosition(0, bottomLeft);
                    line.SetPosition(1, bottomRight);
                    line.SetPosition(2, topRight);
                    line.SetPosition(3, topLeft);
                    line.SetPosition(4, bottomLeft);
                }
            }
        }
    }

    public void HideCells()
    {
        if (indicatorsContainer != null)
        {
            foreach (Transform child in indicatorsContainer)
            {
                if (child != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    public void RefreshCells(Vector2Int startPos, Vector2Int size)
    {
        ShowCells(startPos, size); // Переиспользуем ShowCells для обновления
    }
}