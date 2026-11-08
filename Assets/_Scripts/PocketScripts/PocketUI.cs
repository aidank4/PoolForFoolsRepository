using UnityEngine;

public class PocketUI : MonoBehaviour
{
    [SerializeField] private GameObject uiHolder;

    private void Awake()
    {
        uiHolder.SetActive(false);
    }

    /// <summary>
    /// Show last pocket UI
    /// </summary>
    public void Activate()
    {
        uiHolder.SetActive(true);
    }

    /// <summary>
    /// Hide last pocket UI
    /// </summary>
    public void Close()
    {
        uiHolder.SetActive(false);
    }
}
