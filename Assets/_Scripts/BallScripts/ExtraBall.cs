using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ExtraBall : MonoBehaviour
{
    //NOTE ExtraBall differs from ball by destroying the balls once stopped
    [Header("References")]
    private Player _poolCueScript;


    [Header("Misc Variables")]
    private Rigidbody _ballRb;
    private float _minSpeed = 0.2f;
    private bool _canDestroy = false;


    private void Start()
    {
        //ref
        _ballRb = GetComponent<Rigidbody>();

        StartCoroutine(MovementCheck());
    }

    public void Access(Player poolCueScript)
    {
        _poolCueScript = poolCueScript;
    }

    /// <summary>
    /// Runs infinitely at 0.5 second intervals.
    /// Checks if the extra ball is under a certain speed.
    /// If so it destroys the ball
    /// </summary>
    /// <returns></returns>
    IEnumerator MovementCheck()
    {
        if (!_poolCueScript.shootAvailable) //Can't destroy until player has shot
        {
            _canDestroy = true;
        }
        if (_ballRb.linearVelocity != Vector3.zero) // checks if ball is moving
        {
            if (_ballRb.linearVelocity.sqrMagnitude < _minSpeed && _canDestroy)
            {
                Destroy(this.gameObject);
            }
        }
        //run infinitely like a slow update
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(MovementCheck());
    }


    /// <summary>
    /// If the extra ball hits a pocket the extra ball destroys unless player hasnt shot yet
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "CornerPocketA" || other.gameObject.tag == "CornerPocketB" || other.gameObject.tag == "CornerPocketC" ||
            other.gameObject.tag == "CornerPocketD" || other.gameObject.tag == "SidePocketA" || other.gameObject.tag == "SidePocketB" && !_poolCueScript.shootAvailable)
        {
            Destroy(this.gameObject);
        }
    }
}
