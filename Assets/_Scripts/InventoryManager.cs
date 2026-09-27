using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager : MonoBehaviour
{
    //References
    [SerializeField] private GameObject _powerUpUIRef;
    private PowerUpUI _powerUpUIScript;
    [SerializeField] private TMP_Text _swapText;

    //Ball manager for balls stopped event which can trigger power up choices as turn is over
    [SerializeField] private GameObject _ballManagerRef;
    private BallManager _ballManagerScript;

    //TurnManager to switch turn
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;

    //Array for slot icons
    [SerializeField] private Image[] _slots = new Image[3];
    //Array of powerUps
    [SerializeField] private SO_PowerUp[] _availablePowerUps;
    private SO_PowerUp[] _collectedPowerUps = new SO_PowerUp[3];

    //if an extra power up gets claimed, reference it here for if we swap it or delete it later
    private SO_PowerUp _extraPowerUp = null;

    private void Awake()
    {
        _ballManagerScript = _ballManagerRef.GetComponent<BallManager>();
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
        _powerUpUIScript = _powerUpUIRef.GetComponent<PowerUpUI>();
        _swapText.gameObject.SetActive(false); 

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
    }

    private void SetIcons()
    {
        for (int i = 0;  i < _collectedPowerUps.Length; i++)
        {
            if (_collectedPowerUps[i] == null)
            {
                //if no power up for the slot hide the slot
                _slots[i].gameObject.SetActive(false);
            }
            else
            {
                //set icon to powerup
                _slots[i].sprite = _collectedPowerUps[i].icon;
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
        SO_PowerUp rdmPowerUp = _availablePowerUps[UnityEngine.Random.Range(0, _availablePowerUps.Length)];
        return rdmPowerUp;
    }

    private void AddPowerUp()
    {
        //select power up to start
        SO_PowerUp selectedPowerUp = SelectPowerUp();

        for (int i = 0; i < _collectedPowerUps.Length; i++)
        {
            //if there is open space add random power up there
            if (_collectedPowerUps[i] == null)
            {
                //show power up ui
                StartCoroutine(_powerUpUIScript.DisplayPowerUpIcon(selectedPowerUp));
                _collectedPowerUps[i] = selectedPowerUp;
                SetIcons();
                return;
            }
            //if this is the last index and there is no space --- do something
            else if (_collectedPowerUps[i] != null && i == _collectedPowerUps.Length - 1)
            {
                //keep reference to this powerup that doesnt fit
                _extraPowerUp = selectedPowerUp;

            }
        }
    }
    
    /// <summary>
    /// Dragged into Power up UI button
    /// </summary>
    public void TossPowerUp()
    {
        _powerUpUIScript.HidePUConflictUI();

        _extraPowerUp = null;

        //Switch Turn -- this gets called also in swap because swap calls toss
        _turnManagerScript.SwitchTurn();

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

    public void SwapPowerUp(GameObject selectedSlot)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (_slots[i].transform.parent.gameObject == selectedSlot) // have to compare with parent gameobject because slots is image[] and button is parent
            {
                Debug.Log(_slots[i].name + " Matched");
                _collectedPowerUps[i] = _extraPowerUp;
                SetIcons();
                TossPowerUp(); // hide UI and return to gameplay

                //hide swap text
                _swapText.gameObject.SetActive(false);
               
                return;
            }
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
        }
    }

}
