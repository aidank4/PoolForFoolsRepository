using Unity.VisualScripting;
using UnityEngine;

public class TerrainSpawn : MonoBehaviour
{
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        _turnManager = _turnManagerRef.GetComponent<TurnManager>();
    }

    private void OnEnable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched += HandleTurnSwitch;
        }
    }

    private void OnDisable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnSwitched -= HandleTurnSwitch;
        }
    }


    private void HandleTurnSwitch(bool dontCare)
    {

    }
}
