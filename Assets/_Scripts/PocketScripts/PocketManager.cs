using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PocketManager : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private GameObject _scoreboardRef;
    private Scoreboard _scoreboardScript;
    [SerializeField] private GameObject _turnManagerRef;
    private TurnManager _turnManagerScript;
    [SerializeField] private GameObject _inventoryRef;
    private InventoryManager _inventoryScript;
    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private Image _controlsUI; //Controls text when selecting a pocket to interact with
    [SerializeField] private GameObject _fireworkRef;
    private FireWorks _fireworkScript;
    [SerializeField] private Image _winScreenPOne;
    [SerializeField] private Image _winScreenPTwo;


    [Header("Collections")]
    public List<GameObject> pockets = new List<GameObject>();
    [SerializeField] private List<GameObject> _corks = new List<GameObject>();


    [Header("Misc Variables")]
    private bool _choosingPocket = false; //When true the player can cycle through pocket selections
    private int _pocketIdx;


    //Events
    public event Action Scratched;


    private void Awake()
    {
        //ref
        #region
        _scoreboardScript = _scoreboardRef.GetComponent<Scoreboard>();
        _turnManagerScript = _turnManagerRef.GetComponent<TurnManager>();
        _inventoryScript = _inventoryRef.GetComponent<InventoryManager>();
        _fireworkScript = _fireworkRef.GetComponent<FireWorks>();
        #endregion
        //UI / Hide
        #region
        _highlightObject.SetActive(false);
        _controlsUI.gameObject.SetActive(false);
        _winScreenPOne.gameObject.SetActive(false);
        _winScreenPTwo.gameObject.SetActive(false);
        foreach(GameObject cell in _corks)
        {
            cell.SetActive(false);
        }
        #endregion
    }


    /// <summary>
    /// Subscribe to events
    /// </summary>
    private void OnEnable()
    {
        SO_PowerUp.UsePU += HandlePUs;
    }


    /// <summary>
    /// Unsubscribe
    /// </summary>
    private void OnDisable()
    {
        SO_PowerUp.UsePU -= HandlePUs;
    }


    /// <summary>
    /// Handles pockets collisions with different types of balls.
    /// </summary>
    /// <param name="child"></param>
    /// <param name="other"></param>
    public void OnChildCollision(Collider child, Collider other) //Called by pocket script on collision
    {
        switch (other.gameObject.tag)
        {
            case "StripedBall":
                {
                    if (_turnManagerScript.teamStripes == 0) //Check if team stripes has not been assigned
                    {
                        _turnManagerScript.AssignStripes();
                    }
                    if (_turnManagerScript.currentPlayer == _turnManagerScript.teamStripes) //Add powerUp and playfireworks
                    {
                        _inventoryScript.AddPowerUp();
                        StartCoroutine(PlayFireworks(child.gameObject.transform.position, false));
                    }
                    else //Play sabotage fireworks if player scored wrong ball
                    {
                        StartCoroutine(PlayFireworks(child.gameObject.transform.position, true));
                    }

                    _turnManagerScript.TurnDeciderStripes();
                    Destroy(other.gameObject); //ball
                    AddStripeScore();
                    break;
                }
            case "SolidBall":
                {
                    if ( _turnManagerScript.teamSolids == 0) //Check if team solids has not been assigned
                    {
                        _turnManagerScript.AssignSolids();
                    }

                    if (_turnManagerScript.currentPlayer == _turnManagerScript.teamSolids)//Add powerUp and playfireworks
                    {
                        _inventoryScript.AddPowerUp();
                        StartCoroutine(PlayFireworks(child.gameObject.transform.position, false));
                    }
                    else //Play sabotage fireworks if player scored wrong ball
                    {
                        StartCoroutine(PlayFireworks(child.gameObject.transform.position, true));
                    }

                    _turnManagerScript.TurnDeciderSolids();
                    Destroy(other.gameObject); //ball
                    AddSolidScore();
                    break;
                }
            case "EightBall":
                {
                    Debug.Log("EightBall");
                    //TEMP player has scored all the balls and 8 ball
                    if(_scoreboardScript.pOneScore == 7 && _turnManagerScript.currentPlayer == _turnManagerScript.playerOne)
                    {
                        //Debug.Log("Player 1 Wins!");
                        _winScreenPOne.gameObject.SetActive(true);
                    }
                    else if(_scoreboardScript.pTwoScore == 7 && _turnManagerScript.currentPlayer == _turnManagerScript.playerTwo)
                    {
                        //Debug.Log("Player 2 Wins!");
                        _winScreenPTwo.gameObject.SetActive(true);
                    }
                    else //TEMP player scored 8 ball early
                    {
                        //Debug.Log("Player " + _turnManagerScript.currentPlayer.ToString() + " loses");
                        if(_turnManagerScript.currentPlayer == _turnManagerScript.playerOne)
                        {
                            _winScreenPTwo.gameObject.SetActive(true);
                        }
                        else if(_turnManagerScript.currentPlayer == _turnManagerScript.playerTwo)
                        {
                            _winScreenPOne.gameObject.SetActive(true);
                        }
                    }
                    Destroy(other.gameObject); //Ball
                    break;
                }
            case "CueBall":
                {
                    _turnManagerScript.scratch = true;
                    _turnManagerScript.keepTurn = false;
                    Scratched?.Invoke();
                    break;
                }
        }
    }


    /// <summary>
    /// Increment stripes score +1
    /// </summary>
    private void AddStripeScore()
    {
        if (_turnManagerScript.playerOne == _turnManagerScript.teamStripes)
        {
            _scoreboardScript.pOneScore++;
        }
        else
        {
            _scoreboardScript.pTwoScore++;
        }
    }


    /// <summary>
    /// Increments solid score +1
    /// </summary>
    private void AddSolidScore()
    {
        if (_turnManagerScript.playerOne == _turnManagerScript.teamSolids)
        {
            _scoreboardScript.pOneScore++;
        }
        else
        {
            _scoreboardScript.pTwoScore++;
        }
    }


    /// <summary>
    /// Play a firework at a certain position for 4 seconds
    /// </summary>
    /// <param name="pos"></param>
    IEnumerator PlayFireworks(Vector3 pos, bool sabotage) //Should Move this to firework script script
    {
        _fireworkScript.ActivateFirework(sabotage, pos);
        yield return new WaitForSeconds(4f);
        _fireworkScript.HideFireWork(sabotage);
    }


    /// <summary>
    /// Cycle through the pockets going left
    /// </summary>
    /// <param name="context"></param>
    public void CycleLeft(InputAction.CallbackContext context) //NOTE used by player input
    {
        if (context.performed)
        {
            if (_choosingPocket)
            {
                if (_pocketIdx == pockets.Count - 1) //Restart if at the end
                {
                    _pocketIdx = 0;
                    HighlightPocket();
                }
                else
                {
                    _pocketIdx++;
                    HighlightPocket();
                }
            }
        }
    }


    /// <summary>
    /// Cycle through the pockets going right
    /// </summary>
    /// <param name="context"></param>
    public void CycleRight(InputAction.CallbackContext context) //NOTE used by player input
    {
        if (context.performed)
        {
            if (_choosingPocket)
            {
                if (_pocketIdx == 0) //Restart if at the end
                {
                    _pocketIdx = pockets.Count - 1;
                    HighlightPocket();
                }
                else
                {
                    _pocketIdx--;
                    HighlightPocket();
                }
            }
        }
    }


    /// <summary>
    /// Starts the jail cell coroutine and removes the choosen pocket from the game
    /// </summary>
    /// <param name="context"></param>
    public void ConfirmPocket(InputAction.CallbackContext context) //NOTE used by player input
    {
        if (context.performed)
        {
            if (_choosingPocket)
            {
                _controlsUI.gameObject.SetActive(false); //UI
                _choosingPocket = false;
                _highlightObject.SetActive(false);

                StartCoroutine(PlaceCork(_corks[0], pockets[_pocketIdx].gameObject.transform.position)); //0th cork because we just go until theres none - all corks the same
                pockets[_pocketIdx].SetActive(false);
                pockets.Remove(pockets[_pocketIdx]);
            }
        }
    }


    /// <summary>
    /// Spawn a cork above the chosen pocket.
    /// Then move it onto the pocket and switch inputs back to gameplay.
    /// </summary>
    /// <param name="cork"></param>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    IEnumerator PlaceCork(GameObject cork,Vector3 targetPos)
    {
        float timer = 0f;
        float maxTime = 3f;
        cork.transform.position = targetPos + new Vector3(0, 5, 0); // set jail cell pos above target pocket - Should make 5 a variable for designer
        cork.SetActive(true);
        Vector3 startingPos = cork.transform.position;
        while (timer < maxTime)
        {
            float percent = timer / maxTime;
            cork.transform.position = Vector3.Lerp(startingPos, targetPos, percent); // move toward pocket over time
            cork.transform.Rotate(new Vector3(0, 90, 0) * Time.deltaTime); //Spin cork
            timer += Time.deltaTime;
            yield return null;

        }
        cork.transform.position = targetPos; //Set pos exactly on pocket
        cork.transform.rotation = Quaternion.identity;
        _corks.Remove(cork);
        InputManager.Instance.SwitchCurrentActionMap("Gameplay");
    }


    /// <summary>
    /// Switch the input map and highlight the first pocket
    /// </summary>
    private void StartChoosingPocket()
    {
        InputManager.Instance.SwitchCurrentActionMap("PocketPick");
        _controlsUI.gameObject.SetActive(true);
        _choosingPocket = true;
        _pocketIdx = 0;
        _highlightObject.SetActive(true);
        HighlightPocket();
    }


    /// <summary>
    /// Set pocket as selected and move the highlight object position to the pocket
    /// </summary>
    private void HighlightPocket()
    {
        EventSystem.current.SetSelectedGameObject(pockets[_pocketIdx]);
        _highlightObject.transform.position = pockets[_pocketIdx].transform.position;
    }


    /// <summary>
    /// If pocket block PU this calls ChoosePocket
    /// </summary>
    /// <param name="powerUp"></param>
    private void HandlePUs(SO_PowerUp powerUp)
    {
        if (powerUp.id == "Pocket Block")
        {
            StartChoosingPocket();
        }
    }
}
