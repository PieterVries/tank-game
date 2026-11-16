using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class GameManeger : MonoBehaviour
{
    public static GameManeger Instance;
    private static int scoreP1;
    private static int scoreP2;
    private Label Player1ScoreCount;
    private Label Player2ScoreCount;
    private int activePlayer = 1;
    private int pendingPlayerTurn;
    public GameObject TankL;
    public GameObject TankR;
    private Vector3 TankStartL;
    private Vector3 TankStartR;
    private bool roundEnding;
    //tijdelijk
    public KeyCode RestartButton = KeyCode.R;
    public UIDocument escapeMenuDocument;
    private Button resumeButton;
    private Button quitButton;
    private VisualElement escapeMenu;
    private bool isPauseMenuVisible;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        Instance = this;
        Time.timeScale = 1f;
    }

    void Start()
    {
        UIDocument[] uiDocuments = FindObjectsByType<UIDocument>(FindObjectsSortMode.None);
        foreach (UIDocument uiDocument in uiDocuments)
        {
            VisualElement root = uiDocument.rootVisualElement;
            Label player1Score = root.Q<Label>("Player1ScoreCount");
            Label player2Score = root.Q<Label>("Player2ScoreCount");
            if (player1Score != null || player2Score != null)
            {
                Player1ScoreCount = player1Score;
                Player2ScoreCount = player2Score;
                break;
            }
        }

        InitializePauseMenu();

        if (TankL != null && TankR != null)
        {
            TankStartL = TankL.transform.position;
            TankStartR = TankR.transform.position;

            SetPlayerNumber(TankL, 1);
            SetPlayerNumber(TankR, 2);
        }

        UpdateScore();
        UpdateTurnIndicator();
    }

    void InitializePauseMenu()
    {
        if (escapeMenuDocument == null)
        {
            Debug.LogError("GameManeger needs the pause menu UIDocument assigned.", this);
            return;
        }

        VisualElement root = escapeMenuDocument.rootVisualElement;
        resumeButton = root.Q<Button>("Resume");
        quitButton = root.Q<Button>("Quit");
        escapeMenu = root.Q<VisualElement>("body");

        if (resumeButton == null || quitButton == null || escapeMenu == null)
        {
            Debug.LogError("GameManeger could not find the Resume, Quit, or body elements in the pause menu.", this);
            return;
        }

        resumeButton.clicked += ResumeGame;
        quitButton.clicked += ReturnToMenu;
        SetPauseMenuVisible(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && escapeMenu != null)
        {
            SetPauseMenuVisible(!isPauseMenuVisible);
        }
    }

    void LateUpdate()
    {
        if (pendingPlayerTurn != 0)
        {
            activePlayer = pendingPlayerTurn;
            pendingPlayerTurn = 0;
            UpdateTurnIndicator();
        }
    }

    void OnDestroy()
    {
        if (resumeButton != null)
        {
            resumeButton.clicked -= ResumeGame;
        }

        if (quitButton != null)
        {
            quitButton.clicked -= ReturnToMenu;
        }

        Time.timeScale = 1f;
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void SetPlayerNumber(GameObject tankObject, int playerNumber)
    {
        Tankcontroller tankController = tankObject.GetComponent<Tankcontroller>();
        if (tankController != null)
        {
            tankController.playerNumber = playerNumber;
        }
    }

    void SetPauseMenuVisible(bool visible)
    {
        isPauseMenuVisible = visible;
        escapeMenu.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
        Time.timeScale = visible ? 0f : 1f;
    }

    void ResumeGame()
    {
        SetPauseMenuVisible(false);
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f;
        scoreP1 = 0;
        scoreP2 = 0;
        SceneManager.LoadScene(0);
    }

    public void TankDefeated(int defeatedPlayer)
    {
        if (roundEnding)
        {
            return;
        }

        roundEnding = true;

        if (defeatedPlayer == 1)
        {
            scoreP2++;
        }
        else if (defeatedPlayer == 2)
        {
            scoreP1++;
        }

        UpdateScore();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public bool IsPlayerTurn(int playerNumber)
    {
        return activePlayer == playerNumber;
    }

    public void EndTurn(int playerNumber)
    {
        if (activePlayer == playerNumber && pendingPlayerTurn == 0)
        {
            pendingPlayerTurn = playerNumber == 1 ? 2 : 1;
        }
    }

    private void UpdateScore()
    {
        if (Player1ScoreCount != null)
        {
            Player1ScoreCount.text = scoreP1.ToString();
        }

        if (Player2ScoreCount != null)
        {
            Player2ScoreCount.text = scoreP2.ToString();
        }
    }

    private void UpdateTurnIndicator()
    {
        if (Player1ScoreCount != null)
        {
            Player1ScoreCount.style.color = activePlayer == 1 ? Color.red : Color.white;
        }

        if (Player2ScoreCount != null)
        {
            Player2ScoreCount.style.color = activePlayer == 2 ? Color.red : Color.white;
        }
    }

}
