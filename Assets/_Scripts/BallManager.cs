using UnityEngine;

public class BallManager : MonoBehaviour
{
    /// <summary>
    /// ISSUE - not getting the rigidbodies into the array must test
    /// </summary>
    protected float minSpeed = 0.2f;
    private float _timer;
    private float _minWaitTime = 0.5f;
    
    [SerializeField] private Rigidbody[] _balls;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _balls = GetComponentsInChildren<Rigidbody>();
    }

    
    // Update is called once per frame
    private void FixedUpdate()
    {
        bool ballsStopped = true;
        foreach (Rigidbody ball in _balls)
        {
            //every check of a ball make ballsStill false unless they are still

            if (ball.linearVelocity.sqrMagnitude > minSpeed)
            {
                ballsStopped = false;
            }
            if (ball.angularVelocity.sqrMagnitude > minSpeed)
            {
                ballsStopped = false;
            }
        }

        //if balls are still wait a bit in case it was just a split second
        if (ballsStopped)
        {
            _timer += Time.fixedDeltaTime;
            if (_timer >= _minWaitTime)
            {
                Debug.Log("All Balls are still");
                //_shootAvailable = true;
            }
        }
        else
        {
            _timer = 0;
        }
    }
}
