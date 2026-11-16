using UnityEngine;
using UnityEngine.UIElements;

public class UI_Score : MonoBehaviour
{
    private int scoreP1 = 0;
    private int scoreP2 = 0;
    private Label Player1ScoreCount;
    private Label Player2ScoreCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UIDocument uiDocument = FindFirstObjectByType<UIDocument>();
        if (uiDocument != null)
        {
            Player1ScoreCount = uiDocument.rootVisualElement?.Q<Label>("Player1ScoreCount");
            Player2ScoreCount = uiDocument.rootVisualElement?.Q<Label>("Player2ScoreCount");
        }


        UpdateScore();

    }

    // Update is called once per frame
    void Update()
    {

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
    public void Goalmade(int player)
    {
        if (player == 1)
        {
            scoreP1++;
        }
        else
        {
            scoreP2++;
        }
        UpdateScore();
    }
}
