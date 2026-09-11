using UnityEngine;
using TMPro;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stripeScoreText;
    [SerializeField] private TextMeshProUGUI _solidScoreText;

    public int stripeScore = 0;
    public int solidScore = 0;

    private void Awake()
    {
        _stripeScoreText.text = stripeScore.ToString();
        _solidScoreText.text = solidScore.ToString();
    }

    private void FixedUpdate()
    {
        _stripeScoreText.text = stripeScore.ToString();
        _solidScoreText.text = solidScore.ToString();
    }

}
