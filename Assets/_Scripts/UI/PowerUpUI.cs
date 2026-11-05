using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManager;
    [SerializeField] private GameObject _powerUpConflictUI;
    [SerializeField] private Image _powerUpIcon;
    [SerializeField] private Image _conflictIcon;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _swapButton; // do the swap button bc this will be first highlighted button and we dont want that to be toss
    [SerializeField] private Button _slotA; // when swap gets clicked highlight slotA
    [SerializeField] private TMP_Text _slotDescription;


    [Header("Sabotage UI")]
    [SerializeField] private Image _sabotageOverlay;
    [SerializeField] private Image _sabotageBorderUI;
    [SerializeField] private GameObject _sabotageBorderParent;
    [SerializeField] private Image _sabotageCorner;
    [SerializeField] private TMP_Text _sabotageText;
    private List<SO_PowerUp> _sPowerUps = new List<SO_PowerUp>(); //Sabotage Icons and descriptions that will appear
    private List<GameObject> _sIconsHolder = new List<GameObject>(); //Game Object to hold those icons
    private List<GameObject> _sDescriptionsHolder = new List<GameObject>(); //Game Object to hold those descriptions
    private List<Image> _sBorders = new List<Image>();

    private void Awake()
    {
        //Ref
        _turnManager = _turnManagerRef.GetComponent<TurnManager>();
        //UI
        #region
        _powerUpConflictUI.gameObject.SetActive(false);
        _slotDescription.transform.parent.gameObject.SetActive(false);
        ResetButtonHighlight();
        _powerUpIcon.gameObject.SetActive(false);
        _sabotageOverlay.gameObject.SetActive(false);
        _sabotageBorderUI.gameObject.SetActive(false);
        _sabotageBorderParent.gameObject.SetActive(false);
        _sabotageCorner.gameObject.SetActive(false);
        _sabotageText.gameObject.SetActive(false);
        #endregion
    }


    /// <summary>
    /// Subscribe to events
    /// </summary>
    private void OnEnable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched += HandleTurnSwitch;
        }
        SO_PowerUp.UsePU += HandlePU;
    }
    

    /// <summary>
    /// Unsubscribe
    /// </summary>
    private void OnDisable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched -= HandleTurnSwitch;
        }
        SO_PowerUp.UsePU -= HandlePU;
    }



    /// <summary>
    /// Set up UI for when there is an extra PU to swap and set controls to UI.
    /// </summary>
    /// <param name="powerUp"></param>
    public void ShowPUConflictUI(SO_PowerUp powerUp) 
    {
        _conflictIcon.sprite = powerUp.icon;
        _descriptionText.text = powerUp.description;
        _nameText.text = powerUp.id;
        _powerUpConflictUI.gameObject.SetActive(true);

        InputManager.Instance.SwitchCurrentActionMap("UI"); // switch input so it controls ui not gameplay
        HighlightSwapButton();
    }


    /// <summary>
    /// Hide the extra PU UI and set controls back to gameplay
    /// </summary>
    public void HidePUConflictUI()
    {
        _powerUpConflictUI.gameObject.SetActive(false);
        _slotDescription.transform.parent.gameObject.SetActive(false);
        ResetButtonHighlight();
        InputManager.Instance.SwitchCurrentActionMap("Gameplay"); //switch inputs back to gameplay
    }


    /// <summary>
    /// Display a PU icon for 1 second to show the player they earned a PU.
    /// </summary>
    /// <param name="powerUp"></param>
    /// <returns></returns>
    public IEnumerator DisplayPowerUpIcon(SO_PowerUp powerUp)
    {
        _powerUpIcon.sprite = powerUp.icon;
        _powerUpIcon.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        _powerUpIcon.gameObject.SetActive(false);

    }


    /// <summary>
    /// Set selected object to swap button
    /// </summary>
    private void HighlightSwapButton()
    {
        ResetButtonHighlight();
        EventSystem.current.SetSelectedGameObject(_swapButton.gameObject); // set to the swapbutton
    }


    /// <summary>
    /// Set selected object to an inventory slot button
    /// </summary>
    public void HighlightSlotButton()
    {
        ResetButtonHighlight();
        EventSystem.current.SetSelectedGameObject(_slotA.gameObject); // set to a slot
    }


    /// <summary>
    /// Set selected object to be null
    /// </summary>
    public void ResetButtonHighlight()
    {
        EventSystem.current.SetSelectedGameObject(null); //clear if any other object is being "selected"
    }


    /// <summary>
    /// Hide center sabotage UI and move it to the corner
    /// </summary>
    public void CornerSabotageUI()
    {
        //Hide center sabotage UI
        _sabotageOverlay.gameObject.SetActive(false);
        _sabotageBorderParent.gameObject.SetActive(false);

        //Activate corner UI
        _sabotageCorner.gameObject.SetActive(true);
        _sabotageText.gameObject.SetActive(true);

        foreach (GameObject obj in _sIconsHolder) //Place icons in corner
        {
            obj.transform.SetParent(_sabotageCorner.gameObject.transform, false);
        }
    }


    /// <summary>
    /// Show UI displaying the player has been sabotaged.
    /// </summary>
    /// <param name="icons"></param>
    /// <returns></returns>
    private void CenterSabotageUI(SO_PowerUp[] powerUps)
    {
        //Activate UI
        _sabotageOverlay.gameObject.SetActive(true);
        _sabotageBorderUI.gameObject.SetActive(true);
        _sabotageBorderParent.gameObject.SetActive(true);

        _sBorders.Add(_sabotageBorderUI);
        for (int i = 1; i < powerUps.Length; i++) //Duplicate UI objects if there is more than one sabotage active
        {
            Image dupeBorder = Instantiate(_sabotageBorderUI);
            dupeBorder.transform.SetParent(_sabotageBorderParent.transform, false);
            _sBorders.Add(dupeBorder);
        }

        for (int i = 0; i < _sBorders.Count; i++) //Place UI in center
        {
            SO_PowerUp powerUp = powerUps[i];
            //Create object with an image component
            GameObject iObj = new GameObject("powerUpIcon");
            Image iImage = iObj.AddComponent<Image>();
            iImage.sprite = powerUp.icon; //asiign image to sabotage icon
            iObj.transform.SetParent(_sBorders[i].transform.GetChild(0), false); //0 is icon parent - 1 is desc parent 


            //Create object with a text component
            GameObject dObj = new GameObject("powerUpDescription");
            TextMeshProUGUI dText = dObj.AddComponent<TextMeshProUGUI>();
            dText.text = powerUp.sabDescription; //Assign text to sabotage description
            dText.alignment = TextAlignmentOptions.Center;
            dObj.transform.SetParent(_sBorders[i].transform.GetChild(1), false); //1 is desc parent - 0 is icon parent


            //save reference
            _sIconsHolder.Add(iObj);
            _sDescriptionsHolder.Add(dObj);
        }
        InputManager.Instance.SwitchCurrentActionMap("SabotageUI");
    }


    /// <summary>
    /// Hide remaining sabotage UI and reset lists
    /// </summary>
    public void ResetSabotageUI()
    {
        _sabotageCorner.gameObject.SetActive(false); //Hide corner sabotage UI
        _sabotageText.gameObject.SetActive(false);

        //Clear all the children
        foreach (GameObject obj in _sIconsHolder)
        {
            Destroy(obj);
        }
        foreach (GameObject obj in _sDescriptionsHolder)
        {
            Destroy(obj);
        }
        for (int i = 1; i < _sBorders.Count; i ++) // start at 1 to skip first object - never destroy original
        {
            Destroy(_sBorders[i].gameObject); // destroy all clones
        }
        _sIconsHolder.Clear();
        _sDescriptionsHolder.Clear();
        _sBorders.Clear();
    }


    /// <summary>
    /// On Turn Switch Open sabotage UI if sabotaged. After clear the icons.
    /// </summary>
    /// <param name="sabotaged"></param>
    private void HandleTurnSwitch(bool sabotaged)
    {
        if (sabotaged)
        {
            CenterSabotageUI(_sPowerUps.ToArray());
            _sPowerUps.Clear();
        }
    }


    /// <summary>
    /// if sabotaged add the powerup icon to collection for UI
    /// </summary>
    /// <param name="powerUp"></param>
    private void HandlePU(SO_PowerUp powerUp)
    {
        if (powerUp.affectNext)
        {
            _sPowerUps.Add(powerUp);
        }
    }
}
