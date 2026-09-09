using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private Rigidbody _cueBallRb;


    //Get reference to cue ball so we can hit it ---- must drag cue ball in through the inspector
    [SerializeField] private GameObject _cueBall;
    private GameObject _poolCueModel;
    private GameObject _poolCueParent;

    //for Shooting
    private bool _shootAvailable = true; //Can't start shooting without this bool
    private bool _shooting = false; // Can't release shot without this bool - Without this there is an error where you can release shot before starting shot

    [SerializeField] private float _defaultShotSpeed = 5f;
    private float _ballRadius;
    [SerializeField] private float _shootWaitTime = 5f;


    //Pool cue moving back
    [SerializeField] private float _pullBackSpeed = 1f;

    // cancel shot
    private bool _stopShot = false;

    //Detect hold
    private bool _holdStarted = false;
    private float _holdTimer;
    [SerializeField] private float _maxHold = 3f; // 3 for the seconds of time a player can hold down shoot at max

    //rotation of pool cue
    private Vector2 _rotateInput;
    [SerializeField] private float _rotationSpeed = 50f;

    
    [SerializeField] Scrollbar _chargeVisual;

    [SerializeField] private float _ballSeparation = 1.5f;


    private void Awake()
    {
        _cueBallRb = _cueBall.GetComponent<Rigidbody>();
        _poolCueModel = GameObject.FindWithTag("PoolCue"); //need the model as we only want to turn it off visually
        _poolCueParent = this.gameObject;

        //used for shoot math but the radius only needs to be gathered once so I have it here in Awake.
        _ballRadius = _cueBall.GetComponent<SphereCollider>().radius;

        _chargeVisual.gameObject.SetActive(false); //initialize visual as closed
    }



    // IMPORTANT - this function is using unity events from the inspector,
    // so if the function is updated it might need to be re-entered in the inspector through the input action
    public void Shoot(InputAction.CallbackContext context) // get context so we can manipulate what stage of shoot things happen
    {
        if (context.started && _shootAvailable) // detect when click started so that we can timestamp it for the hold duration
        {
            _shooting = true;
            _stopShot = false;
            
            //Prepare the hold timer
            _holdTimer = 0;
            _holdStarted = true;

            // set up the visual indicator
            _chargeVisual.gameObject.SetActive(true);
            _chargeVisual.size = _holdTimer;


            Debug.Log("Holding");
        }

        if (context.canceled && _shooting && !_stopShot) // canceled is the phase when the player releases input
        {
            
            _shooting = false;
            _holdStarted = false;

            StartCoroutine(ShootDelay());

            _chargeVisual.gameObject.SetActive(false);
            //_poolCueModel.gameObject.SetActive(false);


            CalculateShot();
        }

    }

    //Using Unity events
    public void CancelShot(InputAction.CallbackContext context)
    {
        _stopShot = true;
        Debug.Log("Canceled");

        //reset charge
        _holdTimer = 0;
        _chargeVisual.gameObject.SetActive(false);
    }

    //Using Unity events
    public void Rotate(InputAction.CallbackContext context)
    {
        _rotateInput = context.ReadValue<Vector2>();
        Debug.Log("Rotating");
    }



    //Turn pool cue back on
    private void RespawnPoolCue()
    {
        _poolCueModel.gameObject.SetActive(true);
        _poolCueParent.transform.position = _cueBall.transform.position - new Vector3(0f, 0f, _ballSeparation);

        //eulerAngles is just taking in the inspector values and making it quaternion
        _poolCueParent.transform.eulerAngles = new Vector3(90, 0, 0);
    }



    /// <summary>
    /// Calculate shot power, spin and direction, then launch the ball
    /// </summary>
    private void CalculateShot()
    {
     
        if (_holdTimer > _maxHold) //max out power on shot by not allowing more than 3 second hold
        {
            _holdTimer = _maxHold;
        }

        float shotForce = _defaultShotSpeed * _holdTimer;
        Debug.Log("Hold power multiplier: " + _holdTimer);

        //direction of shot
        Vector3 shotDir = _poolCueParent.transform.up; // transform.up because y axis(green) is forward on pool cue prefab

        //calculate where to hit the ball
        Vector3 hitPos = _cueBall.transform.position - (shotDir * _ballRadius); // Perfect center of the ball no added spin
        Vector3 hitPosSpin = new Vector3(hitPos.x, hitPos.y - 1, hitPos.z); // Change these values to affect spin

        //snap pool cue forward to "hit the ball"
        _poolCueParent.transform.position = hitPos;

        _cueBallRb.AddForceAtPosition(shotDir * shotForce, hitPosSpin, ForceMode.Impulse); // Impulse gives all force at once - realistic for pool
    }
    

    private void Update()
    {
        _poolCueParent.transform.RotateAround(_cueBall.transform.position, _rotateInput, _rotationSpeed * Time.deltaTime);


        if (_holdStarted && !_stopShot)
        {
            _holdTimer += Time.deltaTime;
            _chargeVisual.size = (_holdTimer / _maxHold); // divide by _maxHold because scrollBarSize is capped to 1.

            //dont let the pool cue pull back visual go further than max power
            if (_holdTimer < _maxHold)
            {
                _poolCueParent.transform.position += -_poolCueParent.transform.up * _pullBackSpeed * Time.deltaTime;
            }
        }
    }

    IEnumerator ShootDelay()
    {
        _shootAvailable = false;

        //Leave pool cue up for a tenth of a second to show it hitting the ball;
        yield return new WaitForSeconds(0.1f);
        _poolCueModel.gameObject.SetActive(false);

        //Delay until player can shoot again
        yield return new WaitForSeconds(_shootWaitTime);

        _shootAvailable = true;

        RespawnPoolCue();

    }
}

