using UnityEngine;

public class Terrain_Generator : MonoBehaviour
{
    [SerializeField]
    GameObject[] terrains;

    [SerializeField]
    GameObject[] generationLocations;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Spawn Terrains based on how many spawn locations there are
        for (int i = 0; i < generationLocations.Length; i++)
        {
            //Spawn a terrain
            GenerateTerrainType(generationLocations[i]);
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

        //Spawn the randomly made terrain onto one of the spawn locations
        Instantiate(terrains[currTerrain], generationLocation.transform.position, Quaternion.identity);
    }
}
