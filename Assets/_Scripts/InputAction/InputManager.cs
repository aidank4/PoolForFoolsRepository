using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static PlayerInput Instance { get; private set; }
    private PlayerInput _playerInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = GetComponent<PlayerInput>();
        //Get rid of cursor which would fuck with buttons
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
