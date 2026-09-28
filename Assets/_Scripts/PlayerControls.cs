using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControls : MonoBehaviour
{
    //reference power up UI because we need to highlight the inventory buttons when this is clicked
    [SerializeField] private GameObject _powerUpUIRef;
    private PowerUpUI _powerUpUIScript;
    //Reference pool cue so we can check if shot is available
    [SerializeField] private GameObject _poolCueRef;
    private Player _poolCueScript;

    //Controls UIs
    [SerializeField] private TMP_Text _controlsExplained;
    [SerializeField] private TMP_Text _controlsHidden;


    private void Awake()
    {
        //ref
        _powerUpUIScript = _powerUpUIRef.GetComponent<PowerUpUI>();
        _poolCueScript = _poolCueRef.GetComponent<Player>();
        //UI
        _controlsExplained.gameObject.SetActive(false);
        _controlsHidden.gameObject.SetActive(true);
    }

    public void SwitchToUIState(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            //Cant open power ups if already shot - have to before shooting
            if (_poolCueScript.shootAvailable == true)
            {
                InputManager.Instance.SwitchCurrentActionMap("UI");
                _powerUpUIScript.HighlightSlotButton();
            }
        }
    }
    public void SwitchToGamePlayState(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _powerUpUIScript.HidePUConflictUI();//this function closes ui and switches input state to gameplay
        }
    }
    public void ToggleControls(InputAction.CallbackContext context)
    {
        //Toggle between showing controls and hiding
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
