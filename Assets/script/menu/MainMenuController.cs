using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private Button startButton;

    void Start()
    {
        if (startButton == null)
        {
            Debug.LogError("StartButton �� �������� � Inspector!");
            return;
        }
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void OnStartClicked()
    {
        if (LoadingScreen.Instance != null)
        {
            LoadingScreen.Instance.LoadScene("GameScene");
        }
        else
        {
            Debug.LogError("LoadingScreen.Instance ����������� � ����� MainMenu!");
        }
    }
}