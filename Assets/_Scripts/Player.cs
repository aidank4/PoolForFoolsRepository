using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;
    [SerializeField] private GameObject _predictionLineParent;
    [SerializeField] Scrollbar _chargeVisual;
    [SerializeField] private GameObject _cueBall;
    [SerializeField] private GameObject _cueBallExtra;
    [SerializeField] private GameObject _poolCueModel;
    public GameObject poolCueParent { get; private set; }


    [Header("Modifiable Shot Mechanics")]
    [SerializeField] private float _defaultShotSpeed = 5f;
    [SerializeField] private float _maxCharge = 1f; // 1 for the seconds of time a player can hold down shoot at max
    [SerializeField] private float _pullBackSpeed = 1f;
    [SerializeField] private float _rotationSpeed = 5f;
    [SerializeField] private float _ballSeparation = 0.5f;
    [SerializeField] private float _rotationAccel = 15f;
    [SerializeField] private float _startingRotMult;
    private float _rotationMult = 1f;


    [Header("PowerUp Variables")]
    [SerializeField] private float _multiBallShotAngle = 10;
    [SerializeField] private float _rocketShotForce = 10f;
    [SerializeField] private float _minCharge =  0.1f;
    [SerializeField] private float _rdmChargeSpeed =  5f;
    private GameObject _extraCueBallA;
    private GameObject _extraCueBallB;
    private GameObject _extraLineA;
    private GameObject _extraLineB;
    private bool _rdmPowerPU = false;
    private bool _curvedShot = false;


    [Header("Bools")]
    public bool shootAvailable = true; //Can't start shooting without this bool // accessed and modified by turn manager - accessed by player controls
    private bool _shooting = false; //Can't release shot without this bool - Without this there is an error where you can release shot before starting shot
    private bool _stopShot = false; //Set true by CancelShot() and false when shot started
    private bool _holdStarted = false; //Set true when shot started and false when shot released


    [Header("Misc Variables")]
    private float _ballRadius;
    private float _holdTimer;
    private Vector3 _startingPos; // when shot canceled need to remember where pool cue was to reset its position
    private Vector2 _rotateInput;
    private float _tempRotateInput;
    private bool _holding = false;


    //ENUMS
    public enum ClickStage { Pressed, Released }


    //EVENTS
    public event Action ShotCompleted; //Event for the ball manager to start checking to see for time to switch turn
    private event Action<ClickStage> Shoot; //Gets set to the correct shot type with power up usages



    private void Awake()
    {
        //ref
        #region
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
        poolCueParent = this.gameObject;
        #endregion
        //UI
        _chargeVisual.gameObject.SetActive(false);

        //Extra
        _ballRadius = _cueBall.GetComponent<SphereCollider>().radius; //used for shoot math but the radius only needs to be gathered once so I have it here in Awake.
        _predictionLineParent.transform.position = _cueBall.transform.position + new Vector3(0, 0, _ballSeparation); //set up prediction lines position and rotation
        _predictionLineParent.transform.eulerAngles = new Vector3(0, 0, 0);
        _startingRotMult = _rotationMult;
        Shoot = DefaultShoot;
    }


    private void OnEnable()
    {
        SO_PowerUp.UsePU += HandlePUs;
        if(_turnManagerScript != null)
        {
            _turnManagerScript.TurnSwitched += HandleTurnSwitch;
        }
    }

    private void OnDisable()
    {
        SO_PowerUp.UsePU -= HandlePUs;
        if (_turnManagerScript != null)
        {
            _turnManagerScript.TurnSwitched -= HandleTurnSwitch;
        }
    }


    private void Update()
    {
        //Rotate pool cue
        poolCueParent.transform.RotateAround(_cueBall.transform.position, new Vector3(0, _tempRotateInput, 0), (_rotationSpeed * _rotationMult) * Time.deltaTime);
        _predictionLineParent.transform.RotateAround(_cueBall.transform.position, new Vector3 (0, _tempRotateInput, 0), (_rotationSpeed * _rotationMult) * Time.deltaTime); //USING TEMP ROTATE
        if (_holding)
        {
            if (_rotationMult > 100)
            {
                _rotationMult = _startingRotMult;
            }
            _rotationMult += (_rotationAccel * Time.deltaTime);
        }
        
        if (Shoot == MultiShoot)
        {
            _extraCueBallA.transform.RotateAround(_cueBall.transform.position, new Vector3(0, _tempRotateInput, 0), _rotationSpeed * Time.deltaTime);
            _extraLineA.transform.RotateAround(_cueBall.transform.position, new Vector3(0, _tempRotateInput, 0), _rotationSpeed * Time.deltaTime);
            _extraCueBallB.transform.RotateAround(_cueBall.transform.position, new Vector3(0, _tempRotateInput, 0), _rotationSpeed * Time.deltaTime);
            _extraLineB.transform.RotateAround(_cueBall.transform.position, new Vector3(0, _tempRotateInput, 0), _rotationSpeed * Time.deltaTime);
        }//Rotate extra balls and lines when multi shoot active
        if (_holdStarted && !_stopShot)
        {
            _holdTimer += Time.deltaTime;
            _chargeVisual.size = (_holdTimer / _maxCharge); // divide by _maxHold because scrollBarSize is capped to 1.

            if (_holdTimer < _maxCharge) //dont let the pool cue pull back further than max power
            {
                poolCueParent.transform.position += poolCueParent.transform.right * _pullBackSpeed * Time.deltaTime; // .right is moving the cue back
            }
        }

    }


    /// <summary>
    /// Invokes Shoot and passes in the click stage
    /// </summary>
    /// <param name="context"></param>
    public void ShootInput(InputAction.CallbackContext context) //NOTE called through unity events in the input action
    {
        if (context.started)
        {
            Shoot?.Invoke(ClickStage.Pressed);
        }
        else if(context.canceled)
        {
            Shoot?.Invoke(ClickStage.Released);
        }
    }


    /// <summary>
    /// On click starts the charge and on release shoots the ball.
    /// </summary>
    /// <param name="stage"></param>
    private void DefaultShoot(ClickStage stage)
    {
        if (stage == ClickStage.Pressed && shootAvailable)
        {
            StartShot();
        }
        //On release
        if (stage == ClickStage.Released && _shooting && !_stopShot)
        {
            _shooting = false;
            _holdStarted = false;
            _turnManagerScript.keepTurn = false; //reset keep turn bool to false so that it only gets set true if the correct ball is scored

            CalculateShot(_cueBall, _predictionLineParent);
            StartCoroutine(EndShot());
            _chargeVisual.gameObject.SetActive(false);
        }

    }


    /// <summary>
    /// Shoot without turning on charge, CalculateShot takes care of rocket speed
    /// </summary>
    /// <param name="stage"></param>
    private void RocketShoot(ClickStage stage)
    {
        if (stage == ClickStage.Pressed && shootAvailable)
        {
            //custom start shot
            _shooting = true;
            _stopShot = false;
            _startingPos = poolCueParent.transform.position;
        }
        //On release
        if (stage == ClickStage.Released && _shooting && !_stopShot)
        {
            _shooting = false;
            _turnManagerScript.keepTurn = false;

            CalculateShot(_cueBall, _predictionLineParent);
            StartCoroutine(EndShot());
            Shoot = DefaultShoot;
        }
    }


    /// <summary>
    /// Animates a random charge and shoots with random power
    /// </summary>
    /// <param name="stage"></param>
    private void RdmPowerShoot(ClickStage stage)
    {
        if (stage == ClickStage.Pressed && shootAvailable)
        {
            _shooting = true;
            _stopShot = false;
            _startingPos = poolCueParent.transform.position;
            _holdTimer = UnityEngine.Random.Range(_minCharge, _maxCharge);
            _chargeVisual.gameObject.SetActive(true);
            //anim
            StartCoroutine((RdmPowerAnim(_holdTimer))); //Handles shot release stuff

        }
    }


    private void CurveShoot(ClickStage stage)
    {
        //if ()
    }


    /// <summary>
    /// Animate the charge going up, down, then to the random shot power value.
    /// After, handle shooting the ball and end shot.
    /// </summary>
    /// <param name="targetCharge"></param>
    /// <returns></returns>
    private IEnumerator RdmPowerAnim(float targetCharge)
    {
        bool goingUp = true;
        bool goingDown = false;
        bool goTarget = false;
        float tempholdTimer = 0f;

        while (goingUp)
        {
            if (tempholdTimer >= _maxCharge)
            {
                goingUp = false;
                goingDown = true;
            }
            else
            {
                tempholdTimer += Time.deltaTime * _rdmChargeSpeed;
                _chargeVisual.size = (tempholdTimer / _maxCharge);
                yield return null;
            }
        }
        while (goingDown)
        {
            if (tempholdTimer <= 0)
            {
                goingDown = false;
                goTarget = true;
            }
            else
            {
                tempholdTimer -= Time.deltaTime * _rdmChargeSpeed;
                _chargeVisual.size = (tempholdTimer /  (_maxCharge));
                yield return null;
            }

        }
        while (goTarget)
        {
            if (Mathf.Abs(tempholdTimer - targetCharge) < 0.01)
            {
                goTarget = false;
                _chargeVisual.size = (targetCharge / _maxCharge);
            }
            else
            {
                tempholdTimer += Time.deltaTime * _rdmChargeSpeed;
                _chargeVisual.size = (tempholdTimer / _maxCharge);
                poolCueParent.transform.position += poolCueParent.transform.right * _pullBackSpeed * Time.deltaTime;
                yield return null;
            }
        }
        //shoot the ball
        _shooting = false;
        _turnManagerScript.keepTurn = false;
        _chargeVisual.gameObject.SetActive(false);
        CalculateShot(_cueBall, _predictionLineParent);
        StartCoroutine(EndShot());
        Shoot = DefaultShoot;
    }


    /// <summary>
    /// Set on extra balls physics and shoot all balls
    /// </summary>
    /// <param name="stage"></param>
    private void MultiShoot(ClickStage stage)
    {
        if (stage == ClickStage.Pressed && shootAvailable)
        {
            StartShot();
            _extraCueBallA.GetComponent<Collider>().isTrigger = false; //Activate physics
            _extraCueBallA.GetComponent<Rigidbody>().isKinematic = false;
            _extraCueBallB.GetComponent<Collider>().isTrigger = false;
            _extraCueBallB.GetComponent<Rigidbody>().isKinematic = false;
        }
        else if (stage == ClickStage.Released && _shooting && !_stopShot)
        {
            _shooting = false;
            _holdStarted = false;
            _turnManagerScript.keepTurn = false;

            CalculateShot(_extraCueBallA, _extraLineA);
            CalculateShot(_extraCueBallB, _extraLineB);
            CalculateShot(_cueBall, _predictionLineParent);

            StartCoroutine(EndShot());
            _chargeVisual.gameObject.SetActive(false);
            Shoot = DefaultShoot;
        }
    }


    /// <summary>
    /// Spawn balls and lines in correct spots with correct angles
    /// </summary>
    private void SpawnMultiBalls()
    {
        //Set up a left and right position for the extra balls
        Vector3 shiftLeft = new Vector3(-poolCueParent.transform.forward.x, 0, -poolCueParent.transform.forward.z);
        Vector3 shiftRight = new Vector3(poolCueParent.transform.forward.x, 0, poolCueParent.transform.forward.z);
        Vector3 ballAPos = (_cueBall.transform.position + shiftLeft);
        Vector3 ballBPos = (_cueBall.transform.position + shiftRight);

        //spawn balls and set physics off
        _extraCueBallA = Instantiate(_cueBallExtra, ballAPos, Quaternion.identity);
        _extraCueBallA.GetComponent<Rigidbody>().isKinematic = true;
        _extraCueBallA.GetComponent<Collider>().isTrigger = true;

        _extraCueBallB = Instantiate(_cueBallExtra, ballBPos, Quaternion.identity);
        _extraCueBallB.GetComponent<Rigidbody>().isKinematic = true;
        _extraCueBallB.GetComponent<Collider>().isTrigger = true;


        _predictionLineParent.SetActive(false);
        _predictionLineParent.SetActive(true);

        _extraLineA = Instantiate(_predictionLineParent, _extraCueBallA.transform.position, Quaternion.LookRotation(-poolCueParent.transform.right));
        _extraLineA.transform.localEulerAngles = new Vector3(0, _extraLineA.transform.localEulerAngles.y, _extraLineA.transform.localEulerAngles.z); // set x to 0 so not angled down
        _extraLineA.transform.localEulerAngles += new Vector3(0, -_multiBallShotAngle, 0); //adjust aim angle
        _extraLineA.transform.position = _extraCueBallA.transform.position +  (_extraLineA.transform.forward * _ballSeparation); //put the line slightly in front of ball

        _extraLineB = Instantiate(_predictionLineParent, _extraCueBallB.transform.position, Quaternion.LookRotation(-poolCueParent.transform.right));
        _extraLineB.transform.localEulerAngles = new Vector3(0, _extraLineB.transform.localEulerAngles.y, _extraLineB.transform.localEulerAngles.z);
        _extraLineB.transform.localEulerAngles += new Vector3(0, _multiBallShotAngle, 0);
        _extraLineB.transform.position = _extraCueBallB.transform.position + (_extraLineB.transform.forward * _ballSeparation);

        if (_extraCueBallA.TryGetComponent(out ExtraBall extraBallScriptA)) //Pass a ref of this script cleanly into extra ball for shootAvailable bool
        {
            extraBallScriptA.Access(this);
        }
        if (_extraCueBallB.TryGetComponent(out ExtraBall extraBallScriptB))
        {
            extraBallScriptB.Access(this);
        }
    }


    /// <summary>
    /// Resets shot timer and moves pool cue model back to starting position
    /// </summary>
    /// <param name="context"></param>
    public void CancelShot(InputAction.CallbackContext context) //NOTE called through unity events in the input action
    {
        _stopShot = true;

        //reset charge
        _holdTimer = 0;
        _chargeVisual.gameObject.SetActive(false);

        poolCueParent.transform.position = _startingPos; //move stick back
    }


    /// <summary>
    /// Reads the WASD input from the user and converts to a vector2 _rotateInput
    /// </summary>
    /// <param name="context"></param>
    public void Rotate(InputAction.CallbackContext context) //NOTE called through unity events in the input action
    {
        // WASD _rotateInput = context.ReadValue<Vector2>();
        //TEMP NO WASD
        _tempRotateInput = -context.ReadValue<float>();
        if (context.started)
        {
            _holding = true;
        }
        if (context.canceled)
        {
            _holding = false;
            _rotationMult = _startingRotMult;
        }
    }


    /// <summary>
    /// Respawn pool cue and prediction line based on cue ball position.
    /// </summary>
    public void RespawnPoolCue()
    {
        _poolCueModel.gameObject.SetActive(true);
        poolCueParent.transform.position = _cueBall.transform.position - new Vector3(0f, 0f, _ballSeparation);

        //set up prediction lines position and rotation
        _predictionLineParent.gameObject.SetActive(true);
        _predictionLineParent.transform.position = poolCueParent.transform.position + new Vector3(0, 0, _ballSeparation * 2); // 2 pushes it further away from pool cue
        _predictionLineParent.transform.eulerAngles = new Vector3(0, 0, 0);

        poolCueParent.transform.eulerAngles = new Vector3(0, 90, 3); // model needs to have y 90 // 3 gives small tilt to not clip table
    }


    /// <summary>
    /// Take in a ball and object for its transform.forward direction then shoot the ball
    /// </summary>
    /// <param name="ball"></param>
    /// <param name="direction"></param>
    private void CalculateShot(GameObject ball, GameObject direction)
    {
     
        if (_holdTimer > _maxCharge) //max out power on shot
        {
            _holdTimer = _maxCharge;
        }

        float shotForce = _defaultShotSpeed * _holdTimer;
        if (Shoot == RocketShoot)
        {
            shotForce = _rocketShotForce;
        }

        Vector3 shotDir = direction.transform.forward;
        //calculate where to hit the ball
        Vector3 hitPos = ball.transform.position - (shotDir * _ballRadius); // Perfect center of the ball no added spin
        //Vector3 hitPosSpin = new Vector3(hitPos.x, hitPos.y - 1, hitPos.z); // Change these values to affect spin

        poolCueParent.transform.position = hitPos; //snap pool cue forward to "hit the ball"

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.AddForceAtPosition(shotDir.normalized * shotForce, hitPos, ForceMode.Impulse); // Impulse gives all force at once - realistic for pool
    }


    /// <summary>
    /// Prepares bools gets UI set up and stores variables needed when shot completes
    /// </summary>
    private void StartShot()
    {
        _shooting = true;
        _stopShot = false;

        //Prepare the hold timer
        _holdTimer = 0;
        _holdStarted = true;

        // set up the visual indicator
        _chargeVisual.gameObject.SetActive(true);
        _chargeVisual.size = _holdTimer;

        _startingPos = poolCueParent.transform.position; //Record pool cue position to move back to it incase of canceled shot
    }


    /// <summary>
    /// Wait 0.1 seconds so pool cue model snaps forward and "hits the ball". Then call the ShotEvent()
    /// </summary>
    /// <returns></returns>
    IEnumerator EndShot()
    {
        shootAvailable = false;
        _predictionLineParent.gameObject.SetActive(false); //hide prediction line
        if (_extraLineA != null && _extraLineB != null) //Multi shoot
        {
            Destroy(_extraLineA.gameObject);
            Destroy(_extraLineB.gameObject);
        }

        ShotCompletedEvent(); //This starts the ball manager checking to see if balls are stopped
        yield return new WaitForSeconds(0.1f); //Leave pool cue up for a tenth of a second to show it hitting the ball;
        _poolCueModel.gameObject.SetActive(false);

    }


    /// <summary>
    /// Invokes ShotCompleted
    /// </summary>
    private void ShotCompletedEvent()
    {
        ShotCompleted?.Invoke();
    }


    /// <summary>
    /// Handle different shot based powerups by assigning shoot to the unique type
    /// </summary>
    /// <param name="powerUP"></param>
    private void HandlePUs(SO_PowerUp powerUP)
    {
        if (powerUP.id == "Multi Shot")
        {
            SpawnMultiBalls();
            Shoot = MultiShoot;
        }
        if (powerUP.id == "Rocket Shot")
        {
            Shoot = RocketShoot;
        }
        if (powerUP.id == "Random Power Shot")
        {
            _rdmPowerPU = true;
        }
        if (powerUP.id == "Curved Shot")
        {
            _curvedShot = true;
        }
    }


    /// <summary>
    /// Update power ups that affect next
    /// </summary>
    /// <param name="sabotaged"></param>
    private void HandleTurnSwitch(bool sabotaged)
    {
        if (sabotaged)
        {
            //check dif PUs
            if (_rdmPowerPU)
            {
                _rdmPowerPU = false;
                Shoot = RdmPowerShoot;
            }
            if (_curvedShot)
            {
                _curvedShot = false;
                Shoot = CurveShoot;
            }
        }
    }
}

