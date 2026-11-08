using System.Collections;
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
    private void ActivateFirework(Vector3 spawnLocation, bool sabotage)
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
    private void HideFireWork(bool sabotage)
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


    /// <summary>
    /// Play PU or sabotage firework at pos
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="sabotage"></param>
    /// <returns></returns>
    public IEnumerator PlayFireworks(Vector3 pos, bool sabotage)
    {
        ActivateFirework(pos, sabotage);
        yield return new WaitForSeconds(4f);
        HideFireWork(sabotage);
    }
}
