using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using TMPro;
using Unity.ProjectAuditor.Editor;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventoryManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _powerUpUIRef;
    private PowerUpUI _powerUpUIScript;
    [SerializeField] private TMP_Text _swapText;
    [SerializeField] private GameObject _controlsText;
    [SerializeField] private TMP_Text _slotDescription;
    [SerializeField] private Image _alreadyUsedUI;
    [SerializeField] private GameObject _ballManagerRef; //Ball manager for balls stopped event which can trigger power up choices as turn is over
    private BallManager _ballManagerScript;
    [SerializeField] private GameObject _turnManagerRef; //TurnManager to switch turn -- switch turn in Toss Function and BallsStopped Event
    private TurnManager _turnManagerScript;



    [Header("Containers")] //inventory size is always maxed at 3
    [SerializeField] private Image[] _slots = new Image[3];
    [SerializeField] private List<SO_PowerUp> _availablePUs = new List<SO_PowerUp>();
    private SO_PowerUp[] _currentPUs = new SO_PowerUp[3];
    private SO_PowerUp[] _pOnePUs = new SO_PowerUp[3];
    private SO_PowerUp[] _pTwoPUs = new SO_PowerUp[3];


    [Header("Misc Variables")]
    private SO_PowerUp _extraPowerUp = null; //if an extra power up gets claimed, this ref is used to swap it or delete it later
    private bool _powerUpUsed = false; //bool for stopping player from using multiple power ups in turn
    private int _pBCounter = 0; //Cant use pocket block more than 5 times


    private void Awake()
    {
        //Ref init
        #region
        _ballManagerScript = _ballManagerRef.GetComponent<BallManager>();
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
        _powerUpUIScript = _powerUpUIRef.GetComponent<PowerUpUI>();
        #endregion
        //UI init
        #region
        _swapText.gameObject.SetActive(false);
        _slotDescription.transform.parent.gameObject.SetActive(false);
        _alreadyUsedUI.gameObject.SetActive(false);
        #endregion

    }


    private void OnEnable()
    {
        if (_ballManagerScript != null)
        {
            _ballManagerScript.BallsStopped += BallsStoppedEventTriggered; //gets called when balls are stopped
        }
    }
    
    
    private void OnDisable()
    {
        if (_ballManagerScript != null)
        {
            _ballManagerScript.BallsStopped -= BallsStoppedEventTriggered; //gets called when balls are stopped
        }
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddPowerUp();
        }
        
        if (InputManager.Instance.currentActionMap.name == "UI")
        {
            ShowPUDescription(); //show item descriptions when they are being hovered over
        }
    }


    /// <summary>
    /// Update current players inventory with the current PUs
    /// then set the current PUs to be the other players PUs
    /// for the next turn and refresh icons.
    /// </summary>
    public void UpdateInventory()
    {
        //p1
        if (_turnManagerScript.currentPlayer == _turnManagerScript.playerOne)
        {
            _pOnePUs = _currentPUs;
            _currentPUs = _pTwoPUs;
        }
        //p2
        else if (_turnManagerScript.currentPlayer == _turnManagerScript.playerTwo)
        {
            _pTwoPUs = _currentPUs;
            _currentPUs = _pOnePUs;
        }
        SetIcons();
    }


    /// <summary>
    /// Current PUs icons are displayed by inventory
    /// </summary>
    private void SetIcons()
    {
        for (int i = 0;  i < _currentPUs.Length; i++)
        {
            if (_currentPUs[i] == null)
            {
                _slots[i].gameObject.SetActive(false); //if no power up for the slot hide the slot
            }
            else
            {
                _slots[i].sprite = _currentPUs[i].icon; //set icon to powerup
                if (!_slots[i].gameObject.activeSelf)
                {
                    _slots[i].gameObject.SetActive(true);
                }
            }
        }
    }


    /// <summary>
    /// Returns a random PU from the available PU's
    /// </summary>
    /// <returns>SO_Powerup</returns>
    private SO_PowerUp SelectPowerUp()
    {
        SO_PowerUp rdmPowerUp = _availablePUs[UnityEngine.Random.Range(0, _availablePUs.Count)];
        if (rdmPowerUp.id == "Pocket Block") // Can only get 5 pocketblocks per game
        {
            _pBCounter++;
            if (_pBCounter > 5)
            {
                _availablePUs.Remove(rdmPowerUp);
                rdmPowerUp = _availablePUs[UnityEngine.Random.Range(0, _availablePUs.Count)];
            }
        }
        return rdmPowerUp;
    }


    /// <summary>
    /// Gets a random power up and adds it to the inventory.
    /// If the inventory is full then the extra gets stored for later.
    /// </summary>
    private void AddPowerUp()
    {
        SO_PowerUp selectedPowerUp = SelectPowerUp();

        for (int i = 0; i < _currentPUs.Length; i++)
        {
            if (_currentPUs[i] == null) //if there is open space add random power up there
            {
                StartCoroutine(_powerUpUIScript.DisplayPowerUpIcon(selectedPowerUp));
                _currentPUs[i] = selectedPowerUp;
                SetIcons();
                return;
            }
            else if (_currentPUs[i] != null && i == _currentPUs.Length - 1) // last index and no inventory space
            {
                _extraPowerUp = selectedPowerUp; //extra will be swapped or tossed away later

            }
        }
    }


    /// <summary>
    /// remove PU at argument index
    /// </summary>
    /// <param name="index"></param>
    private void RemovePowerUp(int index)
    {
        _currentPUs[index] = null;
        SetIcons();
    }
    

    /// <summary>
    /// Sets the extra PU to null and then calls a turn switch and updates inventories
    /// </summary>
    public void TossExtraPowerUp() //NOTE Dragged into Toss button
    {
        _powerUpUIScript.HidePUConflictUI();
        _extraPowerUp = null;

        _turnManagerScript.SwitchTurn();
        _powerUpUsed = false; //once turn switches can use power up again

    }


    /// <summary>
    /// Transitions to inventory control so the player can swap in the extra PU.
    /// </summary>
    public void PowerUpSwapState() //NOTE Dragged into Swap button
    {
        if (_extraPowerUp != null)
        {
            _swapText.gameObject.SetActive(true); //UI that tells player to swap PUs
            _controlsText.SetActive(false);
            _powerUpUIScript.HighlightSlotButton();
        }
    }
    
    
    /// <summary>
    /// returns the slot number of the argument Gameobject
    /// </summary>
    /// <param name="selectedSlot"></param>
    /// <returns>int</returns>
    private int SlotNumber(GameObject selectedSlot)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].transform.parent.gameObject == selectedSlot) // have to compare with parent gameobject because slots is image[] and button is parent
            {
                return i;
            }
        }
        Debug.Log("SLOT NUMBER ERROR");
        return 0;
    }


    /// <summary>
    /// Swap the extra PU into the selected slot and overwrite the old PU
    /// </summary>
    /// <param name="selectedSlot"></param>
    private void SwapPowerUp(GameObject selectedSlot)
    {
        _currentPUs[SlotNumber(selectedSlot)] = _extraPowerUp; //overwrite selected slot with the extra
        SetIcons();
        TossExtraPowerUp(); // hide UI and return to gameplay
        _swapText.gameObject.SetActive(false);
        _controlsText.SetActive(true);
    }


    /// <summary>
    /// If there is a power up and one hasn't been used this turn activate the selected powerUp
    /// </summary>
    /// <param name="selectedSlot"></param>
    public void UsePowerUp(GameObject selectedSlot)
    {
        int i = SlotNumber(selectedSlot);
        if (_currentPUs[i] != null)
        {
            if (!_powerUpUsed)
            {
                Debug.Log("Use following power up: " + _currentPUs[i].id);
                _powerUpUIScript.HidePUConflictUI();
                _currentPUs[i].Activate();
                _powerUpUsed = true;
                RemovePowerUp(i); //Remove used PU and hide UI for gameplay

            }
            else
            {
                StartCoroutine(PowerUpWarning()); //You already used a power up this turn, show ui warning
            }
        }
    }

