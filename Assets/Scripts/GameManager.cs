using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using Mirror;

public class GameManager : MonoBehaviour
{
    // track whether or not the whole game has been loaded in and whether or not we should run things in update{}
    public bool isAwake = false;
    // state to determine if the board objects and UI buttons should be interactable
    public bool isWaiting = false;

    public int[,] gamestateGrid = new int[20, 20];
    // private int[,] roundStartGamestateGrid = new int[20, 20] {
    //     {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1},
    //     {-1,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1},
    //     { 0, -1,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0, -1, -1},
    //     {-1,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0, -1, -1, -1,  0,  0,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0,  0,  0, -1, -1, -1,  0,  0,  0,  0, -1, -1,  0,  0,  0, -1},
    //     {-1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0, -1},
    //     {-1,  0,  0,  0, -1, -1,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0, -1, -1},
    //     {-1,  0,  0,  0, -1,  0,  0,  0,  0, -1, -1, -1,  0,  0,  0, -1,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0, -1, -1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0, -1, -1,  0,  0,  0,  0, -1},
    //     {-1, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0,  0, -1, -1},
    //     {-1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1},
    //     {-1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1},
    //     {-1,  0,  0,  0, -1,  0,  0,  0,  0,  0,  0,  0,  0,  0,  0, -1,  0,  0,  0, -1},
    //     {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}
    // };

    private GameObject finishPointSprite;
    public Sprite finishPointTexture1;
    public Sprite finishPointTexture2;
    public Sprite finishPointTexture3;
    public Sprite finishPointTexture4;
    private Sprite[] finishPointTextureList = new Sprite[4];
    public System.Random randomGenerator = new System.Random();
    private Vector3Int finishPoint;
    private int finishingShipID;
    private Dictionary<uint, Tuple<int, int>> roundEndResultsDict = new Dictionary<uint, Tuple<int, int>>();

    private int totalSolutionNumMoves;
    private int totalSolutionLength;
    // private Tuple<List<int, Vector3Int, Vector3Int>, int, int, int[,]> currentSolution;

    public GameObject Ship1Prefab;
    public GameObject Ship2Prefab;
    public GameObject Ship3Prefab;
    public GameObject Ship4Prefab;
    private GameObject Ship1;
    private GameObject Ship2;
    private GameObject Ship3;
    private GameObject Ship4;
    public GameObject Test;
    private GameObject GridObject;
    private Tilemap Walls;
    private GameObject UI;
    private GameObject postSolutionButton;
    private PlayerManager PlayerManager;

    // private GameObject gridObject;
    // private Tilemap walls;
    // private int[,] gameStateGrid = new int[20, 20];

    // COMMUNICATE THE NUMBER OF SHIPS TO THE SCRIPTS HERE
    public int numShips = 4;
    public int selectedShipID = 1;
    private Dictionary<int, GameObject> shipDict = new Dictionary<int, GameObject>();

    // full list of moves. used to validate solution after finish and undo moves
    // Tuple should be of the form <shipID, startLocation, endLocation>
    private List<Tuple<int, Vector3Int, Vector3Int>> movesList = new List<Tuple<int, Vector3Int, Vector3Int>>();

    // SERVER VARIABLES
    // TODO: FIX- this puts a cap of 10 players on the game 
    private int[] finalNumMovesScores = new int[10];
    private int[] finalLengthScores = new int[10];
    public int currBestScore = (int)10e4;
    public int[] currWinningGamestate_flat;

    // === DECLARE AS SINGLETON ===
    private static GameManager _instance;

    public static GameManager Instance {get {return _instance; } }

