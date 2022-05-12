using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;


public class velocity_dragging : MonoBehaviour
{
    // Debugging Variables
    public float minVelocityX = 0;
    public float minVelocityY = 0;

    public int shipID;
    public int notSet;
    public Rigidbody2D rb;
    public GridSquare roundStartSquare;
    private bool isBeingHeld = false;
    private bool isSelected = false;
    public GridSquare[] path = new GridSquare[484];
    public GridSquare[] permPath = new GridSquare[484];
    private GameObject[] pathSpriteList = new GameObject[484];
    private Stack<PathState> pathHistory = new Stack<PathState>();


    public int lenPath = 0;
    public int lenPermPath = 0;
    public int xCurrCoord;
    public int yCurrCoord;
    public Sprite pathSpriteTexture;
    private SpriteRenderer sr;
    private GameObject grid;
    private Tilemap walls;
    private int[,] gameStateGrid;
    private Transform locationTransform;
    public GameObject gameManager;

    private Boolean qPressed = false;

    private string curr_bearing = "NONE";
    // This variable is a state describing whether the ship is currently considering a move along the X or Y-Axis
    // 'X' is the X-Axis 
    // 'Y' is the Y-Axis 
    // 'NONE' means the ship is not currently considering a move

    public GridSquare currPermPosition;
    // This variable tracks which position was the end of the last viable leg. "where it is" when asking the PermPath

    void Awake(){
        // Sprite Renderer to render path sprites later
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        // Turn off physics interaction until the ship is picked up (so it cant be jostled)
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        locationTransform = GetComponent<Transform>();

    
    }

    void Start(){

        // Set staring position and add the ship's starting GridSquare to the permanent and temporary path        
        this.xCurrCoord = (int)Math.Floor(this.gameObject.transform.position.x);
        this.yCurrCoord = (int)Math.Floor(this.gameObject.transform.position.y);
        this.roundStartSquare = new GridSquare(new Vector3(this.xCurrCoord, this.yCurrCoord, 0), this.xCurrCoord, this.yCurrCoord, true); 
        this.path[0] = this.roundStartSquare;
        this.permPath[0] = this.roundStartSquare;
        this.currPermPosition = this.roundStartSquare;


        // Bring in the objects that run the tilemap
        this.grid = GameObject.Find("Walls");
        this.walls = grid.GetComponent<Tilemap>();

        // Bring in objects from the manager
        this.gameManager = GameObject.Find("GameManager");
        this.gameStateGrid = this.gameManager.GetComponent<ManagerScript>().getGamestateGrid();
        if (this.shipID == 0){
            Debug.Log("ERROR: SHIP ATTEMPTED TO MARK LOCATION WITHOUT VALID METADATA (SHIPID)");
        }
        else{
            this.gameStateGrid[this.xCurrCoord + 10, this.yCurrCoord + 10] = this.shipID;
        }
    }

