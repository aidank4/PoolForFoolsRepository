using NUnit.Framework;
using System;
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
    private Player _playerScript;
    [SerializeField] private GameObject _startingPos;
    [SerializeField] private GameObject _cueBall;


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
    private float _maxWaitTime = 3f; // seconds


    [Header("Power Up Variables")]
    private bool _eightBallPUActive = false;


    //Events
    public Action BallsStopped;



    private void Awake()
    {
        //ref
        _playerScript = _poolCueRef.GetComponent<Player>();

        SpawnBalls();
        _spawnedBalls.Add(_cueBall); //After all balls have been spawned add in the cue ball to the list

    }


    private void OnEnable()
    {
        if (_playerScript != null)
        {
            _playerScript.ShotCompleted += HandleShotEvent;
            SO_PowerUp.UsePU += HandlePowerUps;
        }
    }

    private void OnDisable()
    {
        if (_playerScript != null)
        {
            _playerScript.ShotCompleted -= HandleShotEvent;
            SO_PowerUp.UsePU += HandlePowerUps;
        }
    }


    private void FixedUpdate()
    {
        BallCheck();
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

            //if balls are still wait a bit in case it was just a split second
            if (checkBallsStopped)
            {
                _timer += Time.fixedDeltaTime;
                if (_timer >= _minWaitTime)
                {
                    _checkBalls = false;
                    BallsStoppedEvent(); // Used by inventory manager
                    _timer = 0; // reset timer for next time
                    _maxTimer = 0;
                    if (_eightBallPUActive)
                    {
                        EightBallPU(false);
                    }
                    return;
                }
            }
            else
            {
                _timer = 0;
            }
            if (_maxTimer >= _maxWaitTime && _cueBall.GetComponent<Rigidbody>().IsSleeping()) //covers long wait if cueballs ready
            {
                _checkBalls = false;
                BallsStoppedEvent();
                _timer = 0;
                _maxTimer = 0;
                if (_eightBallPUActive)
                {
                    EightBallPU(false);
                }
                return;
            }

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
        for (int i = 0; i < 30; i++)
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
        for (int i = 0; i < 30; i++)
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
            if (i == 4)
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
    /// Call Eight ball PU
    /// </summary>
    /// <param name="powerUp"></param>
    private void HandlePowerUps(SO_PowerUp powerUp)
    {
        if (powerUp.id == "Eight Ball Protection")
        {
            EightBallPU(true);
        }
    }
}
