using UnityEngine;

public class DollarProducer : MonoBehaviour
{
    [Header("Production Settings")]
    [SerializeField] private float productionInterval = 5f; // Интервал производства в секундах
    [SerializeField] private int dollarsPerProduction = 10; // Количество долларов за цикл

    [Header("Icon Settings")]
    [SerializeField] private GameObject dollarIconPrefab; // Префаб иконки доллара
    [SerializeField] private Vector3 iconOffset = new Vector3(0, 5f, 0); // Смещение иконки
    [SerializeField] private Vector3 iconScaleAdjustment = new Vector3(0.3f, 0.3f, 0.3f); // Индивидуальная корректировка масштаба

    private float timer = 0f;
    private bool dollarsReady = false;
    private GameObject currentDollarIcon;

    private ResourceController resourceController;
    private LongPressRelocator longPressRelocator;

    void Start()
    {
        resourceController = FindObjectOfType<ResourceController>();
        if (resourceController == null)
        {
            Debug.LogError("ResourceController не найден!");
        }

        longPressRelocator = GetComponent<LongPressRelocator>();
        if (longPressRelocator == null)
        {
            longPressRelocator = gameObject.AddComponent<LongPressRelocator>();
            Debug.Log("LongPressRelocator добавлен автоматически.");
        }

        timer = 0f;
        dollarsReady = false;
    }

    void Update()
    {
        if (!dollarsReady)
        {
            timer += Time.deltaTime;
            if (timer >= productionInterval)
            {
                dollarsReady = true;
                timer = productionInterval;
                ShowDollarIcon();
            }
        }

        if (currentDollarIcon != null)
        {
            currentDollarIcon.transform.localPosition = iconOffset;
        }
    }

    private void ShowDollarIcon()
    {
        if (dollarIconPrefab != null)
        {
            Quaternion rotation = Quaternion.Euler(0f, -45f, 0f);
            currentDollarIcon = Instantiate(dollarIconPrefab, transform.position + iconOffset, rotation, transform);

            SpriteRenderer prefabRenderer = dollarIconPrefab.GetComponent<SpriteRenderer>();
            SpriteRenderer iconRenderer = currentDollarIcon.GetComponent<SpriteRenderer>();
            if (prefabRenderer != null && iconRenderer != null)
            {
                iconRenderer.sortingLayerName = prefabRenderer.sortingLayerName;
                iconRenderer.sortingOrder = prefabRenderer.sortingOrder;
            }
            else
            {
                Debug.LogWarning("SpriteRenderer не найден на dollarIconPrefab или созданной иконке!");
            }

            // Применяем индивидуальную корректировку масштаба
            currentDollarIcon.transform.localScale = iconScaleAdjustment;
        }
        else
        {
            Debug.LogError("DollarIconPrefab не назначен!");
        }
    }

    void OnMouseDown()
    {
        if (dollarsReady && resourceController != null)
        {
            if (resourceController.GetResource("Energy") >= 1)
            {
                resourceController.AddResource("Energy", -1);
                resourceController.AddResource("Dollars", dollarsPerProduction);
                Debug.Log($"Доллары собраны: +{dollarsPerProduction}");
                CollectDollars();
            }
            else
            {
                Debug.Log("Недостаточно энергии!");
            }
        }
    }

    private void CollectDollars()
    {
        dollarsReady = false;
        timer = 0f;
        if (currentDollarIcon != null)
        {
            Destroy(currentDollarIcon);
            currentDollarIcon = null;
        }
    }

    public bool AreDollarsReady()
    {
        return dollarsReady;
    }
}