    void Update(){
        // ===============================================================================================================
        // Update ship attributes for the frame
        // ===============================================================================================================

        // Update location
        this.xCurrCoord = (int)Math.Floor(this.gameObject.transform.position.x);
        this.yCurrCoord = (int)Math.Floor(this.gameObject.transform.position.y);

        //Debugging Log
        if (rb.velocity.x < this.minVelocityX){
            this.minVelocityX = rb.velocity.x;
        }

                //Debugging Log
        if (rb.velocity.y < this.minVelocityY){
            this.minVelocityY = rb.velocity.y;
        }

        // ===============================================================================================================
        // See if the path has been reset by user
        // ===============================================================================================================

        if (Input.GetKeyDown("space")){
            resetShipPath();
        }


        // if q has been hit and is not being held 
        if (Input.GetKeyDown("q") && this.qPressed == false && this.isSelected == true){
            Debug.Log("reset button called on a frame");
            resetLeg();
            this.qPressed = true;
        }   

        if (!Input.GetKeyDown("q")){
            this.qPressed = false;
        }




        // ===============================================================================================================
        // First run the movement functionality for the frame
        // ===============================================================================================================



        if (isBeingHeld == true){
            Vector3 mousePos;
            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            // See if the cursor is to the left or right of the ship and set X velocity to catch up
            // NOTE: these are cast as floats so some of the Math functions don't throw a fit
            float xValDesired = ((float)Math.Floor(mousePos.x) + (float).5);
            float yValDesired = ((float)Math.Floor(mousePos.y) + (float).5);

            // If there is no bearing check to see if a new one should be set
            if (this.curr_bearing == "NONE"){

                // If the mouse is in another square to the right and there is no collision to the right
                if (((float)Math.Floor(mousePos.x) > this.path[this.lenPath].x) && (!isOccupied(this.path[this.lenPath].location + new Vector3(1,0,0)))){
                    this.curr_bearing = "X";
                }

                // If the mouse is in another square to the left and there is no collision to the left 
                else if ((mousePos.x < this.path[this.lenPath].x) && (!isOccupied(this.path[this.lenPath].location + new Vector3(-1,0,0)))){
                    this.curr_bearing = "X";
                }

                // If the mouse is in another square to the up and there is no collision to the up
                else if (((float)Math.Floor(mousePos.y) > this.path[this.lenPath].y) && (!isOccupied(this.path[this.lenPath].location + new Vector3(0,1,0)))){
                    this.curr_bearing = "Y";
                }


                // If the mouse is in another square to the down and there is no collision to the down
                else if ((mousePos.y < this.path[this.lenPath].y) && (!isOccupied(this.path[this.lenPath].location + new Vector3(0,-1,0)))){
                    this.curr_bearing = "Y";
                }

            }

            

                // If the ship is bearing to the left or the right
                if  (this.curr_bearing == "X"){

                    // Check if the ship is not at the center X value of the grid square the cursor is in
                    // Add necessary velocity if not
                    if (Math.Abs(this.gameObject.transform.position.x - xValDesired) > .05) {

                        rb.velocity = new Vector3(Math.Min(10*Math.Abs(this.gameObject.transform.position.x - xValDesired), (float)35)*Math.Sign(xValDesired - this.gameObject.transform.position.x), 0, 0);
                    }

                    // If the ship is already in the correct position, that position is the home square for the leg, and the ship has left and is coming back  
                    else if ((this.xCurrCoord == this.currPermPosition.x) && (this.lenPath > this.lenPermPath)){

                        // snap to the square and reset bearing
                        superficialSnapToPermPath();
                        this.curr_bearing = "NONE";

                    }

                }

                // If the ship is bearing to the up or down
                if (this.curr_bearing == "Y"){
                    Debug.Log("yCurrCoord");
                    Debug.Log(yCurrCoord);
                    Debug.Log("this.currPermPosition.y");
                    Debug.Log(this.currPermPosition.y);
                    // Check if the cursor is indicating a grid square the ship is not at the center Y value of
                    // Add necessary velocity if not
                    if (Math.Abs(this.gameObject.transform.position.y - yValDesired) > .05) {
                        rb.velocity = new Vector3(0, Math.Min(10*Math.Abs(this.gameObject.transform.position.y - yValDesired), (float)35)*Math.Sign(yValDesired - this.gameObject.transform.position.y), 0);
                    }

                    // If the ship is already in the correct position, that position is the home square for the leg, and the ship has left and is coming back  
                    else if ((this.yCurrCoord == this.currPermPosition.y) && (this.lenPath > this.lenPermPath)){

                        // snap to the square and reset bearing
                        superficialSnapToPermPath();
                        this.curr_bearing = "NONE";

                    }

                }

                // If the ship currently has no bearing the velocity stops
                if (this.curr_bearing == "NONE"){
                    rb.velocity = new Vector3(0, 0, 0);

                }

                // // Before we go to the next frame clip the velocity down so the ship doesn't fly out of the play area or anything like that
                // rb.velocity.x = new Vector3 (Math.Min(rb.velocity.x, 75.0), 0, 0)
                // rb.velocity.y = new Vector3 (0, Math.Min(rb.velocity.y, 75.0), 0)

        }

        // ===============================================================================================================
        // Now run the path management functionality
        // ===============================================================================================================

        // Check to see if the ship is in a new GridSquare
        // NOTE: currently this checks for strictly ANY change in X or Y. If the ship jumps between frames it will not correct.

        if ((this.path[this.lenPath].x != this.xCurrCoord) || (this.path[this.lenPath].y != this.yCurrCoord)){

            // Mark the old square as unoccupied
            this.path[this.lenPath].isOccupied = false;

            // Add a sprite to mark the path
            // this.createPathSprite(this.path[lenPath].x, this.path[lenPath].y, this.lenPath);

            // Add the new GridSquare to the path and update lenPath, xCurrCoord, yCurrCoord
            this.lenPath += 1;
            this.path[this.lenPath] = new GridSquare(new Vector3(this.xCurrCoord, this.yCurrCoord, 0), this.xCurrCoord, this.yCurrCoord, true);
        }

        //Finally check to see if the ship is in a square that completes a viable path-leg i.e. it has hit a wall
        int currXIndex_gamestate = (this.walls.WorldToCell(this.path[this.lenPath].location).x  + 10);
        int currYIndex_gamestate = (this.walls.WorldToCell(this.path[this.lenPath].location).y  + 10);
        // the + 10 is because the array tracking the gamestate runs [0, 19] and the Tilemap tracks [-10, 9]

        // If its bearing left or right and not in the starting square check for left or right collision 
        if ((this.curr_bearing == "X") && ((currXIndex_gamestate - 10) != this.currPermPosition.x)){

            // check to the right and the left 
            if (isOccupied(this.path[this.lenPath].location - new Vector3(1, 0, 0)) || isOccupied(this.path[this.lenPath].location - new Vector3(-1, 0, 0))){
                // Catch up the permanent path with the new leg 
                updatePermPath();
            }
        }

        // If its bearing up or down check for up or down collision
        if ((this.curr_bearing == "Y") && ((currYIndex_gamestate - 10) != this.currPermPosition.y)){
            // check to the up and down
            if (isOccupied(this.path[this.lenPath].location - new Vector3(0, 1, 0)) || isOccupied(this.path[this.lenPath].location - new Vector3(0, -1, 0))){
                // Catch up the permanent path with the new leg 
                updatePermPath();
            }
        }
     


    }

