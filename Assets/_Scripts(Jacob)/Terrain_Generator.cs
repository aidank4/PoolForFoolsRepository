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
        for (int i = 0; i < generationLocations.Length; i++)
        {
            GenerateTerrainType(generationLocations[i]);
        }
    }

    void GenerateTerrainType(GameObject generationLocation)
    {
        if(terrains.Length == 0)
        {
            return;
        }
        
        int currTerrain = Random.Range(0, terrains.Length);

        Instantiate(terrains[currTerrain], generationLocation.transform.position, Quaternion.identity);
    }
}
