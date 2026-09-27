using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager : MonoBehaviour
{
    //References
    [SerializeField] private GameObject _powerUpUIRef;
    private PowerUpUI _powerUpUIScript;

    //Array for slot icons
    [SerializeField] private Image[] _slots = new Image[3];
    //Array of powerUps
    [SerializeField] private SO_PowerUp[] _availablePowerUps;
    private SO_PowerUp[] _collectedPowerUps = new SO_PowerUp[3];

    //if an extra power up gets claimed, reference it here for if we swap it or delete it later
    private SO_PowerUp _extraPowerUp;

    private void Awake()
    {
        _powerUpUIScript = _powerUpUIRef.GetComponent<PowerUpUI>();

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
        //show power up ui
        StartCoroutine(_powerUpUIScript.DisplayPowerUpIcon(selectedPowerUp));

        for (int i = 0; i < _collectedPowerUps.Length; i++)
        {
            //if there is open space add random power up there
            if (_collectedPowerUps[i] == null)
            {
                _collectedPowerUps[i] = selectedPowerUp;
                SetIcons();
                return;
            }
            //if this is the last index and there is no space --- do something
            else if (_collectedPowerUps[i] != null && i == _collectedPowerUps.Length - 1)
            {
                //keep reference to this powerup that doesnt fit
                _extraPowerUp = selectedPowerUp;
                
                //show UI
                _powerUpUIScript.ShowPUConflictUI(_extraPowerUp);

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

    }
    /// <summary>
    /// Dragged into Power up UI button
    /// </summary>
    public void SwapPowerUp()
    {
        if (_extraPowerUp != null)
        {
            Debug.Log(_extraPowerUp.ToString());

            //set slot to the extra
            _collectedPowerUps[0] = _extraPowerUp;
            SetIcons();

            //reset the extra and hide UI
            TossPowerUp();
        }
        else
        {
            Debug.Log("no power up to give ERROR");
        }
    }

}
