using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // Important for Button references

public class TitleScreenManager : MonoBehaviour
{
    private Button playButton;
    private Button quitButton;

    void Awake()
    {
        // Find buttons by name in the scene
        playButton = GameObject.Find("PlayButton").GetComponent<Button>();
        quitButton = GameObject.Find("QuitButton").GetComponent<Button>();

        // Add listeners in code
        playButton.onClick.AddListener(PlayGame);
        quitButton.onClick.AddListener(QuitGame);
    }

    public void PlayGame()
    {
        Debug.Log("Play button clicked!");
        SceneManager.LoadScene("Story");
    }

    public void QuitGame()
    {
        Debug.Log("Quit button clicked!");
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