/// <summary>
/// On button click either swap PU's or use a PU.
/// This is dependant on if there is an extra PU.
/// </summary>
/// <param name="selectedSlot"></param>
public void UseOrSwapPowerUp(GameObject selectedSlot) //NOTE used by inventory buttons
    {
        if (_extraPowerUp != null)
        {
            SwapPowerUp(selectedSlot);
        }
        else
        {
            UsePowerUp(selectedSlot);
        }
    }


    /// <summary>
    /// If there is an extra PU then player must deal with it.
    /// Else switch turn.
    /// </summary>
    private void BallsStoppedEventTriggered() //Called by BallManager
    {
        if (_extraPowerUp != null)
        {
            _powerUpUIScript.ShowPUConflictUI(_extraPowerUp); //have to swap or toss extra PU before turn switch
        }
        else
        {
            //Switch Turn
            _turnManagerScript.SwitchTurn();
            _powerUpUsed = false; //once turn switches can use power up again
        }
    }


    /// <summary>
    /// Set the slots PU description and show the UI
    /// </summary>
    private void ShowPUDescription()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].transform.parent.gameObject == EventSystem.current.currentSelectedGameObject) //if the current slot is being selected
            {
                if (_currentPUs[i] != null) //check that its not empty
                {
                    _slotDescription.text = _currentPUs[i].description;
                }
                else
                {
                    _slotDescription.text = "No power Up O_O";
                }
                _slotDescription.transform.parent.gameObject.SetActive(true);
                return;
            }
        }
    }


    /// <summary>
    /// Display UI that warns player a PU can't be used twice in one turn (lasts 1 second)
    /// </summary>
    /// <returns>IEnumerator</returns>
    IEnumerator PowerUpWarning()
    {
        _controlsText.SetActive(false);
        _alreadyUsedUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        _alreadyUsedUI.gameObject.SetActive(false);
        _controlsText.SetActive(true);
    }
}
