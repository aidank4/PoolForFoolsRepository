using UnityEngine;

public class PocketManager : MonoBehaviour
{
    [SerializeField] private GameObject _scoreboardRef;
    private Scoreboard _scoreboardScript;

    private void Awake()
    {
        _scoreboardScript = _scoreboardRef.GetComponent<Scoreboard>();
    }



    /// <summary>
    /// Custom on trigger so that the child objects can call it
    /// Get child collider so we know which pocket was hit -- will be useful for eight ball
    /// </summary>
    /// <param name="child"></param>
    /// <param name="other"></param>
    public void OnChildCollision(Collider child, Collider other)
    {

        if (other.gameObject.tag == "StripedBall")
        {
            Debug.Log("pocketed");
            Destroy(other.gameObject);
            _scoreboardScript.stripeScore += 1;
        }

        if (other.gameObject.tag == "SolidBall")
        {
            Debug.Log("pocketed");
            Destroy(other.gameObject);
            _scoreboardScript.solidScore += 1;
        }


        if (other.gameObject.tag == "EightBall")
        {
            Debug.Log("EightBall");
            Destroy(other.gameObject);
        }

        if (other.gameObject.tag == "CueBall")
        {
            Debug.Log("Scratch");
            //other.gameObject.SetActive(false);
        }

        /* specific pocket detection
        if (child.gameObject.tag == "CornerPocketA")
        {
            Debug.Log("Corner Pocket");
        }*/
    }
}
