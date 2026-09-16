using UnityEngine;

public class TurnAnimController : MonoBehaviour
{

    //this function is called in an animation event
    public void OnAnimationDone()
    {
        this.gameObject.SetActive(false);
    }
}
