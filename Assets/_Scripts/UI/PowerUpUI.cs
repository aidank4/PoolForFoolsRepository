using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _powerUpConflictUI;
    [SerializeField] private Image _powerUpIcon;
    [SerializeField] private Image _conflictIcon;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private Button _swapButton; // do the swap button bc this will be first highlighted button and we dont want that to be toss
    [SerializeField] private Button _slotA; // when swap gets clicked highlight slotA
    [SerializeField] private TMP_Text _slotDescription;


    private void Start()
    {
        //UI
        #region
        HidePUConflictUI();
        _powerUpIcon.gameObject.SetActive(false);
        #endregion
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
}
