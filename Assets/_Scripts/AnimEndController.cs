using System;
using UnityEngine;

public class AnimEndController : MonoBehaviour
{

    //this function is called in an animation event
    public void AnimationComplete()
    {
        this.gameObject.SetActive(false);
    }
}
