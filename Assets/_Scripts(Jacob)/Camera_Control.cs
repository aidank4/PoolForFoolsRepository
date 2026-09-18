using UnityEngine;

public class Camera_Control : MonoBehaviour
{
    /*[SerializeField]
    private GameObject _camera;*/
    [SerializeField]
    private Animator _cameraAnim;
    
    [SerializeField]
    private Animation _anim;

    private bool _topDown;
    private bool _isometric;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // _cameraAnim = _camera.GetComponent<Animator>();
        _anim = _cameraAnim.GetComponent<Animation>();
        _isometric = true;
        _topDown = false;
    }

    // Update is called once per frame
    void Update()
    {
        playCameraAnim();
    }

    private void playCameraAnim()
    {
        if ((Input.GetKeyDown(KeyCode.Tab)))
        {
            if (_isometric)
            {
                _cameraAnim.SetTrigger("Swap");
                _topDown = true;
                _isometric = false;
                Debug.Log("Top Down");
            }
            else if (_topDown)
            {
                _cameraAnim.SetTrigger("Reverse");
                _topDown = false;
                _isometric = true;
                Debug.Log("Isometric");
            }

        }
    }
}
