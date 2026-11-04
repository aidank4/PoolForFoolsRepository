using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PocketManager : MonoBehaviour
{
    [System.Serializable]
    public struct PocketedBallData
    {
        public Collider pocket;
        public List<Collider> balls;
    }


    [Header("Reference")]
    [SerializeField] private GameObject _scoreboardRef;
    private Scoreboard _scoreboard;
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
    [SerializeField] private PocketedBallData[] _pocketedBalls;
    public List<GameObject> pockets = new List<GameObject>();
    private List<GameObject> _tempCoveredPockets = new List<GameObject>();
    private List<GameObject> _availablePockets = new List<GameObject>();


    [Header("EndPocket")]
    [SerializeField] private GameObject _endPocket;
    [SerializeField] private GameObject _minXBounds;
    [SerializeField] private GameObject _maxXBounds;
    [SerializeField] private GameObject _minZBounds;
    [SerializeField] private GameObject _maxZBounds;
    [SerializeField] private float _yHideValue = -1;
    [SerializeField] private float _yShowValue = 1;
    [SerializeField] private float _epAnimTime = 3f;
    private Vector3 _epHidePos;
    private Vector3 _epShowPos;
    private bool _epRaised = false; //If true on turn switch lower ep



    [Header("Misc Variables")]
    [SerializeField] private float _pocketPushRadius = 3f;
    [SerializeField] private float _pocketPushForce = 2f;
    private bool _choosingPocket = false; //When true the player can cycle through pocket selections
    private bool _endPocketActive = false; //For if the last pocket is active
    private int _pocketIdx;
    private GameObject _selectedPocket;

    [Header("PowerUps")]
    private bool _pocketBlockActive = false; //For differentiating what to do when a pocket is chosen
    private bool _reverseCardActive = false; //For differentiating what to do when a pocket is chosen
    private bool _callingShot = false; //For differentiating what to do when a pocket is chosen
    public bool pocketsCovered { get; private set; } = false; //used to know when to release covered pockets at end of turn
    private bool _coverPockets = false; //Used to know when to cover pockets on turn switch


    //Events
    public event Action Scratched;
    public event Action PocketSelected;
    public event Action PocketExplosion;
    public event Action<GameObject> BallRespawned;


    private void Awake()
    {
        //ref
        #region
        _scoreboard = _scoreboardRef.GetComponent<Scoreboard>();
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
        #endregion

        //Initialize struct Lists
        _pocketedBalls = new PocketedBallData[6];
        for (int i = 0; i <  _pocketedBalls.Length; i++)
        {
            _pocketedBalls[i].pocket = pockets[i].GetComponent<Collider>();
            _pocketedBalls[i].balls = new List<Collider>();
        }

        //Set End Pocket
        SetEndPocketPos();
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


    //SCORE
    #region
    /// <summary>
    /// Increment stripes score +1 or -1
    /// </summary>
    /// <param name="add"></param>
    private void ChangeStripeScore(bool add)
    {
        if (_turnManager.playerOne == _turnManager.teamStripes)
        {
            if (add)
            {
                _scoreboard.pOneScore++;
            }
            else
            {
                _scoreboard.pOneScore--;
            }
        }
        else
        {
            if (add)
            {
                _scoreboard.pTwoScore++;
            }
            else
            {
                _scoreboard.pTwoScore--;
            }
        }
    }


    /// <summary>
    /// Increments solid score +1 or -1
    /// </summary>
    /// <param name="add"></param>
    private void ChangeSolidScore(bool add)
    {
        if (_turnManager.playerOne == _turnManager.teamSolids)
        {
            if (add)
            {
                _scoreboard.pOneScore++;
            }
            else
            {
                _scoreboard.pOneScore--;
            }
        }
        else
        {
            if (add)
            {
                _scoreboard.pTwoScore++;
            }
            else
            {
                _scoreboard.pTwoScore--;
            }
        }
    }
    #endregion


    /// <summary>
    /// Handles pockets collisions with different types of balls.
    /// </summary>
    /// <param name="child"></param>
    /// <param name="ball"></param>
    /// 
    public void OnChildCollision(Collider child, Collider ball) //Called by pocket script on collision
    {
        switch (ball.gameObject.tag)
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
                        StartCoroutine(_fireworkScript.PlayFireworks(child.gameObject.transform.position, false));
                    }
                    else //Play sabotage fireworks if player scored wrong ball
                    {
                        StartCoroutine(_fireworkScript.PlayFireworks(child.gameObject.transform.position, true));
                    }

                    _turnManager.TurnDeciderStripes();
                    //Destroy(ball.gameObject); //ball
                    ChangeStripeScore(true);

                    for (int i = 0; i < _pocketedBalls.Length; i++)
                    {
                        if (_pocketedBalls[i].pocket == child)
                        {
                            _pocketedBalls[i].balls.Add(ball); //Keep ref of ball
                            ball.gameObject.SetActive(false);
                        }
                    }


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
                        StartCoroutine(_fireworkScript.PlayFireworks(child.gameObject.transform.position, false));
                    }
                    else //Play sabotage fireworks if player scored wrong ball
                    {
                        StartCoroutine(_fireworkScript.PlayFireworks(child.gameObject.transform.position, true));
                    }

                    _turnManager.TurnDeciderSolids();
                    //Destroy(ball.gameObject); //ball
                    for (int i = 0; i < _pocketedBalls.Length; i++)
                    {
                        if (_pocketedBalls[i].pocket == child)
                        {
                            _pocketedBalls[i].balls.Add(ball); //Keep ref of ball
                            ball.gameObject.SetActive(false);
                        }
                    }

                    ChangeSolidScore(true);
                    break;
                }
            case "EightBall":
                {
                    Debug.Log("EightBall");
                    ////TEMP player has scored all the balls and 8 ball
                    //if(_scoreboard.pOneScore == 7 && _turnManager.currentPlayer == _turnManager.playerOne)
                    //{
                    //    //Debug.Log("Player 1 Wins!");
                    //    _winScreenPOne.gameObject.SetActive(true);
                    //}
                    //else if(_scoreboard.pTwoScore == 7 && _turnManager.currentPlayer == _turnManager.playerTwo)
                    //{
                    //    //Debug.Log("Player 2 Wins!");
                    //    _winScreenPTwo.gameObject.SetActive(true);
                    //}
                    //else //TEMP player scored 8 ball early
                    //{
                    //    //Debug.Log("Player " + _turnManagerScript.currentPlayer.ToString() + " loses");
                    //    if(_turnManager.currentPlayer == _turnManager.playerOne)
                    //    {
                    //        _winScreenPTwo.gameObject.SetActive(true);
                    //    }
                    //    else if(_turnManager.currentPlayer == _turnManager.playerTwo)
                    //    {
                    //        _winScreenPOne.gameObject.SetActive(true);
                    //    }
                    //}
                    Destroy(ball.gameObject); //Ball
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


    //POCKET SELECTION CORE
    #region

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

                if (_pocketIdx == _availablePockets.Count - 1) //Restart if at the end
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
                    _pocketIdx = _availablePockets.Count - 1;
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
                _selectedPocket = EventSystem.current.currentSelectedGameObject; //Ref selected pocket
                _availablePockets.Clear(); //reset avialable pockets


                if (_reverseCardActive) //Reverse card
                {
                    _controlsUI.gameObject.SetActive(false); //UI
                    _highlightObject.SetActive(false);
                    _choosingPocket = false;


                    GameObject respawnedBall = RetrieveBall();
                    if (respawnedBall == null) //No balls to respawn must redo
                    {
                        StartChoosingPocket("No balls were scored in that pocket. Try Again.");
                        return; //Exit early
                    }
                    else if (respawnedBall.tag == "StripedBall") //Remove score for player
                    {
                        ChangeStripeScore(false);
                    }
                    else
                    {
                        ChangeSolidScore(false);
                    }
                    StartCoroutine(RespawnBall(respawnedBall));
                    InputManager.Instance.SwitchCurrentActionMap("Gameplay");
                    _reverseCardActive = false;
                }


                if (_pocketBlockActive) //Pocket Block - This must be above CTS because the else is necessary to come from this
                {
                    RemovePocket();
                    _pocketBlockActive = false;
                }

                if (_callingShot) //Call their shot - need else so it doesn't get called same time as pocket block - make sure others inactive
                {
                    _controlsUI.gameObject.SetActive(false); //UI
                    _highlightObject.SetActive(false);

                    _choosingPocket = false;
                    _coverPockets = true;
                    _turnManager.ctsActive = false;
                    _callingShot = false;
                    StartCoroutine(CallPocketSelected()); //Must wait till end of frame or else this input gets registered in the UI that appears after the event trigger
                }
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
        AdjustAvailablePockets();
        HighlightPocket();
    }


    /// <summary>
    /// Set pocket as selected and move the highlight object position to the pocket
    /// </summary>
    private void HighlightPocket()
    {
        EventSystem.current.SetSelectedGameObject(_availablePockets[_pocketIdx]);
        _highlightObject.transform.position = _availablePockets[_pocketIdx].transform.position;
    }

    #endregion




    //POCKET SELECTION HELPER FUNCTIONS

    //GENERAL
    #region

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
        while (timer < _epAnimTime)
        {
            float percent = timer / _epAnimTime;
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
    /// Set which pockets can be looked through
    /// </summary>
    private void AdjustAvailablePockets()
    {
        if (_reverseCardActive) //Only unblocked pockets with balls are available
        {
            for (int i = 0; i < _pocketedBalls.Length; i++)
            {
                PocketedBallData data = _pocketedBalls[i];
                if (data.balls.Count > 0)
                {
                    _availablePockets.Add(data.pocket.gameObject); //Add pocket with balls
                    bool stay = false;
                    for (int j = 0; j < pockets.Count; j++) //Check for blocked pockets
                    {
                        if (pockets[j].name == data.pocket.name)
                        {
                            stay = true;
                            break;
                        }
                    }
                    if (!stay) //Remove if the pocket is blocked
                    {
                        _availablePockets.Remove(data.pocket.gameObject);
                    }
                }
            }
        }
        else //Standard pocket check
        {
            foreach (GameObject pocket in pockets) //All unblocked pockets available
            {
                _availablePockets.Add(pocket);
            }
        }
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

        StartCoroutine(PlaceCork(_selectedPocket, true));
        _selectedPocket.GetComponent<Collider>().enabled = false;
        pockets.Remove(_selectedPocket);
    }


    /// <summary>
    /// Look through hidden corks and reactivate them unless a permanent one has been placed on top
    /// </summary>
    private void ShowHiddenCorks()
    {
        foreach (GameObject obj in _tempCoveredPockets)
        {
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

    #endregion




    //CALL THEIR SHOT
    #region

    /// <summary>
    /// Cover every pocket except the pocket at pocketIdx
    /// </summary>
    private void CoverPockets(bool coverAll)
    {
        InputManager.Instance.SwitchCurrentActionMap("PocketPick");
        pocketsCovered = true;
        for (int i = 0; i < pockets.Count; i++)
        {
            if (coverAll) //Hide every pocket
            {
                StartCoroutine(PlaceCork(pockets[i], false));
                pockets[i].gameObject.GetComponent<Collider>().enabled = false;
                _tempCoveredPockets.Add(pockets[i]); //add here to re enable later
            }
            else //Hide all pockets except selected
            {
                if (pockets[i] != _selectedPocket) //skip selected pocket
                {
                    StartCoroutine(PlaceCork(pockets[i], false));
                    pockets[i].gameObject.GetComponent<Collider>().enabled = false;
                    _tempCoveredPockets.Add(pockets[i]); //add here to re enable later
                }
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
    /// Wait for next frame and invoke PocketSelected
    /// </summary>
    /// <returns></returns>
    private IEnumerator CallPocketSelected()
    {
        yield return new WaitForEndOfFrame();
        PocketSelected?.Invoke();
    }

    #endregion




    //REVERSE
    #region

    /// <summary>
    /// Returns a bool for if a player can use the reverse sabotage
    /// </summary>
    /// <returns></returns>
    public bool CanReverse()
    {
        bool reversable = false;
        for (int i = 0; i < _pocketedBalls.Length; i++)
        {
            PocketedBallData data = _pocketedBalls[i];
            if (data.balls.Count > 0) //Pocket has been scored into
            {
                for (int j = 0; j < pockets.Count; j++) //Look in avaiable pockets
                {
                    if (pockets[j].name == data.pocket.name) //if theres a match can reverse
                    {
                        reversable = true;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            else
            {
                continue;
            }
        }

        return reversable;
    }


    /// <summary>
    /// Return most recent ball scored in selected pocket
    /// </summary>
    /// <returns></returns>
    private GameObject RetrieveBall()
    {
        for (int i = 0; i < _pocketedBalls.Length; i++)
        {
            if (_pocketedBalls[i].pocket.gameObject == _selectedPocket) //Look at balls inside selected pocket
            {
                if (_pocketedBalls[i].balls.Count == 0) //Picked a pocket with no balls
                {
                    return null;
                }
                Collider targetBall;
                targetBall = _pocketedBalls[i].balls[_pocketedBalls[i].balls.Count - 1]; // last ball (most recent)
                _pocketedBalls[i].balls.Remove(targetBall); //Remove ball from scored list
                return targetBall.gameObject;
            }
        }
        Debug.Log("ERROR retrieved null ball");
        return null;
    }


    /// <summary>
    /// Respawn ball and push it towards table center.
    /// After a short wait reenable pocket and call event
    /// </summary>
    /// <param name="ball"></param>
    /// <returns></returns>
    private IEnumerator RespawnBall(GameObject ball)
    {
        _selectedPocket.SetActive(false); //disable pocket so ball doesn't insta get eaten
        Vector3 upDir = new Vector3(0, 3, 0);
        ball.transform.position = _selectedPocket.transform.position + upDir; //Respawn it above pocket
        ball.SetActive(true);

        Vector3 targetPos = _tableCenter.transform.position - ball.transform.position;
        ball.GetComponent<Rigidbody>().AddForce((targetPos.normalized + upDir) * (_pocketPushForce), ForceMode.Impulse); //Push it towards the table

        yield return new WaitForSeconds(0.2f);
        _selectedPocket.SetActive(true); //reenable pocket
        ShowHiddenCorks(); //if theres hidden corks show them
        BallRespawned?.Invoke(ball);
    }

    #endregion





    //END POCKET
    #region

    /// <summary>
    /// Set end pocket to random position on the table and hide it.
    /// </summary>
    private void SetEndPocketPos()
    {
        //Get random location on the table
        float xPos = UnityEngine.Random.Range(_minXBounds.transform.position.x, _maxXBounds.transform.position.x);
        float zPos = UnityEngine.Random.Range(_minZBounds.transform.position.x, _maxZBounds.transform.position.x);
        
        //Set positions the pocket will move between
        _epHidePos = new Vector3 (xPos, _yHideValue ,zPos);
        _epShowPos = new Vector3(xPos, _yShowValue, zPos);

        //set pocket position and hide it
        _endPocket.transform.position = _epHidePos;
        _endPocket.SetActive(false);
    }


    /// <summary>
    /// Raise End Pocket out of the floor and turn on UI/FX
    /// </summary>
    /// <returns></returns>
    private IEnumerator RaiseEndPocket()
    {
        _endPocket.SetActive(true);
        
        //SFX

        float timer = 0;
        while (timer < _epAnimTime)
        {
            float percent = timer / _epAnimTime;
            _endPocket.transform.position = Vector3.Lerp(_epHidePos, _epShowPos, percent); //Move towards end pos
            timer += Time.deltaTime;
            yield return null;
        }
        _endPocket.transform.position = _epShowPos; //Set it as the position will be almost there

        //VFX UI ON
        _epRaised = true;
    }


    /// <summary>
    /// Lower end pocket into the floor and turn off UI/FX
    /// </summary>
    /// <returns></returns>
    private IEnumerator LowerEndPocket()
    {
        //Switch Action Map
        //SFX - VFX - UI Off

        float timer = 0;
        while (timer < _epAnimTime)
        {
            float percent = timer / _epAnimTime;
            _endPocket.transform.position = Vector3.Lerp(_epShowPos, _epHidePos, percent); //Move towards end pos
            timer += Time.deltaTime;
            yield return null;
        }
        _endPocket.transform.position = _epHidePos;

        _endPocket.SetActive(false);
        _epRaised = false;
    }


    #endregion




    //HANDLE EVENTS
    #region

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
            _callingShot = true;
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
        //Check if this turn needs the End Pocket
        if (_scoreboard.pOneScore == 7 && _turnManager.currentPlayer == _turnManager.playerOne)
        {
            _endPocketActive = true;
        }
        else if (_scoreboard.pTwoScore == 7 && _turnManager.currentPlayer == _turnManager.playerTwo)
        {
            _endPocketActive = true;
        }
        else
        {
            _endPocketActive = false;
        }





       
        if (_endPocketActive && _epRaised) //EndPocket time active and already raised
        {
            Debug.Log("Do Nothing");
            //Do nothing
        }
        else if (_endPocketActive && !_epRaised) //EndPocket needs to be raise
        {
            Debug.Log("Raise");
            StartCoroutine(RaiseEndPocket());
            CoverPockets(true);
        }
        else if (!_endPocketActive && _epRaised) //End Pocket needs to be lowered
        {
            Debug.Log("Lower");
            StartCoroutine(LowerEndPocket());
        }
        else //NORMAL TURN
        {
            if (_coverPockets) //Check for call their shot asking to cover pockets
            {
                _coverPockets = false;
                CoverPockets(false);
            }
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
        if (powerUp.id == "Reverse Card")
        {
            _reverseCardActive = true;
            HideTempCorks();
            StartChoosingPocket("Choose a pocket to bring back its most recent scored ball");
        }
    }

    #endregion

}
