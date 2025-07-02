using UnityEngine;

public class ArrowAnimation : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.2f; // ��������� �������� �� Y (� ��������)
    [SerializeField] private float speed = 2f; // �������� �������� (��� ����, ��� �������)

    private Vector3 initialLocalPosition; // ��������� ��������� ������� �������
    private bool isAnimating = false;

    void Awake()
    {
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        if (!isAnimating) return;

        // ���������� ����� ��� �������� ��������� �� Y
        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = initialLocalPosition + new Vector3(0, yOffset, 0);
    }

    // ����� ��� ���������/���������� ��������
    public void SetAnimating(bool animate)
    {
        isAnimating = animate;
        if (!animate)
        {
            // ���������� ������� � ��������� ������� ��� ���������� ��������
            transform.localPosition = initialLocalPosition;
        }
    }
}