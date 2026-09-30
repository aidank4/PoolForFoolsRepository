using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Ball : MonoBehaviour
{
    private Rigidbody _ballRb;
    private float _minSpeed = 0.2f;



    private void Awake()
    {
        _ballRb = GetComponent<Rigidbody>();
        StartCoroutine(MovementCheck());
    }


    /// <summary>
    /// Runs infinitely at 0.5 second intervals.
    /// Checks if a ball is under a certain speed.
    /// If so it stops the ball
    /// </summary>
    /// <returns></returns>
    IEnumerator MovementCheck()
    {

        if (_ballRb.linearVelocity != Vector3.zero) // checks if ball is moving
        {
            if (_ballRb.linearVelocity.sqrMagnitude < _minSpeed)
            {
                // Stop the balls from moving and spinning manually so we aren't waiting on the physics system.
                _ballRb.linearVelocity = Vector3.zero;
                _ballRb.angularVelocity = Vector3.zero;
            }
        }
        //run infinitely like a slow update
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(MovementCheck());
    }
}
