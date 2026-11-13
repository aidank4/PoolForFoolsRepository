using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _controlsUI;

    [Header("CameraSpeed")]
    [SerializeField] private float _movSpeed = 10f;
    [SerializeField] private float _rotSpeed = 10f;

    private Vector3 _moveInput;
    private Vector2 _rotateInput;

    private Vector3 _topDownPos;
    private Vector3 _topDownRot;

    private void Awake()
    {
        _controlsUI.SetActive(false);
        _topDownPos = new Vector3(0, 20, 2);
        _topDownRot = new Vector3(90, -90, 0);
    }


    /// <summary>
    /// Record movement input
    /// </summary>
    /// <param name="context"></param>
    public void Fly(InputAction.CallbackContext context)
    {
        Debug.Log("Fly");
        _moveInput = context.ReadValue<Vector3>();
    }


    /// <summary>
    /// Record rotation input
    /// </summary>
    /// <param name="context"></param>
    public void Rotate(InputAction.CallbackContext context)
    {
        Debug.Log("Rotate input");
        _rotateInput = context.ReadValue<Vector2>();
    }


    /// <summary>
    /// Return camera to starting position and rotation.
    /// </summary>
    /// <param name="context"></param>
    public void TopDownView(InputAction.CallbackContext context)
    {
        transform.position = _topDownPos;
        transform.rotation = Quaternion.Euler(_topDownRot);
    }


    /// <summary>
    /// Toggle camera controls text
    /// </summary>
    public void ToggleCamUI()
    {
        if (_controlsUI.activeSelf)
        {
            _controlsUI.SetActive(false);
        }
        else
        {
            _controlsUI.SetActive(true);
        }
    }


    private void Update()
    {
        transform.Translate(new Vector3(_moveInput.x, 0, _moveInput.z) * _movSpeed * Time.deltaTime, Space.Self); //WASD local
        transform.Translate(new Vector3(0, _moveInput.y, 0) * _movSpeed * Time.deltaTime, Space.World); //Space Shift up down world space

        transform.Rotate(new Vector3(0, _rotateInput.y, 0) * (_rotSpeed * Time.deltaTime), Space.World); //Yaw
        transform.Rotate(new Vector3(_rotateInput.x, 0, 0) * (_rotSpeed * Time.deltaTime), Space.Self); //Pitch
    }
}
