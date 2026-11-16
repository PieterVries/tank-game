using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
public class EscScript : MonoBehaviour
{
    private Button play;
    private Button exit;
    private VisualElement Escapemenu;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        UIDocument uiDocument = GetComponent<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("EscScript needs a UIDocument on the same GameObject.", this);
            enabled = false;
            return;
        }

        VisualElement root = uiDocument.rootVisualElement;
        play = root.Q<Button>("Resume");
        exit = root.Q<Button>("Quit");
        Escapemenu = root.Q<VisualElement>("body");

        if (play == null || exit == null || Escapemenu == null)
        {
            Debug.LogError("EscScript could not find the Resume, Quit, or body elements in the UIDocument.", this);
            enabled = false;
            return;
        }

        Escapemenu.style.display = DisplayStyle.None;

        play.clicked += Continue;
        exit.clicked += ReturnToMenu;
    }
    private void Continue()
    {
        Escapemenu.style.display = DisplayStyle.None;
        Time.timeScale = 1f;
    }

    private void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Escapemenu.style.display == DisplayStyle.None)
            {
                Escapemenu.style.display = DisplayStyle.Flex;
                Debug.Log("Escape");
                Time.timeScale = 0f;
            }
            else
            {
                Escapemenu.style.display = DisplayStyle.None;
                Time.timeScale = 1f;
            }

        }

    }
}
