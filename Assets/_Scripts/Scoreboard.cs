using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _pOneScoreText;
    [SerializeField] private TextMeshProUGUI _pTwoScoreText;

    [SerializeField] private TextMeshProUGUI _playerXText;

    [SerializeField] private Image _pOneGlow;
    [SerializeField] private Image _pTwoGlow;

    //reference turn manager to update glow on scoreboard
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;


    public int pOneScore = 0;
    public int pTwoScore = 0;

    private void Awake()
    {
        _pOneScoreText.text = pOneScore.ToString();
        _pTwoScoreText.text = pTwoScore.ToString();

        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();

        //initialize glows to invisible
        _pOneGlow.transform.localScale = Vector3.zero;
        _pTwoGlow.transform.localScale = Vector3.zero;
    }

    private void FixedUpdate()
    {
        _pOneScoreText.text = pOneScore.ToString();
        _pTwoScoreText.text = pTwoScore.ToString();

        //Show glow
        _turnManagerScript.ShowTeam(_pOneGlow, _pTwoGlow);

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
