using System;
using UnityEngine;

public class DartAnimEndController : MonoBehaviour
{

    public event Action OnAnimationComplete;

    //this function is called in an animation event
    public void TriggerAnimationOver()
    {
        OnAnimationComplete?.Invoke();
        this.gameObject.SetActive(false);
    }
}
