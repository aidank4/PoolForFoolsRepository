using System;
using UnityEngine;

public class AnimEndController : MonoBehaviour
{
    public void AnimationComplete() //this function is called in an animation event
    {
        this.gameObject.SetActive(false);
    }
}
