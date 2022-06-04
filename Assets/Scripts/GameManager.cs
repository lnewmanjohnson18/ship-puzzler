using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using Mirror;

public class GameManager : MonoBehaviour
{
    private int[,] gamestateGrid = new int[20, 20];
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
    private System.Random randomGenerator = new System.Random();
    private Vector3 finishPoint;
    private int finishingShipID;

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
    private GameObject nextRoundButton;
    private PlayerManager PlayerManager;

    // private GameObject gridObject;
    // private Tilemap walls;
    // private int[,] gameStateGrid = new int[20, 20];

    // COMMUNICATE THE NUMBER OF SHIPS TO THE SCRIPTS HERE
    private int numShips = 4;
    public int selectedShipID = 1;
    private Dictionary<int, GameObject> shipDict = new Dictionary<int, GameObject>();
    // NOTE: this tracks ships by NAME as it appears in the entity list i.e. ship1, ship2, and does not match their ship id
    //          shipID = shipName + 1 (currently)
    // NOTE NOTE: BEING CHANGED right now

    // full list of moves. used to validate solution after finish and undo moves
    private Tuple<int, Vector3Int>[] listOfMoves = new Tuple<int, Vector3Int>[] {};


    private int totalSolutionLength;
    private int totalSolutionNumMoves;

    // === DECLARE AS SINGLETON ===
    private static GameManager _instance;

    public static GameManager Instance {get {return _instance; } }


    // player calls this when it is loaded in to tell the gameManager to get going
    public void wakeManager(PlayerManager PlayerManager){
        // save a reference to the play manager
        this.PlayerManager = PlayerManager;

        // Name all of the ships
        for (int i = 1; i <= this.numShips; i++){

            // reference the ship
            GameObject ship_i = GameObject.Find(("Ship" + i.ToString() + "(Clone)"));

            // add the ship to the shipDict
            this.shipDict.Add(i, ship_i);

            // set the ID. Ship1 = int 1, Ship2 = int 2, etc..
            ship_i.GetComponent<ShipManager>().setID(i);
        }

        setupNewGame();
    }

    void Awake()
    {

        // set gameStateGrid to its starting configuration
        // gamestateGrid = roundStartGamestateGrid;

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
        this.nextRoundButton = GameObject.Find("NextRoundButton");
        this.nextRoundButton.SetActive(false);


    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {        
        if (Input.GetKeyDown(KeyCode.X))
        {

            for (int i = 0; i < 20; i++){
                string lineString = "";
                for (int j = 0; j < 20; j++){
                    lineString += gamestateGrid[i, j].ToString();
                }
                Debug.Log(lineString);
            }
        }
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
        resetGameGrid();
    }

    public void initializeShips()
    {   
        GameManager.Instance.Ship1.GetComponent<ShipManager>().initializeShip(gamestateGrid);
        GameManager.Instance.Ship2.GetComponent<ShipManager>().initializeShip(gamestateGrid);
        GameManager.Instance.Ship3.GetComponent<ShipManager>().initializeShip(gamestateGrid);
        GameManager.Instance.Ship4.GetComponent<ShipManager>().initializeShip(gamestateGrid);
    }

    public void SpawnTest()
    {
        Instantiate(Test);

    }

    // marks globally which ship is currently highlighted
    public void markSelected(int shipID){

        // mark a note on the manager who is currently selected
        selectedShipID = shipID;

        // tell all other ships they are no longer selected
        foreach (KeyValuePair<int, GameObject> kvp in this.shipDict){
            if (kvp.Key != shipID){
                kvp.Value.GetComponent<ShipManager>().deselect();
            }
        }

        // this.shipDict[shipID].GetComponent<velocity_dragging>().
    }


    // resets the game grid to the way it was at the start of the round
    private void resetGameGrid(){

        // TODO: DONT DO THIS
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
            Debug.Log("gamestateGrid");
            Debug.Log(gamestateGrid);
            Debug.Log("kvp.Value");
            Debug.Log(kvp.Value);
            Debug.Log("kvp.Value.GetComponent<ShipManager>().shipID");
            Debug.Log(kvp.Value.GetComponent<ShipManager>().shipID);
            Debug.Log("point.x");
            Debug.Log(point.x);

            gamestateGrid[point.x + 10, point.y + 10] = kvp.Value.GetComponent<ShipManager>().shipID;
        }
    }

    private Vector3 generateFinishPoint(int finishingShipID){
        while(true){
            int randX = this.randomGenerator.Next(-10,9);
            int randY = this.randomGenerator.Next(-10,9);


            // If the random position is a viable square
            // NOTE: currently a viable square is ANYTHING that isn't a wall so it may be under another ship
            if (this.gamestateGrid[randX + 10, randY + 10] != -1){
                Vector3 finishCandidate = new Vector3(randX, randY, 0);
                createFinishPointSprite(randX, randY, finishingShipID);
                return finishCandidate;
            }
        }
    }

    public void createFinishPointSprite(int x, int y, int finishingShipID){

        // Create a new sprite that will represent the path
        this.finishPointSprite = new GameObject();
        finishPointSprite.name = "Finish Point Sprite";
        SpriteRenderer newSpriteRenderer = finishPointSprite.AddComponent<SpriteRenderer>();
        newSpriteRenderer.sprite = this.finishPointTextureList[(finishingShipID - 1)];
        // NOTE: MINUS 1 is because ship 1 is @ index 0 etc. etc.
        finishPointSprite.transform.localPosition = new Vector3(x+.5f, y+.5f, 0);
        // make it so it appears on top of any other sprite
        newSpriteRenderer.sortingOrder = numShips*20 + 10;

    }

    public void checkSolution(int shipID, Vector3 endSquare){
        // if it is the right ship and has come to a stop at the right location
        if ((shipID == this.finishingShipID) && (this.finishPoint == endSquare)){


            // TODO FILL THIS IN
            PlayerManager.postSolution();

            // activate button
            this.nextRoundButton.SetActive(true);
        }
    }

    public void nextRoundButtonCall(){
        this.nextRoundButton.SetActive(false);

        startNewRound();

    }

    private void startNewRound(){
        // lock board and prompt user for next puzzle
        // TODO
        
        // reset board
        resetGameState();

        // reset all the ships paths
        foreach (KeyValuePair<int, GameObject> kvp in this.shipDict){
            kvp.Value.GetComponent<ShipManager>().setRoundStartPosition();
        }
        // maybe move the ships to new random locations?
    }

    private void resetGameState(){

        // Remove old finish point
        // TODO: FIX THIS
        Destroy(this.finishPointSprite);

        // Pick a new ship and finish spot
        this.finishingShipID = this.randomGenerator.Next(1,numShips);
        this.finishPoint = generateFinishPoint(this.finishingShipID);

        // Reset the game grid
        resetGameGrid();
    }
    
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

    // TODO make so we only use either the Vector3 or x,y. (preferably the Vector3)
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