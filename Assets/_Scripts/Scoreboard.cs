using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _stripeScoreText;
    [SerializeField] private TextMeshProUGUI _solidScoreText;

    [SerializeField] private TextMeshProUGUI _playerXText;

    [SerializeField] private RawImage _solidGlow;
    [SerializeField] private RawImage _stripeGlow;

    //reference turn manager to update glow on scoreboard
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;


    public int stripeScore = 0;
    public int solidScore = 0;

    private void Awake()
    {
        _stripeScoreText.text = stripeScore.ToString();
        _solidScoreText.text = solidScore.ToString();

        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();

        //initialize glows to invisible
        _solidGlow.transform.localScale = Vector3.zero;
        _stripeGlow.transform.localScale = Vector3.zero;
    }

    private void FixedUpdate()
    {
        _stripeScoreText.text = stripeScore.ToString();
        _solidScoreText.text = solidScore.ToString();

        //Show glow
        _turnManagerScript.ShowTeam(_solidGlow, _stripeGlow);

        //show player in top left
        CurrentPlayerUI();
    }

    private void CurrentPlayerUI()
    {
        if (_turnManagerScript.currentPlayer == _turnManagerScript.playerOne)
        {
            _playerXText.text = "P1";
        }
        else if (_turnManagerScript.currentPlayer == _turnManagerScript.playerTwo)
        {
            _playerXText.text = "P2";
        }
    }

}
