using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    public static LoadingScreen Instance { get; private set; }

    [Header("UI Elements")]
    [SerializeField] private GameObject loadingScreenCanvas;  // Canvas с UI загрузки
    [SerializeField] private RectTransform bulletContainer;   // Контейнер для пуль
    [SerializeField] private Sprite bulletSprite;             // Спрайт пули
    [SerializeField] private Image loadingBar;                // Прогресс-бар (Image Type = Filled)
    [SerializeField] private TextMeshProUGUI loadingText;     // Текст процентов

    [Header("Settings")]
    [SerializeField] private int maxBullets = 15;             // Максимум пуль
    [SerializeField] private float minLoadTime = 2f;          // Минимальное время показа UI
    [SerializeField] private float initialAnimationTime = 5f; // Время начальной анимации

    private List<Image> bulletImages = new List<Image>();
    private float bulletWidth;
    private string menuSceneName;
    private bool isLoading = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            menuSceneName = SceneManager.GetActiveScene().name;
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        // Скрываем UI при старте
        loadingScreenCanvas.SetActive(false);

        // Рассчитываем ширину пули
        float totalWidth = bulletContainer.rect.width;
        float spacing = 5f;
        bulletWidth = (totalWidth - spacing * (maxBullets - 1)) / maxBullets;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("Загрузка уже выполняется!");
            return;
        }
        StartCoroutine(ShowAndLoad(sceneName));
    }

    private IEnumerator ShowAndLoad(string sceneName)
    {
        isLoading = true;
        Debug.Log("ShowAndLoad: Начало загрузки");

        // 1. Показать UI
        loadingScreenCanvas.SetActive(true);
        Debug.Log("ShowAndLoad: UI включено");

        // 2. Дождаться конца кадра для отрисовки
        yield return new WaitForEndOfFrame();

        // 3. Сбросить UI
        foreach (var img in bulletImages) if (img) Destroy(img.gameObject);
        bulletImages.Clear();
        if (loadingBar != null) loadingBar.fillAmount = 0f;
        if (loadingText != null) loadingText.text = "0%";
        Debug.Log("ShowAndLoad: UI сброшено");

        // 4. Начальная анимация
        float startTime = Time.time;
        while (Time.time - startTime < initialAnimationTime)
        {
            float prog = (Time.time - startTime) / initialAnimationTime;
            UpdateUI(prog * 0.3f); // До 30% для начальной анимации
            yield return null;
        }

        // 5. Начать асинхронную загрузку сцены
        Debug.Log("ShowAndLoad: Начало загрузки сцены");
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        // 6. Обновлять UI во время загрузки
        startTime = Time.time;
        while (op.progress < 0.9f || Time.time - startTime < minLoadTime)
        {
            float elapsed = Time.time - startTime;
            float prog = (elapsed < minLoadTime)
                ? Mathf.Lerp(0.3f, 1f, elapsed / minLoadTime)
                : Mathf.Lerp(0.3f, 1f, op.progress / 0.9f);

            UpdateUI(prog);
            yield return null;
        }

        // 7. Завершить UI на 100%
        UpdateUI(1f);
        Debug.Log("ShowAndLoad: UI завершено на 100%");

        // 8. Дождаться minLoadTime
        float remain = minLoadTime - (Time.time - startTime);
        if (remain > 0f) yield return new WaitForSeconds(remain);

        // 9. Активировать сцену
        Debug.Log("ShowAndLoad: Активация сцены");
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // 10. Добавить задержку перед выгрузкой меню
        yield return new WaitForSeconds(0.1f); // Дать Unity завершить артефакты

        // 11. Скрыть UI
        loadingScreenCanvas.SetActive(false);
        Debug.Log("ShowAndLoad: UI скрыто");
        isLoading = false;
    }

    private void UpdateUI(float prog)
    {
        // Обновить прогресс-бар и текст
        if (loadingBar != null) loadingBar.fillAmount = prog;
        if (loadingText != null) loadingText.text = $"{Mathf.FloorToInt(prog * 100)}%";

        // Спавн пуль
        int needed = Mathf.FloorToInt(prog * maxBullets);
        while (bulletImages.Count < needed)
        {
            var go = new GameObject("Bullet");
            go.transform.SetParent(bulletContainer, false);
            var img = go.AddComponent<Image>();
            img.sprite = bulletSprite;

            var rt = img.rectTransform;
            rt.sizeDelta = new Vector2(bulletWidth, bulletContainer.rect.height);
            rt.anchorMin = rt.anchorMax = new Vector2(0, 0.5f);
            float x = (bulletWidth + 5f) * bulletImages.Count;
            rt.anchoredPosition = new Vector2(x + bulletWidth / 2f, 0);

            bulletImages.Add(img);
        }
    }
}