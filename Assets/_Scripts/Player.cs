using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
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
    private GameObject _poolCueParent;


    [Header("Modifiable Shot Mechanics")]
    [SerializeField] private float _defaultShotSpeed = 5f;
    [SerializeField] private float _maxCharge = 3f; // 3 for the seconds of time a player can hold down shoot at max
    [SerializeField] private float _pullBackSpeed = 1f;
    [SerializeField] private float _rotationSpeed = 50f;
    [SerializeField] private float _ballSeparation = 0.5f;


    [Header("PowerUp Variables")]
    [SerializeField] private float _multiBallShotSeperation = 0.1f;
    private GameObject _extraCueBallA;
    private GameObject _extraCueBallB;


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
        _poolCueParent = this.gameObject;
        #endregion
        //UI
        _chargeVisual.gameObject.SetActive(false);

        //Extra
        _ballRadius = _cueBall.GetComponent<SphereCollider>().radius; //used for shoot math but the radius only needs to be gathered once so I have it here in Awake.
        _predictionLineParent.transform.position = _poolCueParent.transform.position + new Vector3(0, 0, _ballSeparation * 2); //set up prediction lines position and rotation
        _predictionLineParent.transform.eulerAngles = new Vector3(0, 0, 0);
        Shoot = DefaultShoot;
    }


    private void OnEnable()
    {
        SO_PowerUp.UsePU += HandlePUs;
    }

    private void OnDisable()
    {
        SO_PowerUp.UsePU -= HandlePUs;
    }


    private void Update()
    {
        //Debug.Log(_poolCueParent.transform.right);
        //If given input rotate pool cue and visual indicator
        _poolCueParent.transform.RotateAround(_cueBall.transform.position, new Vector3(0, _tempRotateInput, 0), _rotationSpeed * Time.deltaTime);
        _predictionLineParent.transform.RotateAround(_cueBall.transform.position, new Vector3 (0, _tempRotateInput, 0), _rotationSpeed * Time.deltaTime); //USING TEMP ROTATE

        if (_holdStarted && !_stopShot)
        {
            _holdTimer += Time.deltaTime;
            _chargeVisual.size = (_holdTimer / _maxCharge); // divide by _maxHold because scrollBarSize is capped to 1.

            if (_holdTimer < _maxCharge) //dont let the pool cue pull back further than max power
            {
                _poolCueParent.transform.position += _poolCueParent.transform.right * _pullBackSpeed * Time.deltaTime; // .right is moving the cue back
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

            CalculateShot(_cueBall, Vector3.zero);
            StartCoroutine(EndShot());
            _chargeVisual.gameObject.SetActive(false);
        }

    }


    /// <summary>
    /// Spawns 2 extra cue balls and shoots all cue balls in different directions.
    /// </summary>
    /// <param name="stage"></param>
    private void MultiShoot(ClickStage stage)
    {
        if (stage == ClickStage.Pressed && shootAvailable)
        {
            StartShot();
            //Set up a left and right position for the extra balls
            Vector3 shiftLeft = new Vector3(-_poolCueParent.transform.forward.x, 0, -_poolCueParent.transform.forward.z);
            Vector3 shiftRight = new Vector3(_poolCueParent.transform.forward.x, 0, _poolCueParent.transform.forward.z);
            Vector3 ballAPos = (_cueBall.transform.position + shiftLeft);
            Vector3 ballBPos = (_cueBall.transform.position + shiftRight);

            _extraCueBallA = Instantiate(_cueBallExtra, ballAPos, Quaternion.identity);
            _extraCueBallB = Instantiate(_cueBallExtra, ballBPos, Quaternion.identity);
            if(_extraCueBallA.TryGetComponent(out ExtraBall extraBallScriptA)) //Pass a ref of this script cleanly into extra ball for shootAvailable bool
            {
                extraBallScriptA.Access(this);
            }
            if (_extraCueBallB.TryGetComponent(out ExtraBall extraBallScriptB))
            {
                extraBallScriptB.Access(this);
            }
        }
        else if (stage == ClickStage.Released && _shooting && !_stopShot)
        {
            _shooting = false;
            _holdStarted = false;
            _turnManagerScript.keepTurn = false;

            Vector3 leftOffset = new Vector3(-_multiBallShotSeperation, 0, 0); //aims ball diagonally
            Vector3 rightOffset = new Vector3(_multiBallShotSeperation, 0, 0);
            CalculateShot(_extraCueBallA, leftOffset); //shoot all balls
            CalculateShot(_extraCueBallB, rightOffset);
            CalculateShot(_cueBall, Vector3.zero);

            StartCoroutine(EndShot());
            _chargeVisual.gameObject.SetActive(false);
            Shoot = DefaultShoot;
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

        _poolCueParent.transform.position = _startingPos; //move stick back
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
    }


    /// <summary>
    /// Respawn pool cue and prediction line based on cue ball position.
    /// </summary>
    public void RespawnPoolCue()
    {
        _poolCueModel.gameObject.SetActive(true);
        _poolCueParent.transform.position = _cueBall.transform.position - new Vector3(0f, 0f, _ballSeparation);

        //set up prediction lines position and rotation
        _predictionLineParent.gameObject.SetActive(true);
        _predictionLineParent.transform.position = _poolCueParent.transform.position + new Vector3(0, 0, _ballSeparation * 2); // 2 pushes it further away from pool cue
        _predictionLineParent.transform.eulerAngles = new Vector3(0, 0, 0);

        _poolCueParent.transform.eulerAngles = new Vector3(0, 90, 3); // model needs to have y 90 // 3 gives small tilt to not clip table
    }


    /// <summary>
    /// Calculate shot power, spin and direction, then launch the ball
    /// </summary>
    /// <param name="ball"></param>
    /// <param name="offset"></param>
    private void CalculateShot(GameObject ball, Vector3 offset)
    {
     
        if (_holdTimer > _maxCharge) //max out power on shot
        {
            _holdTimer = _maxCharge;
        }

        float shotForce = _defaultShotSpeed * _holdTimer;
        Vector3 shotDir = -_poolCueParent.transform.right + offset; // Negative transform.right because we want to shoot away from where its facing
        //calculate where to hit the ball
        Vector3 hitPos = ball.transform.position - (shotDir * _ballRadius); // Perfect center of the ball no added spin
        Vector3 hitPosSpin = new Vector3(hitPos.x, hitPos.y - 1, hitPos.z); // Change these values to affect spin

        _poolCueParent.transform.position = hitPos; //snap pool cue forward to "hit the ball"

        Rigidbody ballRb = ball.GetComponent<Rigidbody>();
        ballRb.AddForceAtPosition(shotDir.normalized * shotForce, hitPosSpin, ForceMode.Impulse); // Impulse gives all force at once - realistic for pool
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

        _startingPos = _poolCueParent.transform.position; //Record pool cue position to move back to it incase of canceled shot
    }


    /// <summary>
    /// Wait 0.1 seconds so pool cue model snaps forward and "hits the ball". Then call the ShotEvent()
    /// </summary>
    /// <returns></returns>
    IEnumerator EndShot()
    {
        shootAvailable = false;
        _predictionLineParent.gameObject.SetActive(false); //hide prediction line

        yield return new WaitForSeconds(0.1f); //Leave pool cue up for a tenth of a second to show it hitting the ball;
        _poolCueModel.gameObject.SetActive(false);

        ShotCompletedEvent(); //This starts the ball manager checking to see if balls are stopped
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
            Shoot = MultiShoot;
        }
    }
}

