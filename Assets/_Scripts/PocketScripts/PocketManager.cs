using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
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
    private TurnManager _turnManager;
    [SerializeField] private GameObject _inventoryRef;
    private InventoryManager _inventory;
    [SerializeField] private GameObject _highlightObject;
    [SerializeField] private GameObject _tableCenter;
    [SerializeField] private Image _controlsUI; //Controls text when selecting a pocket to interact with
    [SerializeField] private TMP_Text _controlsUIDesc; //description
    [SerializeField] private GameObject _fireworkRef;
    private FireWorks _fireworkScript;
    [SerializeField] private GameObject _cork;
    [SerializeField] private Image _winScreenPOne;
    [SerializeField] private Image _winScreenPTwo;


    [Header("Collections")]
    public List<GameObject> pockets = new List<GameObject>();
    [SerializeField] private List<GameObject> _corks = new List<GameObject>();
    private List<GameObject> _tempCoveredPockets = new List<GameObject>();


    [Header("Misc Variables")]
    [SerializeField] private float _pocketPushRadius = 3f;
    [SerializeField] private float _pocketPushForce = 2f;
    private bool _choosingPocket = false; //When true the player can cycle through pocket selections
    private int _pocketIdx;

    [Header("PowerUps")]
    private bool _pocketBlockActive = false; //For differentiating what to do when a pocket is chosen
    public bool pocketsCovered { get; private set; } = false; //used to know when to release covered pockets at end of turn
    private bool _coverPockets = false; //Used to know when to cover pockets on turn switch


    //Events
    public event Action Scratched;
    public event Action PocketSelected;
    public event Action PocketExplosion;


    private void Awake()
    {
        //ref
        #region
        _scoreboardScript = _scoreboardRef.GetComponent<Scoreboard>();
        _turnManager = _turnManagerRef.GetComponent<TurnManager>();
        _inventory = _inventoryRef.GetComponent<InventoryManager>();
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
        if (_turnManager != null)
        {
            _turnManager.TurnEnding += HandleTurnEnding;
            _turnManager.TurnSwitched += HandleTurnSwitched;
        }
        SO_PowerUp.UsePU += HandlePUs;
    }


    /// <summary>
    /// Unsubscribe
    /// </summary>
    private void OnDisable()
    {
        if (_turnManager != null)
        {
            _turnManager.TurnEnding -= HandleTurnEnding;
            _turnManager.TurnSwitched -= HandleTurnSwitched;
        }
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
                    if (_turnManager.teamStripes == 0) //Check if team stripes has not been assigned
                    {
                        _turnManager.AssignStripes();
                    }
                    if (_turnManager.currentPlayer == _turnManager.teamStripes) //Add powerUp and playfireworks
                    {
                        _inventory.AddPowerUp();
                        StartCoroutine(PlayFireworks(child.gameObject.transform.position, false));
                    }
                    else //Play sabotage fireworks if player scored wrong ball
                    {
                        StartCoroutine(PlayFireworks(child.gameObject.transform.position, true));
                    }

                    _turnManager.TurnDeciderStripes();
                    Destroy(other.gameObject); //ball
                    AddStripeScore();
                    break;
                }
            case "SolidBall":
                {
                    if ( _turnManager.teamSolids == 0) //Check if team solids has not been assigned
                    {
                        _turnManager.AssignSolids();
                    }

                    if (_turnManager.currentPlayer == _turnManager.teamSolids)//Add powerUp and playfireworks
                    {
                        _inventory.AddPowerUp();
                        StartCoroutine(PlayFireworks(child.gameObject.transform.position, false));
                    }
                    else //Play sabotage fireworks if player scored wrong ball
                    {
                        StartCoroutine(PlayFireworks(child.gameObject.transform.position, true));
                    }

                    _turnManager.TurnDeciderSolids();
                    Destroy(other.gameObject); //ball
                    AddSolidScore();
                    break;
                }
            case "EightBall":
                {
                    Debug.Log("EightBall");
                    //TEMP player has scored all the balls and 8 ball
                    if(_scoreboardScript.pOneScore == 7 && _turnManager.currentPlayer == _turnManager.playerOne)
                    {
                        //Debug.Log("Player 1 Wins!");
                        _winScreenPOne.gameObject.SetActive(true);
                    }
                    else if(_scoreboardScript.pTwoScore == 7 && _turnManager.currentPlayer == _turnManager.playerTwo)
                    {
                        //Debug.Log("Player 2 Wins!");
                        _winScreenPTwo.gameObject.SetActive(true);
                    }
                    else //TEMP player scored 8 ball early
                    {
                        //Debug.Log("Player " + _turnManagerScript.currentPlayer.ToString() + " loses");
                        if(_turnManager.currentPlayer == _turnManager.playerOne)
                        {
                            _winScreenPTwo.gameObject.SetActive(true);
                        }
                        else if(_turnManager.currentPlayer == _turnManager.playerTwo)
                        {
                            _winScreenPOne.gameObject.SetActive(true);
                        }
                    }
                    Destroy(other.gameObject); //Ball
                    break;
                }
            case "CueBall":
                {
                    _turnManager.scratch = true;
                    _turnManager.keepTurn = false;
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
        if (_turnManager.playerOne == _turnManager.teamStripes)
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
        if (_turnManager.playerOne == _turnManager.teamSolids)
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
                if (_pocketBlockActive) //Pocket Block - This must be above because both PUs could be called on same turn but this one would play first
                {
                    RemovePocket();
                    _pocketBlockActive = false;
                }
                else if (_turnManager.ctsActive == true) //Call their shot
                {
                    _controlsUI.gameObject.SetActive(false); //UI
                    _highlightObject.SetActive(false);

                    _choosingPocket = false;
                    _coverPockets = true;
                    _turnManager.ctsActive = false;
                    StartCoroutine(CallPocketSelected()); //Must wait till end of frame or else this input gets registered in the UI that appears after the event trigger
                }
            }
        }
    }


    /// <summary>
    /// Cover every pocket except the pocket at pocketIdx
    /// </summary>
    private void CoverPockets()
    {
        for (int i = 0; i < pockets.Count; i++)
        {
            if (i != _pocketIdx) //skip selected pocket
            {
                StartCoroutine(PlaceCork(pockets[i], false));
                pockets[i].gameObject.GetComponent<Collider>().enabled = false;
                _tempCoveredPockets.Add(pockets[i]); //add here to re enable later
            }
        }
    }


    /// <summary>
    /// push balls away from corks then .destroy temporary corks and re enable pockets
    /// </summary>
    private IEnumerator ReleasePockets()
    {
        pocketsCovered = false;

        foreach (GameObject pocket in _tempCoveredPockets) //Push away nearby balls so they dont score
        {
            Collider[] nearbyBalls = Physics.OverlapSphere(pocket.transform.position, _pocketPushRadius);
            foreach (Collider collider in nearbyBalls)
            {
                if (collider.TryGetComponent(out Rigidbody rb))
                {
                    Debug.Log("BANG");
                    Vector3 targetDir = _tableCenter.transform.position - rb.transform.position;
                    rb.AddForce(targetDir.normalized * _pocketPushForce, ForceMode.Impulse);
                }
            }
        }

        yield return new WaitForSeconds(.5f); //let balls get moved before destroying corks

        foreach (GameObject pocket in _tempCoveredPockets) //Remove temporary corks
        {
            if (pocket.transform.GetChild(0).gameObject != null)
            {
                Destroy(pocket.transform.GetChild(0).gameObject); //Destroy cork
            }
            else
            {
                Debug.Log("null");
            }
            pocket.GetComponent<Collider>().enabled = true;
        }


        _tempCoveredPockets.Clear();
        PocketExplosion?.Invoke(); //Moved balls so must tell ball manager to check movement
    }


    /// <summary>
    /// Put a cork and remove the selected pocket.
    /// Set input state back to gameplay
    /// </summary>
    private void RemovePocket()
    {
        _choosingPocket = false;
        _controlsUI.gameObject.SetActive(false); //UI
        _highlightObject.SetActive(false);

        StartCoroutine(PlaceCork(pockets[_pocketIdx], true));
        pockets[_pocketIdx].GetComponent<Collider>().enabled = false;
        pockets.Remove(pockets[_pocketIdx]);
    }


    /// <summary>
    /// Spawn a cork above the chosen pocket.
    /// Then move it onto the pocket and switch inputs back to gameplay.
    /// check is to check if there were temp corks that need to be unhidden.
    /// </summary>
    /// <param name="pocket"></param>
    /// <param name="check"></param>
    /// <returns></returns>
    IEnumerator PlaceCork(GameObject pocket, bool check)
    {
        GameObject cork = Instantiate(_cork);
        cork.transform.SetParent(pocket.transform); //Set parent to pocket so we can later remove cork if needed
        cork.SetActive(true);
        Vector3 targetPos = pocket.transform.position;
        cork.transform.position = targetPos + new Vector3(0, 5, 0); // set cork pos above target pocket - Should make 5 a variable for designer
        Vector3 startingPos = cork.transform.position;

        float timer = 0f;
        float maxTime = 3f;
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

        if (InputManager.Instance.currentActionMap.name != "Gameplay") //Player cant shoot until cork is down
        {
            InputManager.Instance.SwitchCurrentActionMap("Gameplay");
        }

        if (check)
        {
            ShowHiddenCorks();
        }
    }


    /// <summary>
    /// Look through hidden corks and reactivate them unless a permanent one has been placed on top
    /// </summary>
    private void ShowHiddenCorks()
    {
        foreach (GameObject obj in _tempCoveredPockets)
        {
            Debug.Log(obj.name);
            if (obj.transform.GetChild(0) != null)
            {
                obj.transform.GetChild(0).gameObject.SetActive(true); // corks were false
            }
            else
            {
                Debug.Log("NO children");
            }
        }
    }


    /// <summary>
    /// Hide all the temp corks
    /// </summary>
    private void HideTempCorks()
    {
        foreach (GameObject pocket in _tempCoveredPockets)
        {
            if (pocket.transform.GetChild(0) != null)
            {
                pocket.transform.GetChild(0).gameObject.SetActive(false); //hide all temp corks while player picks a pocket to block
            }
        }
    }


    /// <summary>
    /// Switch the input map and highlight the first pocket.
    /// Text is the description on screen for the player
    /// </summary>
    /// <param name="text"></param>
    private void StartChoosingPocket(string text)
    {
        InputManager.Instance.SwitchCurrentActionMap("PocketPick");
        _controlsUIDesc.text = text;
        _controlsUI.gameObject.SetActive(true);
        _choosingPocket = true;
        _pocketIdx = 0;
        _highlightObject.SetActive(true);
        HighlightPocket();
    }


    /// <summary>
    /// Wait for next frame and invoke PocketSelected
    /// </summary>
    /// <returns></returns>
    private IEnumerator CallPocketSelected()
    {
        yield return new WaitForEndOfFrame();
        Debug.Log("Selected");
        PocketSelected?.Invoke();
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
    /// turn is ending and going to switch to next player
    /// </summary>
    private void HandleTurnEnding()
    {
        //NOTE if both are active at end of turn - release pockets - then calls balls stopped which calls this and then start choosing pocket
        if (pocketsCovered) //release pockets at the end of the turn
        {
            StartCoroutine(ReleasePockets());
        }
        else if (_turnManager.ctsActive && !_turnManager.keepTurn) //choose only if goin to switch turn after
        {
            StartChoosingPocket("Choose only one pocket for your opponent to have available!");
        }
        else
        {
            //do nothing
        }
    }




    /// <summary>
    /// On turn switch show the pockets being covered if player covered them
    /// </summary>
    /// <param name="sabotaged"></param>
    private void HandleTurnSwitched(bool sabotaged)
    {
        if (_coverPockets)
        {
            _coverPockets = false;
            CoverPockets();
            pocketsCovered = true;
        }
    }


    /// <summary>
    /// If pocket block PU this calls ChoosePocket
    /// </summary>
    /// <param name="powerUp"></param>
    private void HandlePUs(SO_PowerUp powerUp)
    {
        if (powerUp.id == "Pocket Block")
        {
            _pocketBlockActive = true;
            HideTempCorks();
            StartChoosingPocket("Choose a pocket to block for the rest of the game!");
        }
        if (powerUp.id == "Call Their Shot")
        {
            _turnManager.ctsActive = true;
        }
    }
}
