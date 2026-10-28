using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;
using UnityEngine.UIElements;

public class BallManager : MonoBehaviour
{
    [System.Serializable]
    public struct BallData
    {
        public GameObject ballObject;
        public bool stripes;
        public bool eightBall;
    }


    [Header("References")]
    [SerializeField] private GameObject _poolCueRef;
    private Player _poolCueScript;
    [SerializeField] private GameObject _startingPos;
    [SerializeField] private GameObject _cueBall;
    [SerializeField] private GameObject _vfxManagerRef;
    private VFXManager _vfxManager;
    [SerializeField] private GameObject _pocketManagerRef;
    private PocketManager _pocketManager;
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManager;

    [Header("Spawn Randomness")]
    [SerializeField] private float _spawnVarianceMin = -0.05f;
    [SerializeField] private float _spawnVarianceMax = 0.05f;


    [Header("Ball Data")]
    [SerializeField] private BallData[] _balls;
    List<BallData> solidOptions = new List<BallData>();
    List<BallData> stripesOptions = new List<BallData>();
    private List<GameObject> _spawnedBalls = new List<GameObject>(); //These gameobjects are the ones actually spawned into the scene (important for rb)
    private BallData tempBall; //Used as a null ball for swapping array elements


    [Header("Misc Variables")]
    private bool _checkBalls; //Used to control if ball movement is being checked
    private float _timer;
    private float _maxTimer;
    private float _minWaitTime = 0.25f;
    private float _maxWaitTime = 3f; //seconds


    [Header("Power Ups")]
    [Header("Bomb")]
    [SerializeField] private float _explosiveForce = 100f;
    [SerializeField] private float _explosiveRadius = 5f;
    [SerializeField] private float _explodeUpOffset = 1f;
    [SerializeField] private Material _blinkRed;
    [SerializeField] private Material _blinkYellow;
    [SerializeField] private Material _blinkBaseColor;
    [SerializeField] private float _initialBlinkSpeed = 0.5f;
    private float _blinkMultiplier = 1f;
    private int _blinkCount = 0;
    private bool _bombBlastActive = false; //When bomb PU is active
    [Header("Rocket")]
    private bool _rocketShotPUActive = false; //When Rocket PU is active
    private Transform _rocketModel;
    private Transform _baseCueBallModel;
    [Header("Magnet")]
    [SerializeField] private float _attractionRadius = 3f; 
    [SerializeField] private float _attractionforce = 0.1f; 
    private bool _magnetActive = false; //When Magnet PU is active
    [Header("8Ball")]
    private bool _eightBallPUActive = false; //When 8ball PU is active
    [Header("Giant Balls")]
    [SerializeField] private float _gbSizeScaler = 3f;
    private bool _ballsBig = false; //The balls are currently big or not
    private bool _gbActive = false; //Giant Balls PU got activated -- affects following turn



    //Events
    public Action BallsStopped;



    private void Awake()
    {
        //ref
        #region
        _poolCueScript = _poolCueRef.GetComponent<Player>();
        _vfxManager = _vfxManagerRef.GetComponent<VFXManager>();
        _rocketModel = _cueBall.transform.Find("Rocket Model");
        _baseCueBallModel = _cueBall.transform.Find("Cue Ball Model");
        _pocketManager = _pocketManagerRef.GetComponent<PocketManager>();
        _turnManager = _turnManagerRef.GetComponent<TurnManager>();
        #endregion
        //hide
        #region
        _rocketModel.gameObject.SetActive(false);
        _baseCueBallModel.gameObject.SetActive(true);
        #endregion

        SpawnBalls();
        _spawnedBalls.Add(_cueBall); //After all balls have been spawned add in the cue ball to the list

    }


