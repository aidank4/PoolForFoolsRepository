using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    //Turn switch transition
    [SerializeField] private GameObject _turnSwitchUI;
    [SerializeField] private TMP_Text _turnText;

    private int _playerOne = 1;
    private int _playerTwo = 2;
    public int currentPlayer { get; private set; }

    public int teamStripes { get; private set; }
    public int teamSolids { get; private set; }

    public bool keepTurn = false;
    public bool scratch = false;

    private void Awake()
    {
        _turnSwitchUI.SetActive(false);
        //set player one as initial player on startup
        currentPlayer = _playerOne;
    }


    //Switch currentPlayer
    public void SwitchTurn()
    {
        if (!keepTurn)
        {
            //Play the turn transition
            _turnSwitchUI.SetActive(true);
            _turnText.text = string.Empty;

            if (currentPlayer == _playerOne)
            {
                currentPlayer = _playerTwo;
                _turnText.text = "Player 2";
            }
            else
            {
                currentPlayer = _playerOne;
                _turnText.text = "Player 1";
            }
            //Play PLAYER 1 or 2 UI, probs function called NewTurnUI

        }

        Debug.Log("current player is " + currentPlayer);
    }


    public void TurnDeciderStripes()
    {
        //if you scratch you lose your turn
        if (scratch)
        {
            keepTurn = false;
            scratch = false; // this might cause the else to play on a scratch
        }
        else
        {
            //Keep turn or lose it depending on if you scored your type of ball
            if (currentPlayer == teamStripes)
            {
                keepTurn = true;
            }
            else if (currentPlayer != teamStripes)
            {
                keepTurn = false;
            }
        }
    }

    public void TurnDeciderSolids()
    {
        //Keep turn or lose it depending on if you scored your type of ball
        if (currentPlayer == teamSolids)
        {
            keepTurn = true;
        }
        else if (currentPlayer != teamSolids)
        {
            keepTurn = false;
        }
    }

    public void AssignStripes()
    {
        teamStripes = currentPlayer;

        //Assign solids to other player
        if (teamStripes == _playerOne)
        {
            teamSolids = _playerTwo;
        }
        else
        {
            teamSolids = _playerOne;
        }
    }

    public void AssignSolids()
    {
        teamSolids = currentPlayer;

        //Assign stripes to other player
        if (teamSolids == _playerOne)
        {
            teamStripes = _playerTwo;
        }
        else
        {
            teamStripes = _playerOne;
        }
    }


}
