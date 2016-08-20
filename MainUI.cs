using UnityEngine;
using UnityEngine.UI;

public class MainUI : MonoBehaviour
{
    public Text ScoreText;

    public void SetScoreText(int amount)
    {
        ScoreText.text = "Score: " + amount.ToString();
    }
}