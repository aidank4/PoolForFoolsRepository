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

    //Get button so that we can add in arrow key selction
    [SerializeField] private Button _swapButton; // do the swap button bc this will be first highlighted button and we dont want that to be toss


    private void Awake()
    {
        HidePUConflictUI();
        _powerUpIcon.gameObject.SetActive(false);
    }

    public void ShowPUConflictUI(SO_PowerUp powerUp)
    {
        _conflictIcon.sprite = powerUp.icon;
        _descriptionText.text = powerUp.description;
        _powerUpConflictUI.gameObject.SetActive(true);
        HighlightButton();
    }
    public void HidePUConflictUI()
    {
        _powerUpConflictUI.gameObject.SetActive(false);
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

    private void HighlightButton()
    {
        EventSystem.current.SetSelectedGameObject(null); //clear if any other object is being "selected"
        EventSystem.current.SetSelectedGameObject(_swapButton.gameObject); // set to the swapbutton
    }
}
