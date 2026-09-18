using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Rigidbody _poolCueRb;
    private Rigidbody _cueBallRb;


    //Get reference to cue ball so we can hit it ---- must drag cue ball in through the inspector
    [SerializeField] private GameObject _cueBall;
    private GameObject _poolCue;

    //for Shooting
    public bool shootAvailable = true;
    [SerializeField] private float _defaultShotSpeed = 5f;
    private float _ballRadius;

    //Detect hold
    private bool _holdStarted = false;
    private float _holdTimer;
    private float _maxHold = 3f; // 3 for the seconds of time a player can hold down shoot at max

    //rotation of pool cue
    private Vector2 _rotateInput;
    [SerializeField] private float _rotationSpeed = 50f;

    
    [SerializeField] Scrollbar _chargeVisual;


    private void Awake()
    {
        _poolCueRb = GetComponent<Rigidbody>();
        _cueBallRb = _cueBall.GetComponent<Rigidbody>();
        _poolCue = this.gameObject;

        //used for shoot math but the radius only needs to be gathered once so I have it here in Awake.
        _ballRadius = _cueBall.GetComponent<SphereCollider>().radius;

        _chargeVisual.gameObject.SetActive(false); //initialize visual as closed

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

            // set up the visual indicator
            _chargeVisual.gameObject.SetActive(true);
            _chargeVisual.size = _holdTimer;


            Debug.Log("Holding");
        }

        if (context.canceled) // canceled is the phase when the player releases input
        {
            //can't shoot right after
            //shootAvailable = false;

            _holdStarted = false;
            _chargeVisual.gameObject.SetActive(false);
            //_poolCue.gameObject.SetActive(false);

            if (_holdTimer > _maxHold) //max out power on shot by not allowing more than 3 second hold
            {
                _holdTimer = _maxHold;
            }

            float shotForce = _defaultShotSpeed * _holdTimer;
            Debug.Log("Hold power multiplier: " + _holdTimer);

            //direction of shot
            Vector3 shotDir = _poolCue.transform.up; // transform.up because y axis(green) is forward on pool cue prefab

            //calculate where to hit the ball
            Vector3 hitPos = _cueBall.transform.position - (shotDir * _ballRadius); // Perfect center of the ball no added spin
            Vector3 hitPosSpin = new Vector3(hitPos.x, hitPos.y - 1, hitPos.z); // Change these values to affect spin
            
            _cueBallRb.AddForceAtPosition(shotDir * shotForce, hitPosSpin, ForceMode.Impulse); // Impulse gives all force at once - realistic for pool

            Debug.Log("Shoot");
            Debug.Log(_rotateInput);
        }

    }

    
    public void Rotate(InputAction.CallbackContext context)
    {
        _rotateInput = context.ReadValue<Vector2>();
        Debug.Log("Rotating");
    }
    

    private void Update()
    {
        _poolCue.transform.RotateAround(_cueBall.transform.position, _rotateInput, _rotationSpeed * Time.deltaTime);

        if (_holdStarted)
        {
            _holdTimer += Time.deltaTime;
            _chargeVisual.size = (_holdTimer / _maxHold); // divide by _maxHold because scrollBarSize is capped to 1.
        }
    }
}

