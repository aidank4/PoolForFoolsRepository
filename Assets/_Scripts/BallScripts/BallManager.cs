using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class BallManager : MonoBehaviour
{

    //Make struct for the array of balls
    [System.Serializable]
    public struct BallData
    {
        public GameObject ballObject;
        public bool stripes;
        public bool spawned;
        public bool eightBall;
        public bool cueBall;
        public int arrayNum; // temp for spawning can't figure out better way
    }

    private bool _checkBalls;
    protected float minSpeed = 0f; // ballscript sets balls moving below 0.2 to 0
    private float _timer;
    private float _minWaitTime = 0.25f;
    //Event for when balls are stopped
    public Action BallsStopped;

    //Get player object for script
    [SerializeField] private GameObject _poolCueRef;
    private Player _playerScript;
    

    [SerializeField] private GameObject _startingPos;
    private float _ballDiameter = 0.5715f;
    private int _rows = 5;

    //Variables for random spawn variance
    [SerializeField] private float _spawnVarianceMin = -0.05f;
    [SerializeField] private float _spawnVarianceMax = 0.05f;

    [SerializeField] private BallData[] _balls;

    private List<GameObject> _ballObjects = new List<GameObject>();


    private void Awake()
    {
        _playerScript = _poolCueRef.GetComponent<Player>();
        SpawnBalls();

    }

    //I don't know much about on enable but maybe it would be better to use than awake in general
    private void OnEnable()
    {
        //Event to start the check for if balls are still
        if (_playerScript != null)
        {
            _playerScript.Shot += ShotEventTriggered;
        }
    }

    private void OnDisable()
    {
        //Event to start the check for if balls are still
        if (_playerScript != null)
        {
            _playerScript.Shot -= ShotEventTriggered;
        }
    }

    private void FixedUpdate()
    {

        BallCheck();
    }



    private void SpawnBalls()
    {
        float rowSeperation = _ballDiameter * Mathf.Sqrt(3f) / 2f;
        int spawnIndex = 0; //this will keep track of which number ball is being spawned

        //cycle through each row of the ball rack set up
        for (int row = 0; row < _rows; row++)
        {
            //each row has the same balls as its row number
            int ballsInRow = row + 1;

            //loop through each ball in each row
            for (int i = 0; i < ballsInRow; i++)
            {
                //each row moves back by row seperation amount
                float zValue = -rowSeperation * row; // negative to flip triangle direction
                float yValue = 0f;

                //calculate the x value by multiplying ball diameter by + or - a certain distance // gets either +- 0, 0.5, 1, 1.5 
                float xValue = (i - (ballsInRow - 1) / 2f) * _ballDiameter;

                Vector3 spawnPos = _startingPos.transform.position - new Vector3(xValue, yValue, zValue); // set it to behind the starting position movable object
                //give some randomness
                Vector3 randomPos = new Vector3(UnityEngine.Random.Range(_spawnVarianceMin, _spawnVarianceMax), 0f, UnityEngine.Random.Range(_spawnVarianceMin, _spawnVarianceMax));


                //spawn and set it to a variable to later reference its rigidbody
                GameObject spawnBall = Instantiate(_balls[SelectedBall(spawnIndex)].ballObject, spawnPos + randomPos, Quaternion.identity);

                //add to array to get rigidbody in fixed update
                _ballObjects.Add(spawnBall);

                spawnIndex++;
            }

        }
        
   
    }


    //NOTE have to change from scriptable objects to structs
    private int SelectedBall(int spawnIndex)
    {
        bool lastBall = false;

        //at some point there will have to be two stripes or two solids so check if it is last ball to repeat
        if (spawnIndex == _balls.Length - 1)
        {
            lastBall = true;
        }

        //eight ball is the 5th ball
        if (spawnIndex == 4)
        {
            //Debug.Log("Index = 4");
            for (int i = 0; i < _balls.Length; i++) // - 1 to ignore cue ball which is in array as well
            {
                //find eightball
                if (_balls[i].eightBall)
                {
                    //Debug.Log("EightBall");
                    return i;
                }
            }
        }

        //If ball is solids
        else if (spawnIndex % 2 == 0 || lastBall)
        {
            //use this list to put available solid balls in
            List<BallData> solidOptions = new List<BallData>();

            foreach (BallData ball in _balls)
            {
                //find solid balls that haven't been spawned
                if (!ball.spawned && !ball.stripes && !ball.eightBall && !ball.cueBall) //eight and cue ball included because they dont have stripes as true
                {
                    solidOptions.Add(ball);
                }
            }

            //Make sure there are still solids to play
            if (solidOptions.Count > 0)
            {
                //After building list pick a random one - set it spawned and return the index
                int luckyIndex = UnityEngine.Random.Range(0, solidOptions.Count - 1);


                foreach (BallData ball in _balls)
                {
                    //have to compare with array num because can't access the og index once the object is in the new list
                    if (solidOptions[luckyIndex].arrayNum == ball.arrayNum)
                    {
                        _balls[ball.arrayNum].spawned = true;
                        //Debug.Log("Solid: " + ball.arrayNum);
                        return ball.arrayNum;
                    }
                }
            }
            else { lastBall = true; } //NOTE this one runs out first so have to check lastBall true if it runs out

        }


        //If ball is stripes
        else if (spawnIndex % 2 == 1 || lastBall)
        {
            //use this list to put available stripe balls in
            List<BallData> stripeOptions = new List<BallData>();

            foreach (BallData ball in _balls)
            {
                //find striped balls that haven't been spawned
                if (!ball.spawned && ball.stripes)
                {
                    stripeOptions.Add(ball);
                }
            }

            //Make sure there are still stripes to play
            if (stripeOptions.Count > 0)
            {
                //After building list pick a random one - set it spawned and return the index
                int luckyIndex = UnityEngine.Random.Range(0, stripeOptions.Count - 1);


                foreach (BallData ball in _balls)
                {
                    if (stripeOptions[luckyIndex].arrayNum == ball.arrayNum)
                    {
                        _balls[ball.arrayNum].spawned = true;
                        //Debug.Log("Stripe: " + ball.arrayNum);
                        return ball.arrayNum;
                    }
                }
            }

        }

        //error
        Debug.Log("No ball found");
        return 0;
    }


    private void BallCheck()
    {
        //Following code handles detecting if balls are stopped
        if (_checkBalls) // this bool is set to true shortly after a shot has been made
        {
            bool checkBallsStopped = true;

            //Check each ball and if any are moving it will set stopped bool to false
            foreach (GameObject ball in _ballObjects)
            {
                //check if ball has been removed from the game
                if (ball == null)
                {
                    _ballObjects.Remove(ball);
                    break; //break so that the foreach loop doesn't continue with a missing element
                }
                else
                {
                    //Look at the rigid body of every ball
                    Rigidbody ballRb = ball.GetComponent<Rigidbody>();
                    if (ballRb.linearVelocity.magnitude > minSpeed)
                    {
                        checkBallsStopped = false;
                    }
                    if (ballRb.angularVelocity.magnitude > minSpeed)
                    {
                        checkBallsStopped = false;
                    }
                }
            }

            //also check cue ball which is not part of the array
            Rigidbody cueBallRb = _balls[_balls.Length - 1].ballObject.GetComponent<Rigidbody>(); // it is the last index in the array
            if (cueBallRb.linearVelocity.magnitude > minSpeed)
            {
                checkBallsStopped = false;
            }
            if (cueBallRb.angularVelocity.magnitude > minSpeed)
            {
                checkBallsStopped = false;
            }

            //if balls are still wait a bit in case it was just a split second
            if (checkBallsStopped)
            {
                _timer += Time.fixedDeltaTime;
                //Debug.Log("Timer" + _timer);
                if (_timer >= _minWaitTime)
                {
                    _checkBalls = false;
                    BallsStoppedEvent(); // Used by inventory manager
                    //StartCoroutine(_playerScript.ShootReturns());
                    _timer = 0; // reset timer for next time
                }
            }
            else
            {
                _timer = 0;
            }
        }
    }

    private void ShotEventTriggered()
    {
        _checkBalls = true;
    }

    private void BallsStoppedEvent()
    {
        BallsStopped?.Invoke();
    }
}
