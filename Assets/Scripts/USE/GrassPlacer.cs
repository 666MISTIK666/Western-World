using UnityEngine;

public class GrassPlacer : MonoBehaviour
{
    public GameObject grassPrefab; // Префаб травы
    public float spacing = 0.5f; // Расстояние между травинками
    public float randomness = 0.1f; // Случайное смещение
    public float yPosition = -1f; // Высота (Y) для размещения (на уровне травы)

    // Размеры игровой зоны (подстрой под свои размеры)
    public float minX = -45f; // Левая граница (X)
    public float maxX = 45f;  // Правая граница (X)
    public float minZ = -30f; // Нижняя граница (Z)
    public float maxZ = 30f;  // Верхняя граница (Z)

    void Start()
    {
        PlaceGrass();
    }

    void PlaceGrass()
    {
        // Верхняя граница (Z = maxZ)
        for (float x = minX; x <= maxX; x += spacing)
        {
            Vector3 position = new Vector3(x + Random.Range(-randomness, randomness), yPosition, maxZ + Random.Range(-randomness, randomness));
            Instantiate(grassPrefab, position, grassPrefab.transform.rotation, transform);
        }

        // Нижняя граница (Z = minZ)
        for (float x = minX; x <= maxX; x += spacing)
        {
            Vector3 position = new Vector3(x + Random.Range(-randomness, randomness), yPosition, minZ + Random.Range(-randomness, randomness));
            Instantiate(grassPrefab, position, grassPrefab.transform.rotation, transform);
        }

        // Левая граница (X = minX)
        for (float z = minZ; z <= maxZ; z += spacing)
        {
            Vector3 position = new Vector3(minX + Random.Range(-randomness, randomness), yPosition, z + Random.Range(-randomness, randomness));
            Instantiate(grassPrefab, position, grassPrefab.transform.rotation, transform);
        }

        // Правая граница (X = maxX)
        for (float z = minZ; z <= maxZ; z += spacing)
        {
            Vector3 position = new Vector3(maxX + Random.Range(-randomness, randomness), yPosition, z + Random.Range(-randomness, randomness));
            Instantiate(grassPrefab, position, grassPrefab.transform.rotation, transform);
        }
    }
}