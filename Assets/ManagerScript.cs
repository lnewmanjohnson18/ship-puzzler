using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ManagerScript : MonoBehaviour
{

    public int totalSolutionLength;

    // This is a simple gameStateGrid that can be conveniently referenced
    // It marks all walls as 1s, ships as 2s and empty spaces as 0s
    public int[,] gameStateGrid = new int[20, 20];   
    private System.Random randomGenerator = new System.Random();
    private GridSquare finishPoint;
    private int finishingShipID;
    // TODO: DO THIS BETTER
    private GameObject finishPointSprite;
    public Sprite finishPointTexture1;
    public Sprite finishPointTexture2;
    public Sprite finishPointTexture3;
    public Sprite finishPointTexture4;
    private Sprite[] finishPointTextureList = new Sprite[4];


    // COMMUNICATE THE NUMBER OF SHIPS TO THE SCRIPTS HERE
    private int numShips = 4;
    private Dictionary<int, GameObject> shipDict = new Dictionary<int, GameObject>();
    // NOTE: this tracks ships by NAME as it appears in the entity list i.e. ship1, ship2, and does not match their ship id
    //          shipID = shipName + 1 (currently)

    private GameObject grid;
    private Tilemap walls;
    private velocity_dragging shipScriptReference;
    // NOTE: THIS REFERS TO A RANDOM SCRIPT AND IS JUST FOR PULLING UTILITIES. DO NOT USE TO CONTROL INDIVIDUAL SHIPS


    // Called when the script instance is being loaded
    void Awake(){
        // TODO: FILL THIS IN AUTOMATICALLY
        this.finishPointTextureList[0] = this.finishPointTexture1;
        this.finishPointTextureList[1] = this.finishPointTexture2;
        this.finishPointTextureList[2] = this.finishPointTexture3;
        this.finishPointTextureList[3] = this.finishPointTexture4;
        // Name all of the ships
        for (int i = 1; i <= this.numShips; i++){

            // referencce the ship            
            GameObject ship_i = GameObject.Find(("Ship" + i.ToString()));

            // add the ship to the shipDict
            this.shipDict.Add(i, ship_i);

            // set the ID. the + 1 is because ID is globally defined for tracking the whole gamestate
            // in this context ID 0 = unoccupied and ID 1 = wall so unfortunately ship1 has an ID of 2 etc.
            ship_i.GetComponent<velocity_dragging>().setID(i+1);

            // just for the first ship set the manager's script reference 
            if (i == 1){
                this.shipScriptReference = ship_i.GetComponent<velocity_dragging>();
            }
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.Find("Walls");
        walls = grid.GetComponent<Tilemap>();

        // foreach (var position in walls.cellBounds.allPositionsWithin) {
        //     if (!walls.HasTile(position)) {
        //         continue;
        //     }
        //     TileBase wall = walls.GetTile(position);

        //     // Create a new sprite that will represent the path
        //     GameObject newPathSprite;
        //     newPathSprite = new GameObject();
        //     SpriteRenderer newSpriteRenderer = newPathSprite.AddComponent<SpriteRenderer>();
        //     // newSpriteRenderer.sprite = pathSpriteTexture;
        //     newPathSprite.transform.localPosition = position;

        //     // Add text to the top of the sprite to indicate how long the path is
        //     newPathSprite.AddComponent<Canvas>();

        //     GameObject myText = new GameObject();
        //     myText.transform.parent = newPathSprite.transform;
        //     myText.name = "pathTextSprite";

        //     TMPro.TextMeshPro text = myText.AddComponent<TMPro.TextMeshPro>();
        //     // text.font = (Font)Resources.Load("MyFont");
        //     text.text = position.ToString();
        //     text.alignment = TMPro.TextAlignmentOptions.Center;
        //     text.fontSize = 6;
        // }

        // walls.DeleteCells(new Vector3Int(0, 0, 0), new Vector3Int(1, 1, 1));

        resetGameGrid();

        // Once the gameStateGrid is in place pick the finishPoint
        this.finishingShipID = this.randomGenerator.Next(1,numShips);
        this.finishPoint = generateFinishPoint(this.finishingShipID);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space")){
            resetGameState();
        }
    }


    public int[,] getGamestateGrid(){
        return this.gameStateGrid;
    }

    // marks globally which ship is currently highlighted
    public void markSelected(int shipID){

        // deselect all other ships
        foreach (KeyValuePair<int, GameObject> kvp in this.shipDict){
            if (kvp.Key + 1 != shipID){
            // NOTE: the + 1 is because the ships are indexed starting at 2 and the names are indexed starting at 1
                kvp.Value.GetComponent<velocity_dragging>().deselect();
            }
        }

        // this.shipDict[shipID].GetComponent<velocity_dragging>().
    }

    public void markOccupied(int x, int y){
        // TODO 
    }

    public void markUnoccupied(int x, int y){
        // TODO
    }

    private GridSquare generateFinishPoint(int finishingShipID){
        while(true){
            int randX = this.randomGenerator.Next(-10,9);
            int randY = this.randomGenerator.Next(-10,9);


            // If the random position is a viable square
            // NOTE: currently a viable square is ANYTHING that isn't a wall so it may be under another ship
            if (this.gameStateGrid[randX + 10, randY + 10] != 1){
                GridSquare finishCandidate = new GridSquare(new Vector3(randX, randY, 0) , randX, randY, false);
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

        // // Add a canvas that sprite
        // finishPointSprite.AddComponent<Canvas>();

        // // Add text to canvas
        // GameObject myText = new GameObject();
        // myText.transform.parent = finishPointSprite.transform;
        // myText.name = "finish";

        // TMPro.TextMeshPro text = myText.AddComponent<TMPro.TextMeshPro>();
        // // text.font = (Font)Resources.Load("MyFont");
        // text.text = lenPath.ToString();
        // text.alignment = TMPro.TextAlignmentOptions.Center;

        // // Dynamically Determine Font Size and Text Box Attributes
        // // 1 digit numbers
        // if (Math.Floor(Math.Log10(lenPath) + 1) <= 1){
        //     text.fontSize = 8;

        //     // Text position
        //     RectTransform rectTransform = text.GetComponent<RectTransform>();
        //     rectTransform.localPosition = new Vector3(0, 0, 0);
        //     rectTransform.sizeDelta = new Vector2(0, 0.8f);
        
        // }
        // // 2 digit numbers
        // if (Math.Floor(Math.Log10(lenPath) + 1) == 2){
        //     text.fontSize = 7;
        //     // Text position
        //     RectTransform rectTransform = text.GetComponent<RectTransform>();
        //     rectTransform.localPosition = new Vector3(0, 0, 0);
        //     rectTransform.sizeDelta = new Vector2(2, 0.8f);
        
        // }
        // // 3 digit numbers
        // if (Math.Floor(Math.Log10(lenPath) + 1) >= 3){
        //     text.fontSize = 5;
        //     // Text position            
        //     RectTransform rectTransform = text.GetComponent<RectTransform>();
        //     rectTransform.localPosition = new Vector3(0, 0, 0);
        //     rectTransform.sizeDelta = new Vector2(2, 0.8f);
        // } 

        // // Add this sprite to the pathSpriteList so it can be managed later
        // this.pathSpriteList[lenPath] = newPathSprite;
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

    private void resetGameGrid(){

        
        // TODO: DONT DO THIS
        this.gameStateGrid = new int[20, 20];   
        // Iterate through the starting game state and add all walls to the grid
        for (int i = -10; i < 10; i++){
            for (int j = -10; j < 10; j++){
                // Debug.Log("Checking");
                // Debug.Log(new Vector3Int(i, j, 0));

                // Try to get the wall at (i, j). y = 0 always cause it's 2D
                TileBase wall = this.walls.GetTile(new Vector3Int(i, j, 0));
                if (wall != null){
                    // Debug.Log("Marking down");
                    this.gameStateGrid[i+10, j+10] = 1;

                    // Center of the cell transform
                    Vector3 worldPos = this.walls.GetCellCenterWorld(new Vector3Int(i, j, 0));
                }
            
            }

        }
    }
}