    private void updatePermPath(){

        GridSquare startOfLeg = this.currPermPosition;
        GridSquare endOfLeg = this.path[this.lenPath];
            
        // before we leave add the position and length of the place we are leaving as a PathState to the history stack so we could jump back if need be
        pathHistory.Push(new PathState(this.currPermPosition, this.lenPermPath));
        Debug.Log("pathHistory len: " + pathHistory.Count.ToString());

        // if this is the first completed leg add the "0th" sprite to mark where the ship started
        if (this.lenPermPath == 0){
            createPathSprite(this.permPath[0].x, this.permPath[0].y, this.lenPermPath);
        }


        // If moving left or right
        if (this.curr_bearing == "X"){
            // add squares to permPath to cover the distance travelled by the leg
            // sign marks if left or right
            int sign = Math.Sign(endOfLeg.x - startOfLeg.x);
            for (int i = 1; i <= Math.Abs(endOfLeg.x - startOfLeg.x); i++){
                this.lenPermPath += 1;
                this.permPath[this.lenPermPath] = new GridSquare(startOfLeg.location + new Vector3(i*sign, 0, 0), startOfLeg.x  + i*sign, startOfLeg.y, true);
                createPathSprite(this.permPath[lenPermPath].x, this.permPath[lenPermPath].y, this.lenPermPath);
            
            }
        }        


        // If moving up or down
        if (this.curr_bearing == "Y"){
            // add squares to permPath to cover the distance travelled by the leg
            // sign marks if up or down
            int sign = Math.Sign(endOfLeg.y - startOfLeg.y);
            for (int i = 1; i <= Math.Abs(endOfLeg.y - startOfLeg.y); i++){
                this.lenPermPath += 1;
                this.permPath[this.lenPermPath] = new GridSquare(startOfLeg.location + new Vector3(0, i*sign, 0), startOfLeg.x, startOfLeg.y + i*sign, true);
                createPathSprite(this.permPath[lenPermPath].x, this.permPath[lenPermPath].y, this.lenPermPath);
            }
        }

        // Reset the bearing
        this.curr_bearing = "NONE";

        // Mark the square we left from as unoccupied
        this.gameStateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;
        
        // // catch up the permanent path tracker to the newly completed leg
        // for (int i = this.lenPermPath + 1; i <= this.lenPath; i++){
        //     this.permPath[i] = this.path[i];
        //     this.lenPermPath += 1;
        //     createPathSprite(this.permPath[i].x, this.permPath[i].y, this.lenPermPath);
        // }

        // update currPermPosition
        this.currPermPosition = this.permPath[lenPermPath];   

        // reset path and lenPath so it matches the newly cleaned lenPermPath
        this.path = this.permPath;
        this.lenPath = this.lenPermPath;

        // mark the newly occupied square as occupied by the ship
        this.gameStateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = shipID;



        // clean up the ship location
        superficialSnapToPermPath();

    }



