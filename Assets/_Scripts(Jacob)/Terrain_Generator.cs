using UnityEngine;

public class Terrain_Generator : MonoBehaviour
{
    [SerializeField]
    GameObject[] terrains;

    [SerializeField]
    GameObject[] generationLocations;

    [SerializeField]
    private Animator riseAnimator;

    [SerializeField]
    private Animation risingAnim;

    [SerializeField]
    private GameObject _ballManagerRef;
    private BallManager _ballManager;

    private bool _hasSpawned = false;

    void Awake()
    {
        _ballManager = _ballManagerRef.GetComponent<BallManager>();
        riseAnimator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (_ballManager != null)
        {
            _ballManager.BallsStopped += HandleBallsStopped;
        }
    }

    private void OnDisable()
    {
        if (_ballManager != null)
        {
            _ballManager.BallsStopped -= HandleBallsStopped;
        }
    }


    private void HandleBallsStopped(bool dontCare)
    {
        if (!_hasSpawned)
        {
            _hasSpawned = true;
            //FUNCTION
            //Spawn Terrains based on how many spawn locations there are
            for (int i = 0; i < generationLocations.Length; i++)
            {
                //Spawn a terrain
                GenerateTerrainType(generationLocations[i]);
            }
            riseAnimator.SetTrigger("Rise");
        }
    }

    //Generate a terrain randomly
    void GenerateTerrainType(GameObject generationLocation)
    {
        //Do not continue if there are no terrains
        if(terrains.Length == 0)
        {
            Debug.Log("No accessible terrains");
            return;
        }
        
        //Randomly select a type of terrain
        int currTerrain = Random.Range(0, terrains.Length);
        Debug.Log("Selected Terrains");

        //Spawn the randomly made terrain onto one of the spawn locations
        Instantiate(terrains[currTerrain], generationLocation.transform.position, Quaternion.identity, generationLocation.transform);
        Debug.Log("Spawn Terrains");
    }
}
