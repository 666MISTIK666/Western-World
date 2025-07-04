using UnityEngine;
using UnityEngine.UI;

public class ExitController : MonoBehaviour
{
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject exitDialog;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    void Start()
    {
        // Скрываем диалог подтверждения
        exitDialog.SetActive(false);

        // Подписываем кнопки
        exitButton.onClick.AddListener(ShowExitDialog);
        yesButton.onClick.AddListener(ExitToMainMenu);
        noButton.onClick.AddListener(HideExitDialog);
    }

    private void ShowExitDialog()
    {
        exitDialog.SetActive(true);
    }

    private void HideExitDialog()
    {
        exitDialog.SetActive(false);
    }

    private void ExitToMainMenu()
    {
        // Используем LoadingScreen для загрузки MainMenu
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("MainMenu");
        }
        else
        {
            Debug.LogError("LoadingScreen.Instance отсутствует в сцене!");
        }
    }
}