    // moves the ship to the end of its last viable leg and resets relevant information
    // NOTE: DOES NOT INTERACT WITH gameStateGrid[] 
    private void snapToPermPath(){

        // Reset position and position bookkeeping
        this.locationTransform.localPosition = this.walls.GetCellCenterWorld(new Vector3Int(this.currPermPosition.x, this.currPermPosition.y, 0));
        this.xCurrCoord = (int)Math.Floor(this.locationTransform.position.x);
        this.yCurrCoord = (int)Math.Floor(this.locationTransform.position.y);

        // Reset lenPath
        this.lenPath = this.lenPermPath;

        //Reset pathArray
        this.path = this.permPath;

        // Turn off physics so it wont move when another ship collides
        rb.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    // just moves the ship and doesn't alter any underlying variables
    private void superficialSnapToPermPath(){
        // Reset position
        this.locationTransform.localPosition = this.walls.GetCellCenterWorld(new Vector3Int(this.currPermPosition.x, this.currPermPosition.y, 0));
    }

    // returns true if there is a wall at query location
    private Boolean isOccupied(Vector3 queryLoc){
        // createPathSprite((int)queryLoc.x, (int)queryLoc.y, this.gameStateGrid[(int)queryLoc.x + 10, (int)queryLoc.y + 10]);
        if ((this.gameStateGrid[(int)queryLoc.x + 10, (int)queryLoc.y + 10] == 0) || (this.gameStateGrid[(int)queryLoc.x + 10, (int)queryLoc.y + 10] == this.shipID)){
            return false;
        }
        else{
            return true;
        }

    }


    void createPathSprite(int x, int y, int lenPath){

        // Create a new sprite that will represent the path
        GameObject newPathSprite = new GameObject();
        SpriteRenderer newSpriteRenderer = newPathSprite.AddComponent<SpriteRenderer>();
        newSpriteRenderer.sprite = pathSpriteTexture;
        newPathSprite.transform.localPosition = new Vector3(x+.5f, y+.5f, 0);

        // Add a canvas that sprite
        newPathSprite.AddComponent<Canvas>();

        // Add text to canvas
        GameObject myText = new GameObject();
        myText.transform.parent = newPathSprite.transform;
        myText.name = "pathTextSprite";

        TMPro.TextMeshPro text = myText.AddComponent<TMPro.TextMeshPro>();
        // text.font = (Font)Resources.Load("MyFont");
        text.text = lenPath.ToString();
        text.alignment = TMPro.TextAlignmentOptions.Center;

        // Dynamically Determine Font Size and Text Box Attributes
        // 1 digit numbers
        if (Math.Floor(Math.Log10(lenPath) + 1) <= 1){
            text.fontSize = 8;

            // Text position
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0, 0, 0);
            rectTransform.sizeDelta = new Vector2(0, 0.8f);
        
        }
        // 2 digit numbers
        if (Math.Floor(Math.Log10(lenPath) + 1) == 2){
            text.fontSize = 7;
            // Text position
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0, 0, 0);
            rectTransform.sizeDelta = new Vector2(2, 0.8f);
        
        }
        // 3 digit numbers
        if (Math.Floor(Math.Log10(lenPath) + 1) >= 3){
            text.fontSize = 5;
            // Text position            
            RectTransform rectTransform = text.GetComponent<RectTransform>();
            rectTransform.localPosition = new Vector3(0, 0, 0);
            rectTransform.sizeDelta = new Vector2(2, 0.8f);
        } 

