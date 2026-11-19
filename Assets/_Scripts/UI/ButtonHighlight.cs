using System.Collections;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;


public class ButtonHighlight : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [SerializeField] private GameObject _star;
    [SerializeField] private float xOffset;
    [SerializeField] private float yOffset;

    private void Awake()
    {
        _star.SetActive(false);
    }


    /// <summary>
    /// On UI hover reparent star vfx to UI.
    /// Place it in the bottom right corner with custom offset
    /// </summary>
    /// <param name="data"></param>
    public void OnSelect(BaseEventData data)
    {
        _star.transform.SetParent(this.transform, false); //Set parent to this button

        RectTransform rect = _star.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(xOffset, yOffset);
        if (!_star.activeSelf) //Not active
        {
            _star.SetActive(true);
        }
    }

    public void OnDeselect(BaseEventData data)
    {
        StartCoroutine(HideStar());
    }


    /// <summary>
    /// Wait for the end of frame and if nothing is selected then hide Star
    /// </summary>
    /// <returns></returns>
    private IEnumerator HideStar()
    {
        yield return new WaitForEndOfFrame();
        if (EventSystem.current.currentSelectedGameObject == null) //nothing selected
        {
            if (_star.activeSelf)
            {
                _star.SetActive(false);
            }
        }
    }
}
