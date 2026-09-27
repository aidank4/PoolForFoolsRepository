using System.Collections;
using System.Linq;
using UnityEngine;

public class Scratching : MonoBehaviour
{
    //TurnManager reference
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;

    [SerializeField]
    private GameObject[] scratchLocations;
    private int _scratchLocationIndex = 5;

    [SerializeField]
    private GameObject cueBallRef;

    private bool respawnBall;

    public void Update()
    {
        OnScratch(scratchLocations);
    }

    public void Awake()
    {
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
        respawnBall = false;
    }

    public void OnScratch(GameObject[] scratchLocation)
    {
        if(_turnManagerScript.scratch == true)
        {
            cueBallRef.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            cueBallRef.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            StartCoroutine(respawnTimer());
            cueBallRef.SetActive(false);
            

            if(respawnBall == true)
            {
                int scratchChosen = Random.Range(0, scratchLocations.Length);

                print("Scratch the ball");

                //Instantiate(cueBallRef, scratchLocation[scratchChosen].transform.position, Quaternion.identity);

                cueBallRef.transform.position = scratchLocation[scratchChosen].transform.position;
                cueBallRef.transform.rotation = Quaternion.identity;

                cueBallRef.SetActive(true);

                _turnManagerScript.scratch = false;

                respawnBall = false;
            }
            
        }
        else
        {
            print("Don't respawn ball");
        }
    }

    private IEnumerator respawnTimer()
    {
        print("start timer");

        yield return new WaitForSeconds(1f);

        respawnBall = true;
    }
}
