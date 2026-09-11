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

                    Debug.Log("stripes" + _turnManagerScript.teamStripes);
                    //Check if team stripes scored stripe and keeps turn

                    Debug.Log("stripes");
                    Destroy(other.gameObject);
                    _scoreboardScript.stripeScore += 1;
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

                    Debug.Log("solids: " + _turnManagerScript.teamSolids);
                    //Check if team solids scored solid to keep turn

                    Debug.Log("solids");
                    Destroy(other.gameObject);
                    _scoreboardScript.solidScore += 1;
                    break;
                }
   

            case "EightBall":
                Debug.Log("EightBall");
                Destroy(other.gameObject);
                break;

            case "CueBall":
                Debug.Log("Scratch");
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
}
