using UnityEngine;

public class PocketManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject _scoreboardRef;
    private Scoreboard _scoreboardScript;
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;



    private void Awake()
    {
        //ref
        #region
        _scoreboardScript = _scoreboardRef.GetComponent<Scoreboard>();
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
        #endregion
    }


    /// <summary>
    /// Handles pockets collisions with different types of balls.
    /// </summary>
    /// <param name="child"></param>
    /// <param name="other"></param>
    public void OnChildCollision(Collider child, Collider other)
    {
        switch (other.gameObject.tag)
        {
            case "StripedBall":
                {
                    if (_turnManagerScript.teamStripes == 0) //Check if team stripes has not been assigned
                    {
                        _turnManagerScript.AssignStripes();
                    }

                    _turnManagerScript.TurnDeciderStripes();
                    Destroy(other.gameObject);
                    AddStripeScore();
                    break;
                }
            case "SolidBall":
                {
                    if ( _turnManagerScript.teamSolids == 0) //Check if team solids has not been assigned
                    {
                        _turnManagerScript.AssignSolids();
                    }

                    _turnManagerScript.TurnDeciderSolids();
                    Destroy(other.gameObject);
                    AddSolidScore();
                    break;
                }
            case "EightBall":
                {
                    Debug.Log("EightBall");
                    if(_scoreboardScript.pOneScore == 7 && _turnManagerScript.currentPlayer == _turnManagerScript.playerOne)
                    {
                        Debug.Log("Player 1 Wins!");
                        //display P1 win screen
                    }
                    else if(_scoreboardScript.pTwoScore == 7 && _turnManagerScript.currentPlayer == _turnManagerScript.playerTwo)
                    {
                        Debug.Log("Player 2 Wins!");
                        //display P2 win screen
                    }
                    else
                    {
                        Debug.Log("Player " + _turnManagerScript.currentPlayer.ToString() + " loses");
                        if(_turnManagerScript.currentPlayer == _turnManagerScript.playerOne)
                        {
                            //display P2 win screen
                        }
                        else if(_turnManagerScript.currentPlayer == _turnManagerScript.playerTwo)
                        {
                            //display P1 win screen
                        }
                    }
                        Destroy(other.gameObject);
                    break;
                }
            case "CueBall":
                {
                    _turnManagerScript.scratch = true;
                    _turnManagerScript.keepTurn = false;
                    break;
                }
        }
    }


    /// <summary>
    /// Increment stripes score +1
    /// </summary>
    public void AddStripeScore()
    {
        if (_turnManagerScript.playerOne == _turnManagerScript.teamStripes)
        {
            _scoreboardScript.pOneScore++;
        }
        else
        {
            _scoreboardScript.pTwoScore++;
        }
    }


    /// <summary>
    /// Increments solid score +1
    /// </summary>
    public void AddSolidScore()
    {
        if (_turnManagerScript.playerOne == _turnManagerScript.teamSolids)
        {
            _scoreboardScript.pOneScore++;
        }
        else
        {
            _scoreboardScript.pTwoScore++;
        }
    }
}
