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
    //private int _scratchLocationIndex = 5; //this wasn't being used

    [SerializeField]
    private GameObject cueBallRef;

    private bool respawnBall;

    //Added this so coroutine doesn't get called by update repeatedly
    private bool _coroutineAvailable = true;

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
            //cueBallRef.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            //cueBallRef.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            if (_coroutineAvailable)
            {
                StartCoroutine(respawnTimer());
            }
            //cueBallRef.SetActive(false);

            //Instead of set active false, just hide the object and get rid of its collider
            //So that turn doesn't switch when it is set active false
            cueBallRef.GetComponent<SphereCollider>().enabled = false;
            cueBallRef.GetComponentInChildren<MeshRenderer>().enabled = false;
            cueBallRef.GetComponent<Rigidbody>().angularVelocity = Vector3.one; // keep it "moving" so turn doesnt switch
            

            if(respawnBall == true)
            {
                int scratchChosen = Random.Range(0, scratchLocations.Length);

                //print("Scratch the ball");

                //Instantiate(cueBallRef, scratchLocation[scratchChosen].transform.position, Quaternion.identity);

                cueBallRef.transform.position = scratchLocation[scratchChosen].transform.position;
                cueBallRef.transform.rotation = Quaternion.identity;
                
                //set velocity to 0
                cueBallRef.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                cueBallRef.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
                //cueBallRef.SetActive(true);
                //Un hide it rather than turn it off or on
                cueBallRef.GetComponent<SphereCollider>().enabled = true;
                cueBallRef.GetComponentInChildren<MeshRenderer>().enabled = true;


                _turnManagerScript.scratch = false;
                respawnBall = false;
                _coroutineAvailable = true;//new bool set it back true after scratch has been set false
            }
            
        }
        else
        {
            //print("Don't respawn ball");
        }
    }

    private IEnumerator respawnTimer()
    {
        _coroutineAvailable = false;
        //print("start timer");

        yield return new WaitForSeconds(1f);

        respawnBall = true;
    }
}
