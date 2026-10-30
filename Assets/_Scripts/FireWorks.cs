using UnityEngine;

public class FireWorks : MonoBehaviour
{
    [SerializeField] private GameObject _fireworkPU;
    [SerializeField] private GameObject _fireworkSabotage;

    private void Awake()
    {
        _fireworkPU.SetActive(false);
        _fireworkSabotage.SetActive(false);
    }


    /// <summary>
    /// Set position and turn on firework effect
    /// </summary>
    /// <param name="sabotage"></param>
    /// <param name="spawnLocation"></param>
    public void ActivateFirework(bool sabotage, Vector3 spawnLocation)
    {
        if(!sabotage)
        {
            _fireworkPU.transform.position = spawnLocation;
            _fireworkPU.SetActive(true);
        }
        else
        {
            _fireworkSabotage.transform.position = spawnLocation;
            _fireworkSabotage.SetActive(true);
        }
    }


    /// <summary>
    /// Set the particle to false
    /// </summary>
    /// <param name="sabotage"></param>
    public void HideFireWork(bool sabotage)
    {
        if (!sabotage)
        {
            _fireworkPU.SetActive(false);
        }
        else
        {
            _fireworkSabotage.SetActive(false);
        }
    }
}
