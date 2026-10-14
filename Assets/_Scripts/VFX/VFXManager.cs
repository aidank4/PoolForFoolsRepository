using UnityEngine;
using UnityEngine.VFX;

public class VFXManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VisualEffect _explosion;


    //misc
    private bool _playing = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Hide
        #region
        _explosion.gameObject.SetActive(false);
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

    //Functions for playing them
    public void ExplosionEffect(Vector3 playPos)
    {
        _explosion.transform.position = playPos;
        _explosion.gameObject.SetActive(true);
        _playing = true;
    }
}
