using UnityEngine;
using TMPro;

public class PlayerUIController : MonoBehaviour
{
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI experienceText;
    private PlayerProgress playerProgress;

    void Start()
    {
        playerProgress = FindObjectOfType<PlayerProgress>();
        if (playerProgress == null)
        {
            Debug.LogError("PlayerProgress not found!");
            return;
        }
        UpdateUI();
    }

    public void UpdateUI()
    {
        levelText.text = playerProgress.currentLevel.ToString();
        experienceText.text = playerProgress.currentExperience + " / " + playerProgress.experienceToNextLevel;
    }
}