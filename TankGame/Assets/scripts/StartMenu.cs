using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class StartMenu : MonoBehaviour
{
    private UIDocument uiDocument;
    private Button playButton;
    private Button quitButton;

    private void OnEnable()
    {
        uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogWarning("StartMenu needs a UIDocument component on the same GameObject.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        playButton = root.Q<Button>("Play");
        quitButton = root.Q<Button>("Exit");

        if (playButton != null)
            playButton.clicked += OnPlayClicked;

        if (quitButton != null)
            quitButton.clicked += OnQuitClicked;
    }

    private void OnDisable()
    {
        if (playButton != null)
            playButton.clicked -= OnPlayClicked;

        if (quitButton != null)
            quitButton.clicked -= OnQuitClicked;
    }

    private void OnPlayClicked()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(0);
        }
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
