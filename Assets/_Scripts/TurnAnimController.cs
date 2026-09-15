using UnityEngine;

public class TurnAnimController : MonoBehaviour
{
    [SerializeField] private GameObject _turnSwitchUI;


    //this function is called in an animation event
    public void OnAnimationDone()
    {
        _turnSwitchUI.SetActive(false);
    }
}