        // Add this sprite to the pathSpriteList so it can be managed later
        this.pathSpriteList[lenPath] = newPathSprite;

    }

    public void setID(int ID){
        this.shipID = ID;
    }

    // resets all the ships to their positions at the start of the round
    private void resetShipPath(){

        // Delete previously placed path sprites
        for (int i = 0; i <= this.lenPath; i++){
            Destroy(this.pathSpriteList[i]);
        }
        this.pathSpriteList = new GameObject[484];

        // mark the old square square as empty
        this.gameStateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;

        // Reset path, permPath, and lenPath
        this.permPath = new GridSquare[484];
        this.currPermPosition = this.roundStartSquare;
        this.permPath[0] = this.roundStartSquare;
        
        this.path = new GridSquare[484];
        this.path[0] = this.roundStartSquare;

        this.lenPath = 0;
        this.lenPermPath = 0;

        // Reset ship location
        snapToPermPath();

    }

    // Walk the ship back one length 
    private void resetLeg(){

        PathState returnState = pathHistory.Pop();
        GridSquare returnLocation = returnState.location;
        int returnStepNumber = returnState.lenPermPath;

        // If we have reset all the way to the starting position
        if (pathHistory.Count == 0){
            pathHistory.Push(new PathState(this.roundStartSquare, 0));
        }

        // Delete previously placed path sprites
        for (int i = returnStepNumber; i <= this.lenPermPath; i++){
            Destroy(this.pathSpriteList[i]);
            this.pathSpriteList[i] = null;
        }
        
        // update the gamestateGrid
        this.gameStateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;
        this.gameStateGrid[returnLocation.x + 10, returnLocation.y + 10] = shipID;


        // Reset lenPermPath
        this.lenPermPath = returnStepNumber;

        // tell the script that the ship is back at the previous leg
        this.currPermPosition = this.permPath[lenPermPath];


        // tell the renderers to move the sprite back at the previous leg
        snapToPermPath();

    }

    private void markSelected(){
        //
        this.isSelected = true;

        // report to the gameManager that this ship has been selected
        this.gameManager.GetComponent<ManagerScript>().markSelected(this.shipID);


        // add something to the sprite
        // TODO
    }

    public void deselect(){
        this.isSelected = false;

        // remove the particle from the sprite
        // TODO
    }

    private void OnMouseDown(){

        if (Input.GetMouseButtonDown(0))
        {
                Vector3 mousePos;
                mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);
                // tells the physics engine to just freeze rotation; let X and Y now change
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                isBeingHeld = true;

                markSelected();

        }
        
    } 

    private void OnMouseUp(){
        isBeingHeld = false;

        // Halt the motion of the ship (snapping back is handled in snapToPermPath())
        rb.velocity = new Vector3(0, 0, 0);
        snapToPermPath();



        this.curr_bearing = "NONE";
    }

}

// a way to refer to a grid element
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

}

// A class to be used as a shorthand to refer the full gamestate of a single ship
// i.e. it contains all necessary information to recreate the game actions of that ship
public class PathState
{
    public GridSquare location;
    public int lenPermPath;
    // public GridSquare[] permPath;

    public PathState(GridSquare location, int lenPermPath){
        this.location = location;
        this.lenPermPath = lenPermPath; 
    }
}


// TODO: FIX OUT OF BOUNDS ON X AXIS