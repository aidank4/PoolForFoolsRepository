using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect _explosion;
    [SerializeField] private VisualEffect _rocket;


    //misc
    private bool _playing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hide
        #region
        _explosion.gameObject.SetActive(false);
        _rocket.gameObject.SetActive(false);
        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        if (_playing)
        {
            if (_explosion.aliveParticleCount == 0)
            {
                _explosion.gameObject.SetActive(false);
                _playing = false;
            }
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
        _playing = true;
    }


    /// <summary>
    /// Turn on rocket VFX and reparent it to parameter
    /// </summary>
    /// <param name="parent"></param>
    public void RocketOn(GameObject parent)
    {
        _rocket.gameObject.transform.SetParent(parent.transform, false);
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
}
