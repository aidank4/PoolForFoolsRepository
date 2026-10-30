using System;
using UnityEngine;

public class DartAnimEndController : MonoBehaviour
{
    public event Action OnAnimationComplete;

    public void TriggerAnimationOver() //this function is called in an animation event
    {
        OnAnimationComplete?.Invoke();
        this.gameObject.SetActive(false);
    }
}
