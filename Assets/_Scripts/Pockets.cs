using UnityEngine;

public class Pockets : MonoBehaviour
{
    private PocketManager _pocketManagerRef;

    private void Awake()
    {
        _pocketManagerRef = GetComponentInParent<PocketManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        //On trigger call parent function that handles what happens
        _pocketManagerRef.OnChildCollision(GetComponent<Collider>(), other);
    }
}
