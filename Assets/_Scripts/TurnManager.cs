using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _poolCueRef; //Player Script
    private Player _poolCueScript;
    [SerializeField] private GameObject _inventoryRef; //Inventory
    private InventoryManager _inventoryScript;
    [SerializeField] private TMP_Text _turnText; //Turn switch transition
    [SerializeField] private GameObject _turnSwitchUI;
    private DartAnimEndController _turnSwitchAnimScript;
    [SerializeField] private GameObject _puUIRef;
    private PowerUpUI _puUI;
    [SerializeField] private GameObject _teamAssignUI; //Team assignment UI
    [SerializeField] private Sprite _teamStripesSprite;
    [SerializeField] private Sprite _teamSolidsSprite;
    [SerializeField] private GameObject _pOneIconUI;
    [SerializeField] private GameObject _pTwoIconUI;
    [SerializeField] private Sprite _stripesIcon;
    [SerializeField] private Sprite _solidsIcon;



    [Header("Ints")]
    public int playerOne { get; private set; }  = 1;
    public int playerTwo { get; private set; } = 2;
    public int currentPlayer { get; private set; }
    public int teamStripes { get; private set; }
    public int teamSolids { get; private set; }



    [Header("Bools")]
    public bool keepTurn = false; //Used to see if a player scored a team ball
    private bool _lostTurn = false; // fixes the issuewhere if a good ball was scored after bad ball then keep turn would still be made true
    public bool scratch = false; //Gets set true by the pocket manager


    [Header("PowerUps")]
    [SerializeField]private bool _sabotaged = false;


    //Events
    public event Action<bool> TurnSwitched;

    private void Awake()
    {
        //ref
        #region
        _poolCueScript = _poolCueRef.GetComponent<Player>();
        _turnSwitchAnimScript = _turnSwitchUI.GetComponent<DartAnimEndController>();
        _inventoryScript = _inventoryRef.GetComponent<InventoryManager>();
        _puUI = _puUIRef.GetComponent<PowerUpUI>();
        #endregion
        //UI
        #region
        _turnSwitchUI.SetActive(false);
        _teamAssignUI.SetActive(false);
        _pOneIconUI.SetActive(false);
        _pTwoIconUI.SetActive(false);
        #endregion
        //set p1 as initial player on startup
        currentPlayer = playerOne;
    }

    private void OnEnable()
    {
        if (_turnSwitchAnimScript != null)
        {
            _turnSwitchAnimScript.OnAnimationComplete += TriggerTurnSwitchEvent; //Gets called when dart animation ends
        }
        SO_PowerUp.UsePU += HandlePU;
    }


    private void OnDisable()
    {
        if (_turnSwitchAnimScript != null)
        {
            _turnSwitchAnimScript.OnAnimationComplete -= TriggerTurnSwitchEvent; //Gets called when dart animation ends
        }
        SO_PowerUp.UsePU -= HandlePU;
    }

    
    /// <summary>
    /// Checks if the player keeps their turn. Then handles switching or keeping turn.
    /// </summary>
    public void SwitchTurn() //Used in the InventoryManager
    {
        if (!keepTurn)
        {
            _inventoryScript.UpdateInventory();
            _inventoryScript.ResetOldPUCollections();
            _puUI.ResetSabotageUI();
            _turnSwitchUI.SetActive(true); //Play the turn transition
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
        else
        {
            //Player keeps turn
            //when turn switches this is handled by TriggerTurnSwitchEvent down below which gets called once the dart anim ends
            _poolCueScript.shootAvailable = true;
            _poolCueScript.RespawnPoolCue();
            _puUI.ResetSabotageUI();
            _inventoryScript.usingPUs.Clear();
        }
    }


    /// <summary>
    /// Determines if stripes gets to keep turn
    /// </summary>
    public void TurnDeciderStripes()
    {
        if (!_lostTurn)
        {
            if (scratch)
            {
                keepTurn = false;
                _lostTurn = true;
            }
            else
            {
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


    /// <summary>
    /// Determines if solids gets to keep their turn
    /// </summary>
    public void TurnDeciderSolids()
    {
        if (!_lostTurn)
        {
            if (scratch)
            {
                keepTurn = false;
                _lostTurn= true;
            }
            else
            {
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


    /// <summary>
    /// Assigns the current player to team stripes and the other player to team solids
    /// </summary>
    public void AssignStripes()
    {
        teamStripes = currentPlayer;
        _teamAssignUI.SetActive(true); //Team text animation
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


    /// <summary>
    /// Assigns the current player to team solids and the other player to team stripes.
    /// </summary>
    public void AssignSolids()
    {
        teamSolids = currentPlayer;
        _teamAssignUI.SetActive(true); //Team text animation
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
    /// highlights which team is currently going. Used by the scoreboard
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
            pTwoGlow.transform.localScale = Vector3.one;
            pOneGlow.transform.localScale = Vector3.zero;
        }
    }


    /// <summary>
    /// Turns on shootAvailable, respawn pool cue, and invoke event.
    /// </summary>
    private void TriggerTurnSwitchEvent()
    {
        _poolCueScript.shootAvailable = true;
        _poolCueScript.RespawnPoolCue();
        TurnSwitched?.Invoke(_sabotaged);
        _sabotaged = false;
        //TEMP GIVE PU on turn switch
        _inventoryScript.AddPowerUp();
    }


    /// <summary>
    /// on power up use set sabotaged true if affect next was true
    /// </summary>
    /// <param name="powerUp"></param>
    private void HandlePU(SO_PowerUp powerUp)
    {
        if (powerUp.affectNext)
        {
            _sabotaged = true;
        }
    }
}
