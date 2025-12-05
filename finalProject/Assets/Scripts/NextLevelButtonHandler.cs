using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NextLevelButtonHandler : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextSceneName; // Set the next scene in Inspector, no quotes needed

    [Header("UI References")]
    public Button button; // Assign the Button component in Inspector

    void Awake()
    {
        if (button == null)
        {
            Debug.LogError("Button not assigned in NextLevelButtonHandler!");
            return;
        }

        button.gameObject.SetActive(false); // hide at start
        button.onClick.AddListener(GoToNextLevel);
    }

    /// <summary>
    /// Makes the button visible
    /// </summary>
    public void ShowButton()
    {
        if (button != null)
        {
            button.gameObject.SetActive(true);
            Debug.Log("Next Level button is now visible!");
        }
    }

    /// <summary>
    /// Loads the next scene
    /// </summary>
    private void GoToNextLevel()
    {
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading next scene: {nextSceneName}");
            SceneManager.LoadScene(nextSceneName);
        }
        else
        {
            Debug.LogWarning("Next scene name not set!");
        }
    }
}
