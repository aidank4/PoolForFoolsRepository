using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _fireworkPU;
    [SerializeField] private GameObject _fireworkSabotage;
    [SerializeField] private VisualEffect _explosion;
    [SerializeField] private VisualEffect _rocket;
    [SerializeField] private VisualEffect _magnet;
    [SerializeField] private VisualEffect _endPocket;


    private bool _magnetPlaying = false; //Magnet Vfx is on
    private bool _explosionPlaying = false; //Explosive Vfx is on


    void Awake()
    {
        //Hide
        #region
        _fireworkPU.SetActive(false);
        _fireworkSabotage.SetActive(false);
        _explosion.gameObject.SetActive(false);
        _rocket.gameObject.SetActive(false);
        _magnet.gameObject.SetActive(false);
        _endPocket.gameObject.SetActive(false);
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (_explosionPlaying)
        {
            if (_explosion.aliveParticleCount == 0) //Turn off explosive VFX once it ends
            {
                _explosion.gameObject.SetActive(false);
                _explosionPlaying = false;
            }
        }
        if (_magnetPlaying) //Keep magnet still so it doesn't rotate with the ball
        {
            _magnet.gameObject.transform.rotation = Quaternion.identity;
        }
    }


    /// <summary>
    /// Set position and turn on firework effect
    /// </summary>
    /// <param name="sabotage"></param>
    /// <param name="spawnLocation"></param>
    private void ActivateFirework(Vector3 spawnLocation, bool sabotage)
    {
        if (!sabotage)
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


    /// <summary>
    /// Turn on explosion VFX and set its position to parameter
    /// </summary>
    /// <param name="playPos"></param>
    public void ExplosionEffect(Vector3 playPos)
    {
        _explosion.transform.position = playPos;
        _explosion.gameObject.SetActive(true);
        _explosionPlaying = true;
    }


    /// <summary>
    /// Turn on rocket VFX and reparent it to parameter
    /// </summary>
    /// <param name="parent"></param>
    public void RocketOn(Transform parent)
    {
        _rocket.gameObject.transform.SetParent(parent, false);
        _rocket.gameObject.transform.rotation = Quaternion.LookRotation(parent.up); //parent is pool cue and up faces the rocket correctly
        _rocket.gameObject.SetActive(true);
    }


    /// <summary>
    /// Turn off rocket VFX and reparent it to VFXManager
    /// </summary>
    public void RocketOff()
    {
        _rocket.gameObject.transform.SetParent(this.gameObject.transform, false);
        _rocket.gameObject.SetActive(false);
    }


    /// <summary>
    /// Set magnet to child of param and turn it on
    /// </summary>
    /// <param name="parent"></param>
    public void MagnetOn(Transform parent)
    {
        _magnetPlaying = true;
        _magnet.gameObject.transform.SetParent(parent, false); //Cue ball is parent
        _magnet.gameObject.SetActive(true);
    }


    /// <summary>
    /// Reparent magnet and turn it off
    /// </summary>
    public void MagnetOff()
    {
        _magnetPlaying = false;
        _magnet.gameObject.transform.SetParent(this.gameObject.transform, false);
        _magnet.gameObject.SetActive(false);
    }


    /// <summary>
    /// Place the end pocket vfx at a position
    /// </summary>
    /// <param name="spawnPos"></param>
    public void PocketVFXPos(Vector3 spawnPos)
    {
        _endPocket.transform.position = new Vector3(spawnPos.x, _endPocket.transform.position.y, spawnPos.z); //keep y
    }


    /// <summary>
    /// Turn on end pocket vfx
    /// </summary>
    /// <param name="spawnPos"></param>
    public void PocketVFXOn()
    {
        _endPocket.gameObject.SetActive(true);
    }


    /// <summary>
    /// Turn off end pocket vfx
    /// </summary>
    /// <param name="spawnPos"></param>
    public void PocketVFXOff()
    {
        _endPocket.gameObject.SetActive(false);
    }
}
