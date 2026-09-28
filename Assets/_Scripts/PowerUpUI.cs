using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PowerUpUI : MonoBehaviour
{
    [SerializeField] private GameObject _powerUpConflictUI;
    [SerializeField] private Image _powerUpIcon;
    [SerializeField] private Image _conflictIcon;
    [SerializeField] private TMP_Text _descriptionText;
    [SerializeField] private TMP_Text _nameText;

    //Get button so that we can add in arrow key selction
    [SerializeField] private Button _swapButton; // do the swap button bc this will be first highlighted button and we dont want that to be toss
    [SerializeField] private Button _slotA; // when swap gets clicked highlight slotA
    [SerializeField] private TMP_Text _slotDescription;


    private void Start()
    {
        HidePUConflictUI();
        _powerUpIcon.gameObject.SetActive(false);
    }

    public void ShowPUConflictUI(SO_PowerUp powerUp)
    {
        //Set UI
        _conflictIcon.sprite = powerUp.icon;
        _descriptionText.text = powerUp.description;
        _nameText.text = powerUp.id;
        _powerUpConflictUI.gameObject.SetActive(true);

        //Set UI Inputs
        InputManager.Instance.SwitchCurrentActionMap("UI"); // switch input so it controls ui not gameplay
        HighlightSwapButton();
    }
    public void HidePUConflictUI()
    {
        _powerUpConflictUI.gameObject.SetActive(false);
        _slotDescription.transform.parent.gameObject.SetActive(false);
        ResetButtonHighlight();
        InputManager.Instance.SwitchCurrentActionMap("Gameplay"); //switch inputs back to gameplay
    }
    public IEnumerator DisplayPowerUpIcon(SO_PowerUp powerUp)
    {
        //assign sprite and make visible
        _powerUpIcon.sprite = powerUp.icon;
        _powerUpIcon.gameObject.SetActive(true);

        //show for 1 second
        yield return new WaitForSeconds(1f);
        _powerUpIcon.gameObject.SetActive(false);

    }

    private void HighlightSwapButton()
    {
        EventSystem.current.SetSelectedGameObject(null); //clear if any other object is being "selected"
        EventSystem.current.SetSelectedGameObject(_swapButton.gameObject); // set to the swapbutton
    }

    public void HighlightSlotButton()
    {
        EventSystem.current.SetSelectedGameObject(null); //clear if any other object is being "selected"
        EventSystem.current.SetSelectedGameObject(_slotA.gameObject); // set to the swapbutton
    }
    public void ResetButtonHighlight()
    {
        EventSystem.current.SetSelectedGameObject(null); //clear if any other object is being "selected"
    }
}
