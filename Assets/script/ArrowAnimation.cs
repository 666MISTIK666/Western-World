using UnityEngine;

public class ArrowAnimation : MonoBehaviour
{
    [SerializeField] private float amplitude = 0.2f; // Амплитуда движения по Y (в единицах)
    [SerializeField] private float speed = 2f; // Скорость анимации (чем выше, тем быстрее)

    private Vector3 initialLocalPosition; // Начальная локальная позиция стрелки
    private bool isAnimating = false;

    void Awake()
    {
        initialLocalPosition = transform.localPosition;
    }

    void Update()
    {
        if (!isAnimating) return;

        // Используем синус для плавного колебания по Y
        float yOffset = Mathf.Sin(Time.time * speed) * amplitude;
        transform.localPosition = initialLocalPosition + new Vector3(0, yOffset, 0);
    }

    // Метод для включения/выключения анимации
    public void SetAnimating(bool animate)
    {
        isAnimating = animate;
        if (!animate)
        {
            // Возвращаем стрелку в начальную позицию при отключении анимации
            transform.localPosition = initialLocalPosition;
        }
    }
}