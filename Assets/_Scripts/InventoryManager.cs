using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.ProjectAuditor.Editor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class InventoryManager : MonoBehaviour
{
    //References
    [SerializeField] private GameObject _powerUpUIRef;
    private PowerUpUI _powerUpUIScript;
    [SerializeField] private TMP_Text _swapText;
    [SerializeField] private TMP_Text _slotDescription;
    [SerializeField] private Image _alreadyUsedUI;

    //Ball manager for balls stopped event which can trigger power up choices as turn is over
    [SerializeField] private GameObject _ballManagerRef;
    private BallManager _ballManagerScript;

    //TurnManager to switch turn
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;

    //Array for slot icons
    [SerializeField] private Image[] _slots = new Image[3];
    //Array of powerUps
    [SerializeField] private SO_PowerUp[] _availablePUs;
    private SO_PowerUp[] _currentPUs = new SO_PowerUp[3];
    //Array for each player
    private SO_PowerUp[] _pOnePUs = new SO_PowerUp[3];
    private SO_PowerUp[] _pTwoPUs = new SO_PowerUp[3];

    //if an extra power up gets claimed, reference it here for if we swap it or delete it later
    private SO_PowerUp _extraPowerUp = null;

    //bool for stopping player from using multiple power ups in turn
    private bool _powerUpUsed = false;

    private void Awake()
    {
        //Ref
        _ballManagerScript = _ballManagerRef.GetComponent<BallManager>();
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
        _powerUpUIScript = _powerUpUIRef.GetComponent<PowerUpUI>();
        //UI
        _swapText.gameObject.SetActive(false);
        _slotDescription.transform.parent.gameObject.SetActive(false);
        _alreadyUsedUI.gameObject.SetActive(false);

    }

    private void OnEnable()
    {
        if (_ballManagerScript != null)
        {
            _ballManagerScript.BallsStopped += BallsStoppedEventTriggered;
        }
    }
    private void OnDisable()
    {
        if (_ballManagerScript != null)
        {
            _ballManagerScript.BallsStopped -= BallsStoppedEventTriggered;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            AddPowerUp();
        }
        //When in UI mode show the descriptions for inventory items when hovered
        if (InputManager.Instance.currentActionMap.name == "UI")
        {
            ShowPUDescription();
        }
    }

    /// <summary>
    /// Set current players inventory to have all the current power ups
    /// then set the current power ups to be the other players power ups
    /// then set icons
    /// use this in turn switch
    /// </summary>
    public void UpdatePowerUps()
    {
        //Update current players PUs
        if (_turnManagerScript.currentPlayer == _turnManagerScript.playerOne)
        {
            _pOnePUs = _currentPUs;
            //set current PUs to be the following players
            _currentPUs = _pTwoPUs;
        }
        else if (_turnManagerScript.currentPlayer == _turnManagerScript.playerTwo)
        {
            _pTwoPUs = _currentPUs;
            _currentPUs = _pOnePUs;
        }
        SetIcons();
    }

    private void SetIcons()
    {
        for (int i = 0;  i < _currentPUs.Length; i++)
        {
            if (_currentPUs[i] == null)
            {
                //if no power up for the slot hide the slot
                _slots[i].gameObject.SetActive(false);
            }
            else
            {
                //set icon to powerup
                _slots[i].sprite = _currentPUs[i].icon;
                //if the slot is currently hidden
                if (!_slots[i].gameObject.activeSelf)
                {
                    _slots[i].gameObject.SetActive(true);
                }
            }
        }
    }

    private SO_PowerUp SelectPowerUp()
    {
        SO_PowerUp rdmPowerUp = _availablePUs[UnityEngine.Random.Range(0, _availablePUs.Length)];
        return rdmPowerUp;
    }

    private void AddPowerUp()
    {
        //select power up to start
        SO_PowerUp selectedPowerUp = SelectPowerUp();

        for (int i = 0; i < _currentPUs.Length; i++)
        {
            //if there is open space add random power up there
            if (_currentPUs[i] == null)
            {
                //show power up ui
                StartCoroutine(_powerUpUIScript.DisplayPowerUpIcon(selectedPowerUp));
                _currentPUs[i] = selectedPowerUp;
                SetIcons();
                return;
            }
            //if this is the last index and there is no space --- do something
            else if (_currentPUs[i] != null && i == _currentPUs.Length - 1)
            {
                //keep reference to this powerup that doesnt fit
                _extraPowerUp = selectedPowerUp;

            }
        }
    }

    private void RemovePowerUp(int index)
    {
        _currentPUs[index] = null;
        SetIcons();
    }
    
    /// <summary>
    /// Dragged into Power up UI button
    /// </summary>
    public void TossExtraPowerUp()
    {
        _powerUpUIScript.HidePUConflictUI();

        _extraPowerUp = null;

        //Switch Turn -- this gets called also in swap because swap calls toss
        _turnManagerScript.SwitchTurn();
        UpdatePowerUps();
        _powerUpUsed = false; //once turn switches can use power up again

    }
    /// <summary>
    /// Dragged into Power up UI button
    /// </summary>
    public void PowerUpSwapState()
    {
        if (_extraPowerUp != null)
        {
            Debug.Log(_extraPowerUp.ToString());
            //Text explantion UI
            _swapText.gameObject.SetActive(true);
            _powerUpUIScript.HighlightSlotButton();
        }
    }
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

    public void UseOrSwapPowerUp(GameObject selectedSlot)
    {
        //if there is an extra power up waiting then we are swapping, if not we are using powerup
        if (_extraPowerUp != null)
        {
            Debug.Log(_slots[SlotNumber(selectedSlot)].name + " Matched");
            _currentPUs[SlotNumber(selectedSlot)] = _extraPowerUp;
            SetIcons();
            TossExtraPowerUp(); // hide UI and return to gameplay

            //hide swap text
            _swapText.gameObject.SetActive(false);
        }
        else
        {
            //Check if there is a power up in the slot
            if (_currentPUs[SlotNumber(selectedSlot)] != null)
            {
                //Check if we have used a power up
                if (!_powerUpUsed)
                {
                    //Use power up
                    Debug.Log("Use following power up: " + _currentPUs[SlotNumber(selectedSlot)].id);
                    _powerUpUsed = true;

                    //Remove power up from inventory because we are using it
                    RemovePowerUp(SlotNumber(selectedSlot));
                    _powerUpUIScript.HidePUConflictUI();

                }
                else
                {
                    //You already used a power up this turn, show ui warning 
                    StartCoroutine(PowerUpWarning());
                }
            }
            //if no power up in slot do nothing
        }
    }


    private void BallsStoppedEventTriggered()
    {
        if (_extraPowerUp != null)
        {
            //show power up UI before switching turn
            _powerUpUIScript.ShowPUConflictUI(_extraPowerUp);
        }
        else
        {
            //Switch Turn
            _turnManagerScript.SwitchTurn();
            UpdatePowerUps();
            _powerUpUsed = false; //once turn switches can use power up again
        }
    }

    private void ShowPUDescription()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            //if the current slot is being selected
            if (_slots[i].transform.parent.gameObject == EventSystem.current.currentSelectedGameObject)
            {
                //check if not empty
                if (_currentPUs[i] != null)
                {
                    //update description to power up
                    _slotDescription.text = _currentPUs[i].description;
                }
                else
                {
                    _slotDescription.text = "No power Up O_O";
                }
                //show description
                _slotDescription.transform.parent.gameObject.SetActive(true);
                return;
            }
        }
    }
    IEnumerator PowerUpWarning()
    {
        _alreadyUsedUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        _alreadyUsedUI.gameObject.SetActive(false);
    }
}
