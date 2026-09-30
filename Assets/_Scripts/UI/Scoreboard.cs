using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TextMeshProUGUI _pOneScoreText;
    [SerializeField] private TextMeshProUGUI _pTwoScoreText;
    [SerializeField] private TextMeshProUGUI _playerXText;
    [SerializeField] private Image _pOneGlow;
    [SerializeField] private Image _pTwoGlow;
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;



    [Header("Ints")]
    public int pOneScore = 0;
    public int pTwoScore = 0;



    private void Awake()
    {
        //ref
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
        //UI
        #region
        _pOneScoreText.text = pOneScore.ToString();
        _pTwoScoreText.text = pTwoScore.ToString();
        //initialize glows to invisible
        _pOneGlow.transform.localScale = Vector3.zero;
        _pTwoGlow.transform.localScale = Vector3.zero;
        #endregion
    }

    private void FixedUpdate()
    {
        //update text
        _pOneScoreText.text = pOneScore.ToString();
        _pTwoScoreText.text = pTwoScore.ToString();

        _turnManagerScript.ShowTeam(_pOneGlow, _pTwoGlow);
        CurrentPlayerUI();
    }


    /// <summary>
    /// Sets top left UI of the current player to P1 or P2
    /// </summary>
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
