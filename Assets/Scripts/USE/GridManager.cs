using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public int width = 90;
    public int height = 60;
    public float cellSize = 1f;
    public Material lineMaterial;
    private LineRenderer[] horizontalLines;
    private LineRenderer[] verticalLines;
    public bool[,] occupancy;
    public Dictionary<GameObject, List<Vector2Int>> objectCells;

    void Awake()
    {
        objectCells = new Dictionary<GameObject, List<Vector2Int>>();
        occupancy = new bool[width, height];
        Debug.Log("GridManager: Initialized occupancy and objectCells in Awake");
    }

    void Start()
    {
        if (Application.isPlaying)
        {
            DrawGrid();
        }
    }

    void OnValidate()
    {
        if (width < 1) width = 1;
        if (height < 1) height = 1;
        if (cellSize <= 0) cellSize = 1f;
    }

    private void DrawGrid()
    {
        if (lineMaterial == null)
        {
            Debug.LogWarning("GridManager: Line material is not assigned!");
            return;
        }
        ClearGrid();

        horizontalLines = new LineRenderer[height + 1];
        verticalLines = new LineRenderer[width + 1];

        float minX = -width * cellSize / 2f;
        float maxX = width * cellSize / 2f;
        float minZ = -height * cellSize / 2f;
        float maxZ = height * cellSize / 2f;

        for (int z = 0; z <= height; z++)
        {
            GameObject lineObj = new GameObject("GridLine_H_" + z);
            lineObj.transform.SetParent(transform);
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = 0.5f;
            line.endWidth = 0.5f;
            line.positionCount = 2;
            float zPos = minZ + z * cellSize;
            line.SetPosition(0, new Vector3(minX, 0.01f, zPos));
            line.SetPosition(1, new Vector3(maxX, 0.01f, zPos));
            horizontalLines[z] = line;
        }

        for (int x = 0; x <= width; x++)
        {
            GameObject lineObj = new GameObject("GridLine_V_" + x);
            lineObj.transform.SetParent(transform);
            LineRenderer line = lineObj.AddComponent<LineRenderer>();
            line.material = lineMaterial;
            line.startWidth = 0.5f;
            line.endWidth = 0.5f;
            line.positionCount = 2;
            float xPos = minX + x * cellSize;
            line.SetPosition(0, new Vector3(xPos, 0.01f, minZ));
            line.SetPosition(1, new Vector3(xPos, 0.01f, maxZ));
            verticalLines[x] = line;
        }
    }

    private void ClearGrid()
    {
        if (horizontalLines != null)
        {
            foreach (var line in horizontalLines)
            {
                if (line != null)
                {
                    if (Application.isPlaying) Destroy(line.gameObject);
                    else DestroyImmediate(line.gameObject);
                }
            }
            horizontalLines = null;
        }
        if (verticalLines != null)
        {
            foreach (var line in verticalLines)
            {
                if (line != null)
                {
                    if (Application.isPlaying) Destroy(line.gameObject);
                    else DestroyImmediate(line.gameObject);
                }
            }
            verticalLines = null;
        }
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x + width * cellSize / 2f) / cellSize);
        int z = Mathf.FloorToInt((worldPosition.z + height * cellSize / 2f) / cellSize);
        x = Mathf.Clamp(x, 0, width - 1);
        z = Mathf.Clamp(z, 0, height - 1);
        return new Vector2Int(x, z);
    }

    public Vector3 GridToWorld(Vector2Int gridPosition, float yOffset = 0f)
    {
        float x = -width * cellSize / 2f + gridPosition.x * cellSize + cellSize / 2f;
        float z = -height * cellSize / 2f + gridPosition.y * cellSize + cellSize / 2f;
        return new Vector3(x, yOffset, z);
    }

    public bool IsAreaFree(Vector2Int startPos, Vector2Int areaSize)
    {
        for (int x = 0; x < areaSize.x; x++)
        {
            for (int y = 0; y < areaSize.y; y++)
            {
                Vector2Int pos = startPos + new Vector2Int(x, y);
                if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
                    return false;
                if (occupancy[pos.x, pos.y])
                    return false;
            }
        }
        return true;
    }

    public void MarkAreaOccupied(Vector2Int startPos, Vector2Int areaSize, GameObject obj)
    {
        if (obj == null)
        {
            Debug.LogError("GridManager: MarkAreaOccupied: obj is null!");
            return;
        }
        if (objectCells == null)
        {
            Debug.LogError("GridManager: MarkAreaOccupied: objectCells not initialized!");
            objectCells = new Dictionary<GameObject, List<Vector2Int>>();
        }

        // Освободить предыдущие клетки, если объект уже зарегистрирован
        if (objectCells.ContainsKey(obj))
        {
            MarkAreaFree(obj);
        }

        List<Vector2Int> cells = new List<Vector2Int>();
        for (int x = 0; x < areaSize.x; x++)
        {
            for (int y = 0; y < areaSize.y; y++)
            {
                Vector2Int pos = startPos + new Vector2Int(x, y);
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                {
                    occupancy[pos.x, pos.y] = true;
                    cells.Add(pos);
                }
            }
        }

        objectCells[obj] = cells;
        Debug.Log($"GridManager: MarkAreaOccupied: Registered {obj.name} at {startPos} with {cells.Count} cells");
    }

    public void MarkAreaFree(GameObject obj)
    {
        if (obj == null || objectCells == null)
        {
            Debug.LogWarning("GridManager: MarkAreaFree: obj or objectCells is null!");
            return;
        }

        if (objectCells.ContainsKey(obj))
        {
            foreach (Vector2Int pos in objectCells[obj])
            {
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                {
                    occupancy[pos.x, pos.y] = false;
                }
            }
            objectCells.Remove(obj);
            Debug.Log($"GridManager: MarkAreaFree: Freed cells for {obj.name}");
        }
    }

    public void ForceClearArea(Vector2Int startPos, Vector2Int areaSize)
    {
        if (objectCells == null)
        {
            objectCells = new Dictionary<GameObject, List<Vector2Int>>();
        }

        for (int x = 0; x < areaSize.x; x++)
        {
            for (int y = 0; y < areaSize.y; y++)
            {
                Vector2Int pos = startPos + new Vector2Int(x, y);
                if (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height)
                {
                    occupancy[pos.x, pos.y] = false;
                }
            }
        }

        List<GameObject> keysToRemove = new List<GameObject>();
        foreach (var pair in objectCells)
        {
            if (pair.Key == null)
            {
                keysToRemove.Add(pair.Key);
                continue;
            }
            foreach (var cell in pair.Value)
            {
                if (cell.x >= startPos.x && cell.x < startPos.x + areaSize.x &&
                    cell.y >= startPos.y && cell.y < startPos.y + areaSize.y)
                {
                    keysToRemove.Add(pair.Key);
                    break;
                }
            }
        }
        foreach (var key in keysToRemove)
        {
            objectCells.Remove(key);
            if (key != null)
            {
                Debug.Log($"GridManager: ForceClearArea: Removed entry for {key.name}");
            }
        }
    }

    public bool IsCellOccupied(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= width || pos.y < 0 || pos.y >= height)
            return false;
        return occupancy[pos.x, pos.y];
    }

    void OnDestroy()
    {
        ClearGrid();
    }

    void OnDisable()
    {
        if (!Application.isPlaying)
        {
            ClearGrid();
        }
    }
}