using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _powerUpUIRef;
    private PowerUpUI _powerUpUIScript; //need for highlighting the inventory buttons
    [SerializeField] private GameObject _inventoryRef;
    private InventoryManager _inventoryScript; // for UsePowerUp
    [SerializeField] private GameObject _poolCueRef;
    private Player _poolCueScript; //need so we can check if shot is available
    [SerializeField] private TMP_Text _controlsExplained;
    [SerializeField] private TMP_Text _controlsHidden;
    [SerializeField] private GameObject _slotA;
    [SerializeField] private GameObject _slotB;
    [SerializeField] private GameObject _slotC;



    private void Awake()
    {
        //ref
        #region
        _powerUpUIScript = _powerUpUIRef.GetComponent<PowerUpUI>();
        _inventoryScript = _inventoryRef.GetComponent<InventoryManager>();
        _poolCueScript = _poolCueRef.GetComponent<Player>();
        #endregion
        //UI
        #region
        _controlsExplained.gameObject.SetActive(false);
        _controlsHidden.gameObject.SetActive(true);
        #endregion
    }

    //Use Power Ups
    #region
    /// <summary>
    /// Use the power up in the first slot.
    /// </summary>
    public void UsePowerUpA(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _inventoryScript.StartPowerUp(_slotA);
        }
    }


    /// <summary>
    /// Use the power up in the second slot.
    /// </summary>
    public void UsePowerUpB(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _inventoryScript.StartPowerUp(_slotB);
        }
    }


    /// <summary>
    /// Use the power up in the third slot.
    /// </summary>
    public void UsePowerUpC(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _inventoryScript.StartPowerUp(_slotC);
        }
    }


    /// <summary>
    /// Call UsePowerUp which closes visuals and activates power up
    /// </summary>
    /// <param name="context"></param>
    public void ActivatePowerUp(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _inventoryScript.UsePowerUp();
        }
    }
    #endregion

    

    /// <summary>
    /// switches to UI inputs and highlights an inventory slot
    /// </summary>
    /// <param name="context"></param>
    public void SwitchToUIState(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_poolCueScript.shootAvailable == true) //Cant open power ups if already shot - have to before shooting
            {
                InputManager.Instance.SwitchCurrentActionMap("UI");
                _powerUpUIScript.HighlightSlotButton();
            }
        }
    }


    /// <summary>
    /// Closes UI and switches input state to gameplay
    /// </summary>
    /// <param name="context"></param>
    public void SwitchToGamePlayState(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _powerUpUIScript.HidePUConflictUI();
        }
    }


    /// <summary>
    /// Toggle between showing controls and hiding them
    /// </summary>
    /// <param name="context"></param>
    public void ToggleControls(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if(_controlsHidden.gameObject.activeSelf)
            {
                _controlsExplained.gameObject.SetActive(true);
                _controlsHidden.gameObject.SetActive(false);
                return;
            }
            else
            {
                _controlsExplained.gameObject.SetActive(false);
                _controlsHidden.gameObject.SetActive(true);
            }
        }
    }
}
