using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;

public class BallManager : MonoBehaviour
{
    /*
    protected float minSpeed = 0.2f;
    private float _timer;
    private float _minWaitTime = 0.5f;
    
    [SerializeField] private Rigidbody[] _balls;
    */

    //[SerializeField] private GameObject _testBall;
    [SerializeField] private GameObject _startingPos;
    private float _ballDiameter = 0.5715f;
    private int _rows = 5;

    [SerializeField] private BallSO[] _balls;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        //_balls = GetComponentsInChildren<Rigidbody>();
        SpawnBalls();
    }

    
    // Update is called once per frame
    private void FixedUpdate()
    {
        /*bool ballsStopped = true;
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
        }*/
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
                float zValue = rowSeperation * row;
                float yValue = 0f;

                //calculate the x value by multiplying ball diameter by + or - a certain distance // gets either +- 0, 0.5, 1, 1.5 
                float xValue = (i - (ballsInRow - 1) / 2f) * _ballDiameter;

                Vector3 spawnPos = _startingPos.transform.position - new Vector3(xValue, yValue, zValue); // set it to behind the starting position movable object

                //spawn
                //Instantiate(_testBall, spawnPos, Quaternion.identity, transform.parent);
                Instantiate(_balls[SelectedBall(spawnIndex)].ballObject, spawnPos, Quaternion.identity);
                
                
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
            foreach (BallSO ball in _balls)
            {
                //find eightball
                if (ball.eightBall)
                {
                    return ball.arrayNum;
                }
            }
        }

        //If ball is solids
        else if (spawnIndex % 2 == 0 || lastBall)
        {
            //use this list to put available solid balls in
            List<BallSO> solidOptions = new List<BallSO>();

            foreach (BallSO ball in _balls)
            {
                //find solid balls that haven't been spawned
                if (!ball.spawned && !ball.stripes)
                {
                    solidOptions.Add(ball);
                }
            }

            //Make sure there are still solids to play
            if (solidOptions.Count > 0)
            {
                //After building list pick a random one - set it spawned and return the index
                int luckyIndex = UnityEngine.Random.Range(0, solidOptions.Count - 1);

                // there has to be a better way of doing this --- maybe giving each ball a number
                foreach (BallSO ball in _balls)
                {
                    if (solidOptions[luckyIndex].name == ball.name)
                    {
                        ball.spawned = true;
                        Debug.Log("Solid: " + ball.arrayNum);
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
            List<BallSO> stripeOptions = new List<BallSO>();

            foreach (BallSO ball in _balls)
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

                // there has to be a better way of doing this --- maybe giving each ball a number
                foreach (BallSO ball in _balls)
                {
                    if (stripeOptions[luckyIndex].name == ball.name)
                    {
                        ball.spawned = true;
                        Debug.Log("Stripe: " + ball.arrayNum);
                        return ball.arrayNum;
                    }
                }
            }

        }

        //error
        Debug.Log("No ball found");
        return 0;
    }
}
