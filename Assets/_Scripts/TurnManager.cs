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

    //team icon Ui
    [SerializeField] private GameObject _pOneIconUI;
    [SerializeField] private GameObject _pTwoIconUI;
    [SerializeField] private Sprite _stripesIcon;
    [SerializeField] private Sprite _solidsIcon;


    public int playerOne { get; private set; }  = 1;
    public int playerTwo { get; private set; } = 2;
    public int currentPlayer { get; private set; }

    public int teamStripes { get; private set; }
    public int teamSolids { get; private set; }

    public bool keepTurn = false;
    private bool _lostTurn = false; // only using keep turn had an issue where if a good ball was scored after bad ball then keep turn would still be made true
    public bool scratch = false;

    private void Awake()
    {
        //set ui false
        _turnSwitchUI.SetActive(false);
        _teamAssignUI.SetActive(false);
        _pOneIconUI.SetActive(false);
        _pTwoIconUI.SetActive(false);

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
            _lostTurn = false; // reset lost turn once the switch turn has been called
            scratch = false;

        }
        //Debug.Log("current player is " + currentPlayer);
    }


    public void TurnDeciderStripes()
    {
        //make sure we can potentially keep our turn - you lost turn by scoring a bad ball
        if (!_lostTurn)
        {
            //if you scratch you lose your turn
            if (scratch)
            {
                keepTurn = false;
                _lostTurn = true;
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
                    _lostTurn = true;
                }
            }
        }
    }

    public void TurnDeciderSolids()
    {
        //make sure we can potentially keep our turn - you lost turn by scoring a bad ball
        if (!_lostTurn)
        {
            //if you scratch you lose your turn
            if (scratch)
            {
                keepTurn = false;
                _lostTurn= true;
            }
            else
            {
                //Keep turn or lose it depending on if you scored your type of ball
                if (currentPlayer == teamSolids)
                {
                    keepTurn = true;
                }
                else if (currentPlayer != teamSolids)
                {
                    keepTurn = false;
                    _lostTurn = true;
                }
            }
        }

    }

    public void AssignStripes()
    {
        teamStripes = currentPlayer;

        //turn on object with animation -- stripes sprite
        _teamAssignUI.SetActive(true);
        _teamAssignUI.GetComponentInChildren<Image>().sprite = _teamStripesSprite;


        //Assign solids to other player and set team icon
        if (teamStripes == playerOne)
        {
            _pOneIconUI.GetComponent<Image>().sprite = _stripesIcon;

            teamSolids = playerTwo;
            _pTwoIconUI.GetComponent<Image>().sprite = _solidsIcon;

            //show icon
            _pOneIconUI.SetActive(true);
            _pTwoIconUI.SetActive(true);
        }
        else
        {
            _pTwoIconUI.GetComponent<Image>().sprite = _stripesIcon;

            teamSolids = playerOne;
            _pOneIconUI.GetComponent<Image>().sprite = _solidsIcon;

            //show icon
            _pOneIconUI.SetActive(true);
            _pTwoIconUI.SetActive(true);
        }
    }

    public void AssignSolids()
    {
        teamSolids = currentPlayer;

        //turn on object with animation -- solids sprite
        _teamAssignUI.SetActive(true);
        _teamAssignUI.GetComponentInChildren<Image>().sprite = _teamSolidsSprite;


        //Assign stripes to other player and set icon
        if (teamSolids == playerOne)
        {
            _pOneIconUI.GetComponent<Image>().sprite = _solidsIcon;

            teamStripes = playerTwo;
            _pTwoIconUI.GetComponent<Image>().sprite = _stripesIcon;

            //show icon
            _pOneIconUI.SetActive(true);
            _pTwoIconUI.SetActive(true);
        }
        else
        {
            _pTwoIconUI.GetComponent<Image>().sprite = _solidsIcon;

            teamStripes = playerOne;
            _pOneIconUI.GetComponent<Image>().sprite = _stripesIcon;

            //show icon
            _pOneIconUI.SetActive(true);
            _pTwoIconUI.SetActive(true);
        }
    }

    /// <summary>
    /// highlights which team is currently going, used by scoreboard
    /// </summary>
    public void ShowTeam(Image pOneGlow, Image pTwoGlow)
    {
        if (currentPlayer == playerOne)
        {
            pOneGlow.transform.localScale = Vector3.one; //show it by scaling it on
            pTwoGlow.transform.localScale = Vector3.zero; //hide it by scaling it off
        }
        else if (currentPlayer == playerTwo)
        {
            pTwoGlow.transform.localScale = Vector3.one; //show it by scaling it on
            pOneGlow.transform.localScale = Vector3.zero; //hide it by scaling it off
        }
    }


}
