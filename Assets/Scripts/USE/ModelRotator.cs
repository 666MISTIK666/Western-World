using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModelRotator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Transform modelTransform; // ��������� ������, ������� ����� �������
    public RawImage fighterModelView; // RawImage, ��� ������������ ���������
    private bool isRotating = false; // ����, ������� �� ������
    private float rotationSpeed = 1000f; // �������� ��������

    void Update()
    {
        // �������� ���������� ������ ����� ���� ������
        if (isRotating && modelTransform != null)
        {
            float mouseX = Input.GetAxis("Mouse X");
            modelTransform.Rotate(0, -mouseX * rotationSpeed * Time.deltaTime, 0);
        }
    }

    // �������� �������� ��� �������
    public void OnPointerDown(PointerEventData eventData)
    {
        isRotating = true;
    }

    // ������������� �������� ��� ���������� ������ ����
    public void OnPointerUp(PointerEventData eventData)
    {
        isRotating = false;
    }
}
