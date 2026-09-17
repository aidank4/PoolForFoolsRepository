using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    //Turn switch transition
    [SerializeField] private GameObject _turnSwitchUI;
    [SerializeField] private TMP_Text _turnText;

    //Team assignment UI
    [SerializeField] private GameObject _teamAssignUI;
    [SerializeField] private Sprite _teamStripesSprite;
    [SerializeField] private Sprite _teamSolidsSprite;

    public int playerOne { get; private set; }  = 1;
    public int playerTwo { get; private set; } = 2;
    public int currentPlayer { get; private set; }

    public int teamStripes { get; private set; }
    public int teamSolids { get; private set; }

    public bool keepTurn = false;
    public bool scratch = false;

    private void Awake()
    {
        //set ui false
        _turnSwitchUI.SetActive(false);
        _teamAssignUI.SetActive(false);

        //set player one as initial player on startup
        currentPlayer = playerOne;
    }


    //Switch currentPlayer
    public void SwitchTurn()
    {
        if (!keepTurn)
        {
            //Play the turn transition
            _turnSwitchUI.SetActive(true);
            _turnText.text = string.Empty;

            if (currentPlayer == playerOne)
            {
                currentPlayer = playerTwo;
                _turnText.text = "Player 2";
            }
            else
            {
                currentPlayer = playerOne;
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

        //turn on object with animation -- stripes sprite
        _teamAssignUI.SetActive(true);
        _teamAssignUI.GetComponentInChildren<Image>().sprite = _teamStripesSprite;
        Debug.Log("assign stripes");

        //Assign solids to other player
        if (teamStripes == playerOne)
        {
            teamSolids = playerTwo;
        }
        else
        {
            teamSolids = playerOne;
        }
    }

    public void AssignSolids()
    {
        teamSolids = currentPlayer;

        //turn on object with animation -- solids sprite
        _teamAssignUI.SetActive(true);
        _teamAssignUI.GetComponentInChildren<Image>().sprite = _teamSolidsSprite;

        Debug.Log("assign solids");

        //Assign stripes to other player
        if (teamSolids == playerOne)
        {
            teamStripes = playerTwo;
        }
        else
        {
            teamStripes = playerOne;
        }
    }

    /// <summary>
    /// highlights which team is currently going, used by scoreboard
    /// </summary>
    public void ShowTeam(RawImage solidGlow, RawImage stripesGlow)
    {
        if (currentPlayer == teamStripes)
        {
            stripesGlow.transform.localScale = Vector3.one; //show it by scaling it on
            solidGlow.transform.localScale = Vector3.zero; //hide it by scaling it off
        }
        else if (currentPlayer == teamSolids)
        {
            solidGlow.transform.localScale = Vector3.one; //show it by scaling it on
            stripesGlow.transform.localScale = Vector3.zero; //hide it by scaling it off
        }


    }


}
