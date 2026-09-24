using UnityEngine;

public class PocketManager : MonoBehaviour
{
    //Scoreboard reference
    [SerializeField] private GameObject _scoreboardRef;
    private Scoreboard _scoreboardScript;

    //TurnManager reference
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;

    private void Awake()
    {
        _scoreboardScript = _scoreboardRef.GetComponent<Scoreboard>();
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
    }



    /// <summary>
    /// Custom on trigger so that the child objects can call it
    /// Get child collider so we know which pocket was hit -- will be useful for eight ball
    /// </summary>
    /// <param name="child"></param>
    /// <param name="other"></param>
    public void OnChildCollision(Collider child, Collider other)
    {

        switch (other.gameObject.tag)
        {
            case "StripedBall":
                {
                    //Check if team stripes has not been assigned
                    if (_turnManagerScript.teamStripes == 0)
                    {
                        _turnManagerScript.AssignStripes();
                    }

                    //keep or lose turn
                    _turnManagerScript.TurnDeciderStripes();

                    //Debug.Log("stripes" + _turnManagerScript.teamStripes);

                    Debug.Log("stripes");
                    Destroy(other.gameObject);

                    //increase score
                    AddStripeScore();

                    break;

                }


            case "SolidBall":
                {
                    //Check if team solids has not been assigned
                    if ( _turnManagerScript.teamSolids == 0)
                    {
                        _turnManagerScript.AssignSolids();
                    }

                    //keep or lose turn
                    _turnManagerScript.TurnDeciderSolids();

                    //Debug.Log("solids: " + _turnManagerScript.teamSolids);
                    //Check if team solids scored solid to keep turn

                    Debug.Log("solids");
                    Destroy(other.gameObject);

                    //increase score
                    AddSolidScore();

                    break;
                }
   

            case "EightBall":
                Debug.Log("EightBall");
                Destroy(other.gameObject);
                break;

            case "CueBall":
                Debug.Log("Scratch");
                _turnManagerScript.scratch = true;
                _turnManagerScript.keepTurn = false; // if scratch after scoring this will cover not keeping turn
                //other.gameObject.SetActive(false);
                break;
        }

        /*
        specific pocket detection
        if (child.gameObject.tag == "CornerPocketA")
        {
            Debug.Log("Corner Pocket");
        }*/
    }



    /// <summary>
    /// Functions for keeping score - doing it here for access to score text variables
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