    void Awake()
    {

        if (_instance != null && _instance != this){
            Destroy(this.gameObject);
            Debug.Log("WARNING: redundant GameManager object was destroyed");
        }
        else{
            _instance = this;
        }   

        // TODO: FILL THIS IN AUTOMATICALLY
        this.finishPointTextureList[0] = this.finishPointTexture1;
        this.finishPointTextureList[1] = this.finishPointTexture2;
        this.finishPointTextureList[2] = this.finishPointTexture3;
        this.finishPointTextureList[3] = this.finishPointTexture4;

        // Get references to objects in scene
        this.GridObject = GameObject.Find("Grid");
        this.Walls = GameObject.Find("Walls").GetComponent<Tilemap>();
        this.UI = GameObject.Find("PersistentUI");
        this.postSolutionButton = GameObject.Find("PostSolutionButton");
        this.postSolutionButton.SetActive(false);


    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {      
        if (isAwake)
        {

            if (shipDict[finishingShipID].GetComponent<ShipManager>().currPermPosition == finishPoint)
            {
                postSolutionButton.SetActive(true);
            }
            else
            {
                postSolutionButton.SetActive(false);

            }

        }

        // debugging commands here
        if (Input.GetKeyDown(KeyCode.X))
        {
            // disableBoard(!isWaiting);
            // int[,] fakeGrid = new int[20,20];
            // PlayerManager.postSolution(new List<Tuple<int, Vector3Int, Vector3Int>>(), 10, 300, new int[20,20]);
            // this.activateCountdown(10, 100);
            // this.postSolutionButton.SetActive(true);
            // PlayerManager.postSolution(movesList, totalSolutionNumMoves, totalSolutionLength, gamestateGrid);
            // for (int i = 0; i < 20; i++){
            //     string lineString = "";
            //     for (int j = 0; j < 20; j++){
            //         lineString += gamestateGrid[i, j].ToString();
            //     }
            //     Debug.Log(lineString);
            // }
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            resetMostRecentMove();

        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            resetToRoundStart();
        }

    }

    // player calls this when it is loaded in to tell the gameManager to get going
    public void wakeManager(PlayerManager PlayerManager){
        // set awake = true
        isAwake = true;

        // save a reference to the play manager
        this.PlayerManager = PlayerManager;

        // Name all of the ships
        for (int i = 1; i <= this.numShips; i++){

            // reference the ship
            GameObject ship_i = GameObject.Find(("Ship" + i.ToString() + "(Clone)"));

            // add the ship to the shipDict
            this.shipDict.Add(i, ship_i);
        }

        setupNewGame();
    }


    private void setupNewGame()
    {
        this.totalSolutionLength = 0;
        this.totalSolutionNumMoves = 0;

        // this sets up the game grid to the start of the game
        resetGameGrid();

        // choose which ship is now the key ship and where its finish point is
        this.finishingShipID = this.randomGenerator.Next(1,numShips);
        this.finishPoint = generateFinishPoint(this.finishingShipID);
    }

    public void resetToRoundStart()
    {
        // reset game grid
        resetGameGrid();

        // tell all the ships to reset themselves
        foreach (KeyValuePair<int, GameObject> kvp in this.shipDict)
        {
            kvp.Value.GetComponent<ShipManager>().resetShipToRoundStart();
        }

        // reset stats
        totalSolutionNumMoves = 0;
        totalSolutionLength = 0;

        // update stats text
        updateStatsText();
    }

    public void spawnShips()
    {
        Instantiate(Ship1Prefab);
        Ship1 = GameObject.Find("Ship1(Clone)");
        Instantiate(Ship2Prefab);
        Ship2 = GameObject.Find("Ship2(Clone)");
        Instantiate(Ship3Prefab);
        Ship3 = GameObject.Find("Ship3(Clone)");
        Instantiate(Ship4Prefab);
        Ship4 = GameObject.Find("Ship4(Clone)");
    }

    public void initializeGameGrid()
    {
        // call this function to initialize the game grid
        resetGameGrid();

    }

    public void initializeShips()
    {   
        GameManager.Instance.Ship1.GetComponent<ShipManager>().initializeShip(1);
        GameManager.Instance.Ship2.GetComponent<ShipManager>().initializeShip(2);
        GameManager.Instance.Ship3.GetComponent<ShipManager>().initializeShip(3);
        GameManager.Instance.Ship4.GetComponent<ShipManager>().initializeShip(4);
    }

    public void SpawnTest()
    {
        Instantiate(Test);

    }


    // resets the game grid to the way it was at the start of the round
    private void resetGameGrid(){

        this.gamestateGrid = new int[20, 20];   
        // Iterate through the starting game state and add all walls to the grid
        for (int i = -10; i < 10; i++){
            for (int j = -10; j < 10; j++){
                // Debug.Log("Checking");
                // Debug.Log(new Vector3Int(i, j, 0));

                // Try to get the wall at (i, j). y = 0 always cause it's 2D
                TileBase wall = this.Walls.GetTile(new Vector3Int(i, j, 0));
                if (wall != null){
                    // Debug.Log("Marking down");
                    this.gamestateGrid[i+10, j+10] = -1;

                    // Center of the cell transform
                    // Vector3 worldPos = this.walls.GetCellCenterWorld(new Vector3Int(i, j, 0));
                }
            }
        }
        // Iterate through ships and add all ships to the grid
        foreach (KeyValuePair<int, GameObject> kvp in this.shipDict){
            Vector3Int point = kvp.Value.GetComponent<ShipManager>().roundStartSquare;
            gamestateGrid[point.x + 10, point.y + 10] = kvp.Value.GetComponent<ShipManager>().shipID;
        }
    }

    public Vector3Int generateFinishPoint(int finishingShipID){
        while(true){
            int randX = this.randomGenerator.Next(-10,9);
            int randY = this.randomGenerator.Next(-10,9);


            // If the random position is a viable square
            // NOTE: currently a viable square is ANYTHING that isn't a wall so it may be under another ship
            if (this.gamestateGrid[randX + 10, randY + 10] != -1){
                Vector3Int finishCandidate = new Vector3Int(randX, randY, 0);
                createFinishPointSprite(randX, randY, finishingShipID);
                return finishCandidate;
            }
        }
    }

    public void createFinishPointSprite(int x, int y, int finishingShipID){

        // Create a new sprite that will represent the path
        this.finishPointSprite = new GameObject();
        finishPointSprite.name = "FinishPointSprite";
        SpriteRenderer newSpriteRenderer = finishPointSprite.AddComponent<SpriteRenderer>();
        newSpriteRenderer.sprite = this.finishPointTextureList[(finishingShipID - 1)];
        // NOTE: MINUS 1 is because ship 1 is @ index 0 etc. etc.
        finishPointSprite.transform.localPosition = new Vector3(x+.5f, y+.5f, 0);
        // make it so it appears on top of any other sprite
        newSpriteRenderer.sortingOrder = numShips*20 + 10;

    }

    public void postSolutionButtonCall()
    {
        if (!isWaiting)
        {
            PlayerManager.postSolution(movesList, totalSolutionNumMoves, totalSolutionLength, gamestateGrid);
        }
    }

    public void soloPlayNextRoundButton()
    {
        PlayerManager.finishCountdown();
    }

    public void startNextRound(int[,] roundStartGamestate, int newFinishingShipID, Vector3Int newFinishPoint)
    {
        // update gamestate
        gamestateGrid = roundStartGamestate;

        // tell the ships where their new round start square is
        for (int i = roundStartGamestate.GetLowerBound(0); i < roundStartGamestate.GetUpperBound(0); i++)
        {
            for (int j = roundStartGamestate.GetLowerBound(1); j < roundStartGamestate.GetUpperBound(1); j++)
            {
                if (shipDict.ContainsKey(roundStartGamestate[i, j]))
                {
                    shipDict[roundStartGamestate[i, j]].GetComponent<ShipManager>().roundStartSquare = new Vector3Int(i - 10, j - 10, 0);
                }
            }

        }

        // reset all variables that around round specific (also resets state variables)
        resetRoundVariables();

        // destroy the old objective
        Destroy(GameObject.Find("FinishPointSprite"));

        // save the new objective
        this.finishingShipID = newFinishingShipID;
        this.finishPoint = newFinishPoint;

        // update the stats text

        // disable end of round screen UI
        this.transform.GetChild(2).gameObject.SetActive(false);

        // enable the board
        disableBoard(false);

    }

    private void resetRoundVariables()
    {
        // reset all the temporary round variables on game manager
        isWaiting = false;
        isAwake = true;
        roundEndResultsDict.Clear();
        totalSolutionNumMoves = 0;
        totalSolutionLength = 0;
        updateStatsText();
        movesList.Clear();
        currBestScore = (int)10e4;

        // reset the text variables stored on the UI objects 
        this.transform.GetChild(2).transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "";
        this.transform.GetChild(2).transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = "";
        this.transform.GetChild(2).transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text = "";

        // reset all the temporary round variables on all ships and delete path sprites
        foreach (KeyValuePair<int, GameObject> kvp in shipDict)
        {
            kvp.Value.GetComponent<ShipManager>().resetRoundVariables();
        }

    }

    //DEP??
    // private void startNewRound(){
    //     // lock board and prompt user for next puzzle
    //     // TODO
        
    //     // reset board
    //     resetGameState();

    //     // reset all the ships paths
    //     foreach (KeyValuePair<int, GameObject> kvp in this.shipDict){
    //         kvp.Value.GetComponent<ShipManager>().setRoundStartPosition();
    //     }
    //     // maybe move the ships to new random locations?
    // }


    // DEP??
    // TODO: RENAME THIS TO LIKE "startNewRound" CAUSE THATS WHAT IT DOES RIGHT?
    // DO THIS AND THE ABOVE FUNCTION COVER DIFFERENT USE CASES?
    // private void resetGameState(){

    //     // Remove old finish point
    //     // TODO: FIX THIS
    //     Destroy(this.finishPointSprite);

    //     // Pick a new ship and finish spot
    //     this.finishingShipID = this.randomGenerator.Next(1,numShips);
    //     this.finishPoint = generateFinishPoint(this.finishingShipID);

    //     // Reset the game grid
    //     resetGameGrid();
    // }

    // resets the most recent move the player has input
    private void resetMostRecentMove()
    {

        // check if there is a move to reset
        if (movesList.Count > 0)
        {
            // peek at the move to be reset
            Tuple<int, Vector3Int, Vector3Int> mostRecentMove = movesList[movesList.Count - 1];

            // remove it from the list of moves
            movesList.RemoveAt(movesList.Count - 1);

            // move the ship piece back to the previous location
            shipDict[mostRecentMove.Item1].GetComponent<ShipManager>().resetLeg();

            // remove the length of the reset move from total length of path
            totalSolutionLength -= Math.Max(Math.Abs(mostRecentMove.Item2.x - mostRecentMove.Item3.x),  Math.Abs(mostRecentMove.Item2.y - mostRecentMove.Item3.y));
        
            // decrement total num moves
            totalSolutionNumMoves--;

            updateStatsText();
        }

    }

    // called when the server alerts that a solution has been posted and indicates to the player the countdown has started
    public void activateCountdown(int numMoves, int lenSolution)
    {
        // activates the UI elements for the countdown and updates their text
        this.transform.GetChild(1).gameObject.SetActive(true);
        this.transform.GetChild(1).transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "NumMoves: " + numMoves;
        this.transform.GetChild(1).transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text = "Length: " + lenSolution;

    }

    public void activateWaitScreen(bool isSolo)
    {
        // disable the board
        disableBoard(true);

        // disable the countdown UI elements? not sure.
        // this.transform.GetChild(1).gameObject.SetActive(true);

        // activate the wait screen
        this.transform.GetChild(0).gameObject.SetActive(true);

        // if (!isSolo){this.transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);}



    }

    public void activateRoundEndScreen(int numMoves, int lenPath, uint netId, bool isServer)
    {
        // disable the wait screen or the countdown UI if either are active
        this.transform.GetChild(0).gameObject.SetActive(false);
        this.transform.GetChild(1).gameObject.SetActive(false);
        //activate the roundEndScreen
        this.transform.GetChild(2).gameObject.SetActive(true);

        // disable game pieces between rounds
        disableBoard(true);

        // disable the new round button if client
        if (isServer){this.transform.GetChild(2).transform.GetChild(7).gameObject.SetActive(false);}
        // disable the "waiting for host" text if host
        else {this.transform.GetChild(2).transform.GetChild(6).gameObject.SetActive(false);}
        

        // safety check that this is the first post for this netId
        if (!roundEndResultsDict.ContainsKey(netId))
        {
            Debug.Log("adding score to text");
            // add the score to the resultsDict
            roundEndResultsDict.Add(netId, new Tuple<int, int>(numMoves, lenPath));
            this.transform.GetChild(2).transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text += "" + netId + "\n";
            this.transform.GetChild(2).transform.GetChild(2).GetComponent<TMPro.TextMeshProUGUI>().text += "" + numMoves + "\n";
            this.transform.GetChild(2).transform.GetChild(3).GetComponent<TMPro.TextMeshProUGUI>().text += "" + lenPath + "\n";

        }
        else
        {
            Debug.Log("++WARNING++: duplicate results were posted for netId" + netId);
        }

    }

    // asks the server to initiate new round
    public void requestNewRound()
    {
        PlayerManager.CmdRequestNewRound();
    }

    // function to be called every frame when the countdown timer is running
    public void updateTimerText(float timeRemaining)
    {
        this.transform.GetChild(1).transform.GetChild(0).GetComponent<TMPro.TextMeshProUGUI>().text = "Time Remaining:\n" + timeRemaining;

    }

    public void addMoveToList(int shipID, Vector3Int startLocation, Vector3Int endLocation)
    {

        // add move to the list of all moves
        this.movesList.Add(new Tuple<int, Vector3Int, Vector3Int>(shipID, startLocation, endLocation));

        // add the new move to the total length of path
        totalSolutionLength += Math.Max(Math.Abs(startLocation.x - endLocation.x),  Math.Abs(startLocation.y - endLocation.y));

        // increment total num moves
        totalSolutionNumMoves++;

        // update the user's stats display
        updateStatsText();
    }


    // update the text telling the player how long their current solution is
    // this should be called any time a leg is added or removed to movesList  
    public void updateStatsText()
    {
        foreach (Transform UIElement in this.UI.transform)
        {
            TMPro.TextMeshProUGUI textObj = UIElement.GetComponent<TMPro.TextMeshProUGUI>();
            if (UIElement.gameObject.name == "NumMovesText")
            {
                textObj.text = textObj.text.Split(":")[0] + ": " + totalSolutionNumMoves.ToString();                 
            }

            if (UIElement.gameObject.name == "SolutionLengthText")
            {
                textObj.text = textObj.text.Split(":")[0] + ": " + totalSolutionLength.ToString();                 

            }
        }
    }

    // turns off the ships and disables and buttons for when the game is in a wait state
    // NOTE: DISABLES so they are still visible just non-interactable
    public void disableBoard(bool disable)
    {
        // update state
        isWaiting = disable ? true : false;

        // disable / enable ships 
        foreach (KeyValuePair<int, GameObject> kvp in shipDict)
        {
            kvp.Value.GetComponent<ShipManager>().enabled = disable ? false : true;
        }
    }

    public void printGrid(int[,] grid)
    {
        Debug.Log("=====PRINTING GRID=====");
        for (int i = 0; i < grid.GetLength(0); i++)
        {
            string lineString = "";
            // prints this way so the grid "reads right"
            for (int j = (grid.GetLength(1) - 1); j > -1 ; j--)
            {
                lineString += grid[j, i].ToString() + " ";
            }
            Debug.Log(lineString);
        }
    }

    // ===============================================================================================================
    // SERVER METHODS
    // ===============================================================================================================

    // N/A rn
}

// ==================== SUPPLEMENTAL CLASSES ====================


// a way to refer to a grid element
// NOTE: BELOW CLASS HAS BEEN ENTIRELY REMOVED FROM CURRENT IMPLEMENTATION
public class GridSquare
{
    public Vector3 location;
    public int x;
    public int y;
    public bool isOccupied = false;

    public GridSquare(Vector3 location, int x, int y, bool isOccupied){
        this.location = new Vector3(x, y, 0);
        this.x = x;
        this.y = y;
        this.isOccupied = isOccupied;
    }

    // helper function to compare two gridsquares without worrying about tagged on variable
    public bool isEqualTo(GridSquare other){
        if ((this.x == other.x) && (this.y == other.y)){
            return true;
        }
        else{
            return false;
        }
    }

    public override string ToString(){
        return ("(" + x.ToString() + ", " + y.ToString() + ")");
    }

}

// A class to be used as a shorthand to refer the full gamestate of a single ship
// i.e. it contains all necessary information to recreate the game actions of that ship
public class PathState
{
    public Vector3Int location;
    public int lenPermPath;
    public int numMoves;

    public PathState(Vector3Int location, int lenPermPath, int numMoves){
        this.location = location;
        this.lenPermPath = lenPermPath;
        this.numMoves = numMoves;
    }
}

// ==============================================================