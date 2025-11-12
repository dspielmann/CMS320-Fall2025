using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InstructionsMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button startButton;

    void Start()
    {
        // Make sure the button is linked before adding listener
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartGame);
        }
        else
        {
            Debug.LogWarning("StartButton not assigned in the Inspector!");
        }
    }

    public void StartGame()
    {
        Debug.Log("Starting Level 1...");
        SceneManager.LoadScene("Level1");
    }
}
