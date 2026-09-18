using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody _ballRb;

    private float _minSpeed = 0.2f;

    //private bool _moving = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _ballRb = GetComponent<Rigidbody>();

        StartCoroutine(MovementCheck());
    }


    /// <summary>
    /// When balls collide they are very slow for a split second.
    /// cant just check if the velocity is slow or they will stop on impact
    /// have to wait after impact then check
    /// this coroutine calls itself every 0.5 seconds
    /// 
    /// Replaces update as update
    /// </summary>
    /// <returns></returns>
    IEnumerator MovementCheck()
    {

        if (_ballRb.linearVelocity != Vector3.zero) // checks if ball has been hit and is moving
        {
            yield return new WaitForSeconds(2);

            //after 2 seconds check if they are close to stopping
            if (_ballRb.linearVelocity.sqrMagnitude < _minSpeed)
            {
                // Stop the balls from moving and spinning manually so we aren't waiting on the physics system.
                _ballRb.linearVelocity = Vector3.zero;
                _ballRb.angularVelocity = Vector3.zero;

                //below comment is old conditional
                //_ballRb.linearVelocity.x<_minSpeed && _ballRb.linearVelocity.z < _minSpeed

                // if the ball was still moving quickly the if statement will check again in 0.5 seconds
            }
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(MovementCheck());
    }
}
