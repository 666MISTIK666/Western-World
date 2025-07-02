using UnityEngine;

public class GrassPlacer : MonoBehaviour
{
    public GameObject grassPrefab; // ������ �����
    public float spacing = 0.5f; // ���������� ����� ����������
    public float randomness = 0.1f; // ��������� ��������
    public float yPosition = -1f; // ������ (Y) ��� ���������� (�� ������ �����)

    // ������� ������� ���� (�������� ��� ���� �������)
    public float minX = -45f; // ����� ������� (X)
    public float maxX = 45f;  // ������ ������� (X)
    public float minZ = -30f; // ������ ������� (Z)
    public float maxZ = 30f;  // ������� ������� (Z)

    void Start()
    {
        PlaceGrass();
    }

    void PlaceGrass()
    {
        // ������� ������� (Z = maxZ)
        for (float x = minX; x <= maxX; x += spacing)
        {
            Vector3 position = new Vector3(x + Random.Range(-randomness, randomness), yPosition, maxZ + Random.Range(-randomness, randomness));
            Instantiate(grassPrefab, position, grassPrefab.transform.rotation, transform);
        }

        // ������ ������� (Z = minZ)
        for (float x = minX; x <= maxX; x += spacing)
        {
            Vector3 position = new Vector3(x + Random.Range(-randomness, randomness), yPosition, minZ + Random.Range(-randomness, randomness));
            Instantiate(grassPrefab, position, grassPrefab.transform.rotation, transform);
        }

        // ����� ������� (X = minX)
        for (float z = minZ; z <= maxZ; z += spacing)
        {
            Vector3 position = new Vector3(minX + Random.Range(-randomness, randomness), yPosition, z + Random.Range(-randomness, randomness));
            Instantiate(grassPrefab, position, grassPrefab.transform.rotation, transform);
        }

        // ������ ������� (X = maxX)
        for (float z = minZ; z <= maxZ; z += spacing)
        {
            Vector3 position = new Vector3(maxX + Random.Range(-randomness, randomness), yPosition, z + Random.Range(-randomness, randomness));
            Instantiate(grassPrefab, position, grassPrefab.transform.rotation, transform);
        }
    }
}