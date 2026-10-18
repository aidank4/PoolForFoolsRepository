using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect _explosion;
    [SerializeField] private VisualEffect _rocket;
    [SerializeField] private VisualEffect _magnet;


    private bool _magnetPlaying = false;
    private bool _explosionPlaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hide
        #region
        _explosion.gameObject.SetActive(false);
        _rocket.gameObject.SetActive(false);
        _magnet.gameObject.SetActive(false);
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (_explosionPlaying)
        {
            if (_explosion.aliveParticleCount == 0)
            {
                _explosion.gameObject.SetActive(false);
                _explosionPlaying = false;
            }
        }
        if (_magnetPlaying)
        {
            _magnet.gameObject.transform.rotation = Quaternion.identity;
        }
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
        _rocket.gameObject.transform.rotation = Quaternion.LookRotation(parent.up);
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
        _magnet.gameObject.transform.SetParent(parent, false);
        //_magnet.gameObject.transform.rotation.
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
}
