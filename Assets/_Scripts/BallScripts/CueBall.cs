using UnityEngine;

public class CueBall : MonoBehaviour
{
    //CURRENTLY NEEDED FOR CURVE SHOT
    [Header("References")]
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManager;


    //Misc
    public bool hit {  get; private set; } //Set true when cue ball has hit another ball


    private void Awake()
    {
        _turnManager = _turnManagerRef.GetComponent<TurnManager>();
    }


    /// <summary>
    /// Subscribe to turn switch event
    /// </summary>
    private void OnEnable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched += HandleTurnSwitch;
        }
    }


    /// <summary>
    /// Unsubscribe
    /// </summary>
    private void OnDisable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched -= HandleTurnSwitch;
        }
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "SolidBall" || collision.gameObject.tag == "StripedBall" || collision.gameObject.tag == "EightBall" || collision.gameObject.tag == "Wall")
        {
            hit = true;
        }
    }

    /// <summary>
    /// Reset hit bool when turn is switched
    /// </summary>
    /// <param name="sabotaged"></param>
    private void HandleTurnSwitch(bool sabotaged)
    {
        hit = false;
    }
}