    /// <summary>
    /// Subscribe to Events
    /// </summary>
    private void OnEnable()
    {
        if (_poolCueScript != null)
        {
            _poolCueScript.ShotCompleted += HandleShotEvent;
        }
        if (_pocketManager != null)
        {
            _pocketManager.Scratched += HandleScratch;
        }
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched += HandleTurnSwitch;
        }
        SO_PowerUp.UsePU += HandlePowerUps;
    }


    /// <summary>
    /// Unsubscribe
    /// </summary>
    private void OnDisable()
    {
        if (_poolCueScript != null)
        {
            _poolCueScript.ShotCompleted -= HandleShotEvent;
        }
        if (_pocketManager != null)
        {
            _pocketManager.Scratched -= HandleScratch;
        }
        SO_PowerUp.UsePU += HandlePowerUps;
    }


    private void FixedUpdate()
    {
        BallCheck();
        if (_rocketShotPUActive && !_checkBalls) //Rotate rocket while player aims
        {
            RocketRotation();
        }
    }

    private void Update()
    {
        if (_magnetActive && _cueBall.GetComponent<Rigidbody>().linearVelocity.magnitude > .2f) //if under .2 balls should stop magnitizing to let cue ball stop
        {
            MagnetAttract();
        }
    }


    /// <summary>
    /// Checks every ball to see if it is sleeping. Once all are still call BallsStoppedEvent()
    /// </summary>
    private void BallCheck()
    {
        if (_checkBalls) // this bool is set to true shortly after a shot has been made
        {
            _maxTimer += Time.fixedDeltaTime;
            bool checkBallsStopped = true;
            foreach (GameObject ball in _spawnedBalls) //Check each ball and if any are moving it will set stopped bool to false
            {
                if (ball == null) //check if ball has been removed from the game
                {
                    _spawnedBalls.Remove(ball);
                    break; //break so that the foreach loop doesn't continue with a missing element
                }
                else
                {
                    Rigidbody ballRb = ball.GetComponent<Rigidbody>();
                    if (!ballRb.IsSleeping())
                    {
                        checkBallsStopped = false;
                    }
                }
            }

            if (_cueBall.GetComponent<Rigidbody>().IsSleeping() && _bombBlastActive && !_poolCueScript.shootAvailable) //Bomb blast power up
            {
                //checkBallsStopped = false; MIGHT NEED
                StartCoroutine(BlastBalls());
                return;
            }

            //if balls are still wait a bit in case it was just a split second
            if (checkBallsStopped)
            {
                _timer += Time.fixedDeltaTime;
                if (_timer >= _minWaitTime)
                {
                    BallCheckOver();
                    return;
                }
            }
            else
            {
                _timer = 0;
            }
            if (_maxTimer >= _maxWaitTime && _cueBall.GetComponent<Rigidbody>().IsSleeping()) //covers long wait if cueballs ready
            {
                BallCheckOver();
                return;
            }

        }
    }


    /// <summary>
    ///Handles what to do when balls are all still 
    /// </summary>
    private void BallCheckOver()
    {
        BallsStoppedEvent();
        _checkBalls = false;
        _timer = 0;
        _maxTimer = 0;

        //Reset Power Ups
        if (_eightBallPUActive)
        {
            EightBallPU(false);
        }
        if (_rocketShotPUActive)
        {
            RocketBall();
        }
        if (_magnetActive)
        {
            _magnetActive = false;
            _vfxManager.MagnetOff();
        }
        if (_ballsBig)
        {
            _ballsBig = false;
            ResetGiantBalls();
        }
    }


    /// <summary>
    /// Calls functions that set the balls spawn order.
    /// Then spawn the balls in position next to each other
    /// with a small added randomness.
    /// </summary>
    private void SpawnBalls()
    {
        QueueSolids();
        QueueStripes();
        SetSpawnOrder(); //NOTE Queue Functions must come before SetSpawnOrder()
        int spawnIndex = 0; //this will keep track of which number ball to spawn

        float ballDiameter = 0.5715f;
        int rows = 5;
        float rowSeperation = ballDiameter * Mathf.Sqrt(3f) / 2f;
        for (int row = 0; row < rows; row++) //cycle through each row of the ball rack set up
        {
            int ballsInRow = row + 1;
            for (int i = 0; i < ballsInRow; i++) //loop through each ball in each row
            {
                float zValue = -rowSeperation * row; // negative to flip triangle direction
                float yValue = 0f;
                float xValue = (i - (ballsInRow - 1) / 2f) * ballDiameter; //gets either +-0, 0.5, 1, 1.5 to shift the ball to the side on the x

                Vector3 spawnPos = _startingPos.transform.position - new Vector3(xValue, yValue, zValue); // set it to behind the starting position movable object
                Vector3 randomPos = new Vector3(UnityEngine.Random.Range(_spawnVarianceMin, _spawnVarianceMax), 0f, UnityEngine.Random.Range(_spawnVarianceMin, _spawnVarianceMax));


                //spawn and set it to a variable to later reference its rigidbody
                GameObject spawnBall = Instantiate(_balls[spawnIndex].ballObject, spawnPos + randomPos, Quaternion.identity);
                _spawnedBalls.Add(spawnBall); //add to list to get rigidbody later in fixed update

                spawnIndex++;
            }

        }
    }


    /// <summary>
    /// Randomize all the solids into a new queue
    /// </summary>
    private void QueueSolids()
    {
        foreach (BallData ball in _balls)
        {
            if (!ball.stripes && !ball.eightBall) //eightball is not set as stripes
            {
                solidOptions.Add(ball);
            }
        }

        int indexA;
        int indexB;
        int holder = solidOptions.Count;
        solidOptions.Add(tempBall);
        for (int i = 0; i < 30; i++) //30 swaps to randomize order
        {
            bool different = false;
            indexA = UnityEngine.Random.Range(0, solidOptions.Count - 2); // - 2 because holder is in here being null
            do
            {
                indexB = UnityEngine.Random.Range(0, solidOptions.Count - 2);
                if (indexB != indexA)
                {
                    different = true;
                }
            } while (!different);

            //swap the indexes
            solidOptions[holder] = solidOptions[indexA];
            solidOptions[indexA] = solidOptions[indexB];
            solidOptions[indexB] = solidOptions[holder];
        }
        solidOptions.Remove(tempBall);
    }


    /// <summary>
    /// Randomize all the stripes into a new queue
    /// </summary>
    private void QueueStripes()
    {
        foreach (BallData ball in _balls)
        {
            if (ball.stripes) //eight is not set as stripes
            {
                stripesOptions.Add(ball);
            }
        }

        int indexA;
        int indexB;
        int holder = stripesOptions.Count;
        stripesOptions.Add(tempBall);
        for (int i = 0; i < 30; i++) //30 swaps to randomize order
        {
            bool different = false;
            indexA = UnityEngine.Random.Range(0, stripesOptions.Count - 2); // - 2 because holder is in here being null
            do
            {
                indexB = UnityEngine.Random.Range(0, stripesOptions.Count - 2);
                if (indexB != indexA)
                {
                    different = true;
                }
            } while (!different);

            //swap the indexes
            stripesOptions[holder] = stripesOptions[indexA];
            stripesOptions[indexA] = stripesOptions[indexB];
            stripesOptions[indexB] = stripesOptions[holder];
        }
        stripesOptions.Remove(tempBall);
    }


    /// <summary>
    ///Goes through the ball array and sets up all the indexes in the correct positions to be spawned.
    ///Based on solid and stripe queues so put this after those have been called
    /// </summary>
    private void SetSpawnOrder()
    {
        int solidsIndex = 0;
        int stripesIndex = 0;
        for (int i = 0; i < _balls.Length; i++)
        {
            if (i == 4) //8 ball must spawn 4th to be in the center
            {
                for (int idx = 0; idx < _balls.Length; idx++)
                {
                    if (_balls[idx].eightBall)
                    {
                        _balls[i] = _balls[idx];
                        break;
                    }
                }
            }
            else if (i % 2 == 0)
            {
                _balls[i] = solidOptions[solidsIndex];
                solidsIndex++;
            }
            else if (i % 2 == 1)
            {
                _balls[i] = stripesOptions[stripesIndex];
                stripesIndex++;
            }
        }
    }


    /// <summary>
    /// Once balls are stopped invoke event
    /// </summary>
    private void BallsStoppedEvent()
    {
        BallsStopped?.Invoke();
    }


    /// <summary>
    /// Once the player has shot set _checkBalls to true to start the ball check
    /// </summary>
    private void HandleShotEvent()
    {
        _checkBalls = true;
        if (_rocketShotPUActive) // Constrain rocket rotation on shot
        {
            _cueBall.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
            _vfxManager.RocketOff();
        }
    }


    /// <summary>
    /// Finds the 8 ball and sets is kinematic.
    /// Argument will set kinematic to true or false
    /// </summary>
    /// <param name="activate"></param>
    private void EightBallPU(bool activate)
    {
        foreach (GameObject ball in _spawnedBalls)
        {
            if (ball.gameObject.tag == "EightBall")
            {
                //Swap model
                Transform rock = ball.transform.Find("Rock");
                if (!rock.gameObject.activeSelf)
                {
                    rock.gameObject.SetActive(true);
                }
                else
                {
                    rock.gameObject.SetActive(false);
                }
                //Set physics
                if (activate)
                {
                    ball.GetComponent<Rigidbody>().isKinematic = true;
                    _eightBallPUActive = true;
                }
                else
                {
                    ball.GetComponent<Rigidbody>().isKinematic = false;
                    _eightBallPUActive = false;
                }
            }
        }
    }


    /// <summary>
    /// Swap cue ball model with rocket and turn on vfx.
    /// If rocket is already active then reset back to standard cue ball
    /// </summary>
    private void RocketBall()
    {

        if (!_rocketModel.gameObject.activeSelf) //rocket on
        {
            _rocketShotPUActive = true;
            RocketRotation();
            _rocketModel.gameObject.SetActive(true);
            _baseCueBallModel.gameObject.SetActive(false);
            _vfxManager.RocketOn(_rocketModel);
        }
        else // rocket off
        {
            _baseCueBallModel.gameObject.SetActive(true);
            _rocketModel.gameObject.SetActive(false);
            _cueBall.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None; // reset rotation constraints
            _rocketShotPUActive = false;
        }
    }


    /// <summary>
    /// Update rocket models rotation to orientation of pool cue
    /// </summary>
    private void RocketRotation()
    {
        _rocketModel.rotation = Quaternion.LookRotation(_poolCueScript.poolCueParent.transform.up, -_poolCueScript.poolCueParent.transform.right);
    }


    /// <summary>
    /// Adds explosive force to objects near cue ball
    /// </summary>
    /// <returns></returns>
    IEnumerator BlastBalls()
    {
        _checkBalls = false;
        _blinkMultiplier *= 0.25f;
        yield return new WaitForSeconds(1f); // After 1 second blink faster
        _blinkMultiplier *= 0.25f;
        yield return new WaitForSeconds(.5f); //After 0.5 secs blow up
        StopAllCoroutines();
        _blinkMultiplier = 1f; //reset
        _cueBall.GetComponentInChildren<Renderer>().material = _blinkBaseColor;
        _vfxManager.ExplosionEffect(_cueBall.transform.position);

        Collider[] nearbyObjects = Physics.OverlapSphere(_cueBall.transform.position, _explosiveRadius);
        foreach (Collider obj in nearbyObjects)
        {
            if (obj != _cueBall.GetComponent<Collider>()) //don't blow up cue ball
            { 
                if (obj.TryGetComponent(out Rigidbody rb))
                {
                    rb.AddExplosionForce(_explosiveForce, _cueBall.transform.position, _explosiveRadius, _explodeUpOffset, ForceMode.Impulse);
                }
            }
        }
        _bombBlastActive = false;
        _checkBalls = true;
    }


    /// <summary>
    /// blinks the cue ball between yellow and red
    /// </summary>
    /// <param name="ballMesh"></param>
    /// <returns></returns>
    IEnumerator BlastBlink(Renderer ballMesh)
    {
        
        if (_blinkCount % 2 == 0)
        {
            ballMesh.material = _blinkYellow;
            _blinkCount++;
        }
        else
        {
            ballMesh.material = _blinkRed;
            _blinkCount++;
        }
        yield return new WaitForSeconds(_initialBlinkSpeed * _blinkMultiplier);
        StartCoroutine(BlastBlink(ballMesh)); // Keep calling to blink
    }


    /// <summary>
    /// Pull in nearby collider objects
    /// </summary>
    private void MagnetAttract()
    {
        Collider[] closeBalls = Physics.OverlapSphere(_cueBall.transform.position, _attractionRadius);
        foreach (Collider victim in closeBalls)
        {
            if (victim != _cueBall.GetComponent<Collider>()) //Pull all balls except for cue ball
            {
                if (victim.TryGetComponent(out Rigidbody rb))
                {
                    Vector3 direction = rb.gameObject.transform.position - _cueBall.transform.position;
                    float distance = direction.magnitude; //How far from cue ball
                    if (distance > 0.75f) //don't pull objects to close to the cue ball - or else they always push it
                    {
                        rb.AddForce(direction.normalized * (-_attractionforce/distance), ForceMode.Force);
                    }
                }
            }
        }
    }


    /// <summary>
    /// aside from the cue ball, make all the balls grow in size. Also turn the pockets off
    /// </summary>
    private void MakeGiantBalls()
    {
        foreach (GameObject ball in _spawnedBalls)
        {
            if (ball == _cueBall) // dont make cue ball big
            {
                continue;
            }
            ball.transform.localScale *= _gbSizeScaler;
            Rigidbody ballRb = ball.GetComponent<Rigidbody>(); //Turn off and on physics or else balls get stuck "flying"
            ballRb.isKinematic = true;
            ballRb.isKinematic = false;
        }
    }


    /// <summary>
    /// return the balls to regular size and turn pockets back on
    /// </summary>
    private void ResetGiantBalls()
    {
        foreach (GameObject ball in _spawnedBalls)
        {
            if (ball == _cueBall) // dont make cue ball small
            {
                continue;
            }
            ball.transform.localScale /= _gbSizeScaler;
            Rigidbody ballRb = ball.GetComponent<Rigidbody>(); //Turn off and on physics or else balls get stuck "flying"
            ballRb.isKinematic = true;
            ballRb.isKinematic = false;
        }
    }


    /// <summary>
    /// On scratch reset cue ball affecting power ups
    /// </summary>
    private void HandleScratch()
    {
        if (_rocketShotPUActive)
        {
            RocketBall();
        }
        if (_magnetActive)
        {
            _magnetActive = false;
            _vfxManager.MagnetOff();
        }
    }


    /// <summary>
    /// Call ball related powerups
    /// </summary>
    /// <param name="powerUp"></param>
    private void HandlePowerUps(SO_PowerUp powerUp)
    {
        if (powerUp.id == "Eight Ball Protection")
        {
            EightBallPU(true);
        }
        if(powerUp.id == "Bomb Shot")
        {
            _bombBlastActive = true;
            Renderer ballMesh = _cueBall.GetComponentInChildren<Renderer>();
            StartCoroutine(BlastBlink(ballMesh));
        }
        if (powerUp.id == "Rocket Shot")
        {
            RocketBall();
        }
        if (powerUp.id == "Magnetic Shot")
        {
            _magnetActive = true;
            _vfxManager.MagnetOn(_cueBall.transform);
        }
        if (powerUp.id == "Giant Balls")
        {
            _gbActive = true;
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
            if (_gbActive)
            {
                _gbActive = false;
                MakeGiantBalls();
                _ballsBig = true;
            }
        }
    }

}
