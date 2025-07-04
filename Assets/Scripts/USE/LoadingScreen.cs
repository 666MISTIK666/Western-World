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
    [SerializeField] private GameObject loadingScreenCanvas;  // Canvas � UI ��������
    [SerializeField] private RectTransform bulletContainer;   // ��������� ��� ����
    [SerializeField] private Sprite bulletSprite;             // ������ ����
    [SerializeField] private Image loadingBar;                // ��������-��� (Image Type = Filled)
    [SerializeField] private TextMeshProUGUI loadingText;     // ����� ���������

    [Header("Settings")]
    [SerializeField] private int maxBullets = 15;             // �������� ����
    [SerializeField] private float minLoadTime = 2f;          // ����������� ����� ������ UI
    [SerializeField] private float initialAnimationTime = 5f; // ����� ��������� ��������

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
        // �������� UI ��� ������
        loadingScreenCanvas.SetActive(false);

        // ������������ ������ ����
        float totalWidth = bulletContainer.rect.width;
        float spacing = 5f;
        bulletWidth = (totalWidth - spacing * (maxBullets - 1)) / maxBullets;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
        {
            Debug.LogWarning("�������� ��� �����������!");
            return;
        }
        StartCoroutine(ShowAndLoad(sceneName));
    }

    private IEnumerator ShowAndLoad(string sceneName)
    {
        isLoading = true;
        Debug.Log("ShowAndLoad: ������ ��������");

        // 1. �������� UI
        loadingScreenCanvas.SetActive(true);
        Debug.Log("ShowAndLoad: UI ��������");

        // 2. ��������� ����� ����� ��� ���������
        yield return new WaitForEndOfFrame();

        // 3. �������� UI
        foreach (var img in bulletImages) if (img) Destroy(img.gameObject);
        bulletImages.Clear();
        if (loadingBar != null) loadingBar.fillAmount = 0f;
        if (loadingText != null) loadingText.text = "0%";
        Debug.Log("ShowAndLoad: UI ��������");

        // 4. ��������� ��������
        float startTime = Time.time;
        while (Time.time - startTime < initialAnimationTime)
        {
            float prog = (Time.time - startTime) / initialAnimationTime;
            UpdateUI(prog * 0.3f); // �� 30% ��� ��������� ��������
            yield return null;
        }

        // 5. ������ ����������� �������� �����
        Debug.Log("ShowAndLoad: ������ �������� �����");
        var op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        // 6. ��������� UI �� ����� ��������
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

        // 7. ��������� UI �� 100%
        UpdateUI(1f);
        Debug.Log("ShowAndLoad: UI ��������� �� 100%");

        // 8. ��������� minLoadTime
        float remain = minLoadTime - (Time.time - startTime);
        if (remain > 0f) yield return new WaitForSeconds(remain);

        // 9. ������������ �����
        Debug.Log("ShowAndLoad: ��������� �����");
        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;

        // 10. �������� �������� ����� ��������� ����
        yield return new WaitForSeconds(0.1f); // ���� Unity ��������� ���������

        // 11. ������ UI
        loadingScreenCanvas.SetActive(false);
        Debug.Log("ShowAndLoad: UI ������");
        isLoading = false;
    }

    private void UpdateUI(float prog)
    {
        // �������� ��������-��� � �����
        if (loadingBar != null) loadingBar.fillAmount = prog;
        if (loadingText != null) loadingText.text = $"{Mathf.FloorToInt(prog * 100)}%";

        // ����� ����
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