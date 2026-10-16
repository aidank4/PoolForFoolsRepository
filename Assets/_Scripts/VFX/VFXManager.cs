using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect _explosion;
    [SerializeField] private VisualEffect _rocket;


    //misc
    //private static readonly int lifetimeID = Shader.PropertyToID("Lifetime");
    //private float _startingLifeTime;
    //private float _lifeTime;
    private bool _playing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hide
        #region
        _explosion.gameObject.SetActive(false);
        _rocket.gameObject.SetActive(false);
        #endregion

        //_lifeTime = _rocket.GetFloat(lifetimeID);
        //_startingLifeTime = _lifeTime;
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


    //private IEnumerator RocketVFXDim()
    //{
    //    float timer = 0f;
    //    float maxTime = 1f;
    //    while (timer < maxTime)
    //    {
    //        timer += Time.deltaTime;
    //        _lifeTime -= _lifeTime / 100f;
    //        _rocket.SetFloat(lifetimeID, _lifeTime);
    //        yield return null;
    //    }
    //    _rocket.gameObject.transform.SetParent(this.gameObject.transform, false);
    //    _rocket.gameObject.SetActive(false);
    //    _lifeTime = _startingLifeTime;
    //    _rocket.SetFloat(lifetimeID, _lifeTime);
    //}
}
