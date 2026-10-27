using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Image _sabotageOverlay; //SabotageUI
    [SerializeField] private Image _sabotageIconUI;
    [SerializeField] private Image _sabotageCorner;
    [SerializeField] private TMP_Text _sabotageText;

    [Header("Power Ups")]
    [SerializeField]private List<Sprite> _sIcons = new List<Sprite>();
    [SerializeField]private List<GameObject> _sIconHolder = new List<GameObject>();

    private void Awake()
    {
        //Ref
        _turnManager = _turnManagerRef.GetComponent<TurnManager>();
        //UI
        #region
        HidePUConflictUI();
        _powerUpIcon.gameObject.SetActive(false);
        _sabotageOverlay.gameObject.SetActive(false);
        _sabotageIconUI.gameObject.SetActive(false);
        _sabotageCorner.gameObject.SetActive(false);
        _sabotageText.gameObject.SetActive(false);
        #endregion
    }


    private void OnEnable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched += HandleTurnSwitch;
        }
        SO_PowerUp.UsePU += HandlePU;
    }

    private void OnDisable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched -= HandleTurnSwitch;
        }
        SO_PowerUp.UsePU -= HandlePU;
    }



    /// <summary>
    /// Set up UI for when there is an extra PU and set controls to UI.
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
        EventSystem.current.SetSelectedGameObject(_slotA.gameObject); // set to the swapbutton
    }


    /// <summary>
    /// Set selected object to be null
    /// </summary>
    public void ResetButtonHighlight()
    {
        EventSystem.current.SetSelectedGameObject(null); //clear if any other object is being "selected"
    }


    /// <summary>
    /// Show UI displaying the player has been sabotaged.
    /// </summary>
    /// <param name="icons"></param>
    /// <returns></returns>
    private IEnumerator SabotageUI(Sprite[] icons)
    {
        _sabotageOverlay.gameObject.SetActive(true); //middle UI On
        _sabotageIconUI.gameObject.SetActive(true);
        foreach (Sprite icon in icons) //Place Icons in center
        {
            GameObject obj = new GameObject("icon");
            Image objImage = obj.AddComponent<Image>();
            objImage.sprite = icon;
            obj.transform.SetParent(_sabotageIconUI.gameObject.transform, false);
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 0);
            _sIconHolder.Add(obj);
        }
        yield return new WaitForSeconds(1.5f);
        _sabotageOverlay.gameObject.SetActive(false);
        _sabotageIconUI.gameObject.SetActive(false);

        _sabotageCorner.gameObject.SetActive(true);//Corner UI On
        _sabotageText.gameObject.SetActive(true);
        foreach (GameObject obj in _sIconHolder) //Place icons in corner
        {
            obj.transform.SetParent(_sabotageCorner.gameObject.transform, false);
            RectTransform rectTransform = obj.GetComponent<RectTransform>();
            rectTransform.anchoredPosition = new Vector2(0, 0);
        }
    }

    /// <summary>
    /// Hide remaining sabotage UI and reset list of icons
    /// </summary>
    public void ResetSabotageUI()
    {
        _sabotageCorner.gameObject.SetActive(false);
        _sabotageText.gameObject.SetActive(false);
        foreach (GameObject obj in _sIconHolder)
        {
            Destroy(obj);
        }
        _sIconHolder.Clear();
    }


    /// <summary>
    /// On Turn Switch Open sabotage UI if sabotaged. After clear the icons.
    /// </summary>
    /// <param name="sabotaged"></param>
    private void HandleTurnSwitch(bool sabotaged)
    {
        if (sabotaged)
        {
            StartCoroutine(SabotageUI(_sIcons.ToArray()));
            _sIcons.Clear();
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
            _sIcons.Add(powerUp.icon);
        }
    }
}
