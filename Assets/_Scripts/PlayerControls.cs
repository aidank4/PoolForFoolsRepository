using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerControls : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _cameraRef;
    private CameraMovement _camera;
    [SerializeField] private GameObject _powerUpUIRef;
    private PowerUpUI _powerUpUIScript; //need for highlighting the inventory buttons
    [SerializeField] private GameObject _inventoryRef;
    private InventoryManager _inventoryScript; // for UsePowerUp
    [SerializeField] private GameObject _pocketManagerRef;
    private PocketManager _pocketManager;
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
        _camera = _cameraRef.GetComponent<CameraMovement>();
        _powerUpUIScript = _powerUpUIRef.GetComponent<PowerUpUI>();
        _inventoryScript = _inventoryRef.GetComponent<InventoryManager>();
        _poolCueScript = _poolCueRef.GetComponent<Player>();
        _pocketManager = _pocketManagerRef.GetComponent<PocketManager>();
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
        if (context.performed && _poolCueScript.shootAvailable)
        {
            _inventoryScript.StartPowerUp(_slotA);
        }
    }


    /// <summary>
    /// Use the power up in the second slot.
    /// </summary>
    public void UsePowerUpB(InputAction.CallbackContext context)
    {
        if (context.performed && _poolCueScript.shootAvailable)
        {
            _inventoryScript.StartPowerUp(_slotB);
        }
    }


    /// <summary>
    /// Use the power up in the third slot.
    /// </summary>
    public void UsePowerUpC(InputAction.CallbackContext context)
    {
        if (context.performed && _poolCueScript.shootAvailable)
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
    /// Invokes CornerSabotageUI() which moves the UI from the center to the corner.
    /// Then changes input to gameplay
    /// </summary>
    /// <param name="context"></param>
    public void CloseSabotageUI(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            SabotageUIHelper();
        }
    }


    /// <summary>
    /// Close sabotage UI and if corks coming down then change map to Hold
    /// </summary>
    private void SabotageUIHelper()
    {
        _powerUpUIScript.CornerSabotageUI();

        if (_pocketManager.corking) //If corks coming down dont set to gameplay
        {
            InputManager.Instance.SwitchCurrentActionMap("Hold");
        }
        else
        {
            InputManager.Instance.SwitchCurrentActionMap("Gameplay");
        }
    }


    /// <summary>
    /// switches to UI inputs and highlights an inventory slot
    /// </summary>
    /// <param name="context"></param>
    public void SwitchToUIState(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_poolCueScript.shootAvailable == true) //Cant open inventory if already shot - have to before shooting
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


    /// <summary>
    /// Toggle camera controls
    /// </summary>
    /// <param name="context"></param>
    public void CameraMap(InputAction.CallbackContext context)
    {
        if (context.performed && _poolCueScript.shootAvailable) //Only can move cam before shot
        {
            _camera.ToggleCamUI();
            if (InputManager.Instance.currentActionMap.name == "CameraMove" || InputManager.Instance.currentActionMap.name == "CameraRot") //Switch to game
            {
                Debug.Log("Back to game");
                InputManager.Instance.SwitchCurrentActionMap("Gameplay");
                

            }
            else //Switch to cam
            {
                Debug.Log("Camera Time");
                InputManager.Instance.SwitchCurrentActionMap("CameraMove");
            }
        }
    }



    /// <summary>
    /// Toggle camera rotation controls
    /// </summary>
    /// <param name="context"></param>
    public void CameraRot(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (InputManager.Instance.currentActionMap.name == "CameraMove") //Toggle
            {
                Debug.Log("Rotation");
                InputManager.Instance.SwitchCurrentActionMap("CameraRot");

            }
            else
            {
                Debug.Log("back to cam");
                InputManager.Instance.SwitchCurrentActionMap("CameraMove");
            }
        }
    }

    /// <summary>
    /// If Sabotage UI is open on hold close it
    /// </summary>
    /// <param name="context"></param>
    public void Escape(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (_powerUpUIScript._sabotageOverlay.gameObject.activeSelf)
            {
                SabotageUIHelper();
            }
        }
    }
}
