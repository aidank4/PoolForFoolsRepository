using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    private Rigidbody _poolStickRb;
    private Rigidbody _cueBallRb;


    //Get reference to cue ball so we can hit it ---- must drag cue ball in through the inspector
    [SerializeField] private GameObject _cueBall;

    //for Shooting
    [SerializeField] private float _defaultShotSpeed = 5f;
    private float _ballRadius;

    //Detect hold
    private bool _holdStarted = false;
    private float _holdTimer;
    private float _maxHold = 3f; // 3 for the seconds of time a player can hold down shoot at max

    //private Vector2 _rotateInput;



    private void Awake()
    {
        _poolStickRb = GetComponent<Rigidbody>();
        _cueBallRb = _cueBall.GetComponent<Rigidbody>();

        //used for shoot math but the radius only needs to be gathered once so I have it here in Awake.
        _ballRadius = _cueBall.GetComponent<SphereCollider>().radius;
    }



    // IMPORTANT - this function is using unity events from the inspector,
    // so if the function is updated it might need to be re-entered in the inspector through the input action
    public void Shoot(InputAction.CallbackContext context) // get context so we can manipulate what stage of shoot things happen
    {
        if (context.started) // detect when click started so that we can timestamp it for the hold duration
        {
            //Prepare the hold timer
            _holdTimer = 0;
            _holdStarted = true;

            Debug.Log("Holding");
        }

        if (context.canceled) // canceled is the phase when the player releases input
        {
            _holdStarted = false;
            // Implement Close visual

            if (_holdTimer > _maxHold) //max out power on shot by not allowing more than 3 second hold
            {
                _holdTimer = _maxHold;
            }

            float shotForce = _defaultShotSpeed * _holdTimer;
            Debug.Log("Hold power multiplier: " + _holdTimer);

            //direction of shot
            Vector3 shotDir = new Vector3(0, 0f, 1); // Changing these values impacts shot direction, raising the y makes ball chip


            //calculate where to hit the ball
            Vector3 hitPos = _cueBall.transform.position - (shotDir * _ballRadius); // Perfect center of the ball no added spin
            Vector3 hitPosSpin = new Vector3(hitPos.x, hitPos.y - 1, hitPos.z); // Change these values to affect spin
            
            _cueBallRb.AddForceAtPosition(shotDir * shotForce, hitPosSpin, ForceMode.Impulse); // Impulse gives all force at once - realistic for pool

            Debug.Log("Shoot");
        }

    }

    /*
    public void Rotate(InputAction.CallbackContext context)
    {
        _rotateInput = context.ReadValue<Vector2>();
    }
    */

    private void Update()
    {
        //_poolStickRb.MoveRotation());

        if (_holdStarted)
        {
            //progressBar.SetActive(true) open visual

            _holdTimer += Time.deltaTime;
        }
    }
}

