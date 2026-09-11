using UnityEngine;

public class TurnManager : MonoBehaviour
{
    private int _playerOne = 1;
    private int _playerTwo = 2;
    public int currentPlayer { get; private set; }

    public int teamStripes { get; private set; }
    public int teamSolids { get; private set; }

    public bool keepTurn = false;

    private void Awake()
    {
        //set player one as initial player on startup
        currentPlayer = _playerOne;
    }


    //Switch currentPlayer
    public void SwitchTurn()
    {
        if (!keepTurn)
        {
            if (currentPlayer == _playerOne)
            {
                currentPlayer = _playerTwo;
            }
            else
            {
                currentPlayer = _playerOne;
            }
            //Play PLAYER 1 or 2 UI, probs function called NewTurnUI

        }

        Debug.Log("current player is " + currentPlayer);
    }


    public void TurnDeciderStripes()
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
            teamStripes = _playerTwo;
        }
    }


}
