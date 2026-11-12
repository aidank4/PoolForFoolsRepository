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

    private Vector3 _startingPos;
    private Vector3 _startingRot;

    private void Awake()
    {
        _controlsUI.SetActive(false);
        _startingPos = transform.position;
        _startingRot = transform.rotation.eulerAngles;
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
    public void ResetView(InputAction.CallbackContext context)
    {
        transform.position = _startingPos;
        transform.rotation = Quaternion.Euler(_startingRot);
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
        transform.Translate(_moveInput * _movSpeed * Time.deltaTime, Space.Self);

        transform.Rotate(new Vector3(0, _rotateInput.y, 0) * (_rotSpeed * Time.deltaTime), Space.World); //Yaw
        transform.Rotate(new Vector3(_rotateInput.x, 0, 0) * (_rotSpeed * Time.deltaTime), Space.Self); //Pitch
    }
}
