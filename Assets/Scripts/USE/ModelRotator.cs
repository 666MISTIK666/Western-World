using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ModelRotator : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Transform modelTransform; // Трансформ модели, которую будем вращать
    public RawImage fighterModelView; // RawImage, для отслеживания указателя
    private bool isRotating = false; // Флаг, вращаем ли модель
    private float rotationSpeed = 1000f; // Скорость вращения

    void Update()
    {
        // Вращение происходит только когда мышь зажата
        if (isRotating && modelTransform != null)
        {
            float mouseX = Input.GetAxis("Mouse X");
            modelTransform.Rotate(0, -mouseX * rotationSpeed * Time.deltaTime, 0);
        }
    }

    // Начинаем вращение при нажатии
    public void OnPointerDown(PointerEventData eventData)
    {
        isRotating = true;
    }

    // Останавливаем вращение при отпускании кнопки мыши
    public void OnPointerUp(PointerEventData eventData)
    {
        isRotating = false;
    }
}
