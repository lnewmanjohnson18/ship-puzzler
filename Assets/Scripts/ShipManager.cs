using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using TMPro;


public class ShipManager : MonoBehaviour
{

    // Debugging Variables
    public bool wasTouched = false;
    public float minVelocityX = 0;
    public float minVelocityY = 0;

    public int shipID;
    public int notSet;
    public Rigidbody2D rb;
    public Vector3Int roundStartSquare;
    private bool isBeingHeld = false;
    private bool isSelected = false;
    public Vector3Int[] path = new Vector3Int[484];
    public Vector3Int[] permPath = new Vector3Int[484];
    private GameObject[] pathSpriteList = new GameObject[484];
    private Stack<PathState> pathHistory = new Stack<PathState>();


    public int lenPath = 0;
    public int lenPermPath = 0;
    public int xCurrCoord;
    public int yCurrCoord;
    public Sprite pathSpriteTexture;
    private SpriteRenderer sr;
    private GameObject gridObject;
    private Tilemap walls;
    private int[,] gameStateGrid;
    private Transform locationTransform;
    public GameObject gameManager;


    private string curr_bearing = "NONE";
    // This variable is a state describing whether the ship is currently considering a move along the X or Y-Axis
    // 'X' is the X-Axis 
    // 'Y' is the Y-Axis 
    // 'NONE' means the ship is not currently considering a move

    public Vector3Int currPermPosition;
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

        // initializeShip() is called from the NetworkBehaviour PlayerManager when the client is ready
    }

    void Update(){

        // ===============================================================================================================
        // Update and report ship attributes for the frame
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

        // send the most recent viable path state to the manager
        // reportPathState(this.pathHistory.Peek());
        // DEP: THIS IS CALLED DOWN FROM MANAGER NOW

        // ===============================================================================================================
        // See if the path has been reset by user
        // ===============================================================================================================

        // if (Input.GetKeyDown("space")){
        //     resetShipToRoundStart();
        // }
        // DEP: THIS IS CALLED DOWN FROM MANAGER NOW






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
                if (((float)Math.Floor(mousePos.x) > this.path[this.lenPath].x) && (!isOccupied(this.path[this.lenPath] + new Vector3Int(1,0,0)))){
                    this.curr_bearing = "X";
                }

                // If the mouse is in another square to the left and there is no collision to the left 
                else if ((mousePos.x < this.path[this.lenPath].x) && (!isOccupied(this.path[this.lenPath] + new Vector3Int(-1,0,0)))){
                    this.curr_bearing = "X";
                }

                // If the mouse is in another square to the up and there is no collision to the up
                else if (((float)Math.Floor(mousePos.y) > this.path[this.lenPath].y) && (!isOccupied(this.path[this.lenPath] + new Vector3Int(0,1,0)))){
                    this.curr_bearing = "Y";
                }


                // If the mouse is in another square to the down and there is no collision to the down
                else if ((mousePos.y < this.path[this.lenPath].y) && (!isOccupied(this.path[this.lenPath] + new Vector3Int(0,-1,0)))){
                    this.curr_bearing = "Y";
                }

            }

            

                // If the ship is bearing to the left or the right
                if  (this.curr_bearing == "X"){

                    // Check if the ship is not at the center X value of the grid square the cursor is in
                    // Add necessary velocity if not
                    if (Math.Abs(this.gameObject.transform.position.x - xValDesired) > .05) {

                        rb.velocity = new Vector3(Math.Min(10*Math.Abs(this.gameObject.transform.position.x - xValDesired), (float)50)*Math.Sign(xValDesired - this.gameObject.transform.position.x), 0, 0);
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
                    // Check if the cursor is indicating a grid square the ship is not at the center Y value of
                    // Add necessary velocity if not
                    if (Math.Abs(this.gameObject.transform.position.y - yValDesired) > .05) {
                        rb.velocity = new Vector3(0, Math.Min(10*Math.Abs(this.gameObject.transform.position.y - yValDesired), (float)50)*Math.Sign(yValDesired - this.gameObject.transform.position.y), 0);
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

        }

        // ===============================================================================================================
        // Now run the path management functionality
        // ===============================================================================================================

        // Check to see if the ship is in a new GridSquare
        // NOTE: currently this checks for strictly ANY change in X or Y. If the ship jumps between frames it will not correct.

        if ((this.path[this.lenPath].x != this.xCurrCoord) || (this.path[this.lenPath].y != this.yCurrCoord)){

            // Add the new GridSquare to the path and update lenPath, xCurrCoord, yCurrCoord
            this.lenPath += 1;
            this.path[this.lenPath] = new Vector3Int(this.xCurrCoord, this.yCurrCoord, 0);
        }

        //Finally check to see if the ship is in a square that completes a viable path-leg i.e. it has hit a wall
        int currXIndex_gamestate = (this.walls.WorldToCell(this.path[this.lenPath]).x  + 10);
        int currYIndex_gamestate = (this.walls.WorldToCell(this.path[this.lenPath]).y  + 10);
        // the + 10 is because the array tracking the gamestate runs [0, 19] and the Tilemap tracks [-10, 9]

        // If its bearing left or right and not in the starting square check for left or right collision 
        if ((this.curr_bearing == "X") && ((currXIndex_gamestate - 10) != this.currPermPosition.x)){

            // check to the right and the left 
            if (isOccupied(this.path[this.lenPath] - new Vector3Int(1, 0, 0)) || isOccupied(this.path[this.lenPath] - new Vector3Int(-1, 0, 0))){
                // Catch up the permanent path with the new leg 
                updatePermPath();
            }
        }

        // If its bearing up or down check for up or down collision
        if ((this.curr_bearing == "Y") && ((currYIndex_gamestate - 10) != this.currPermPosition.y)){
            // check to the up and down
            if (isOccupied(this.path[this.lenPath] - new Vector3Int(0, 1, 0)) || isOccupied(this.path[this.lenPath] - new Vector3Int(0, -1, 0))){
                // Catch up the permanent path with the new leg 
                updatePermPath();
            }
        }
     
    }


    private void updatePermPath(){

        Vector3Int startOfLeg = this.currPermPosition;
        Vector3Int endOfLeg = this.path[this.lenPath];
            
        // before we leave add the position and length of the place we are leaving as a PathState to the history stack so we could jump back if need be

        // if this is the first completed leg add the "0th" sprite to mark where the ship started
        if (this.lenPermPath == 0){
            createPathSprite((int)this.permPath[0].x, (int)this.permPath[0].y, this.lenPermPath);
        }


        // If moving left or right
        if (this.curr_bearing == "X"){
            // add squares to permPath to cover the distance travelled by the leg
            // sign marks if left or right
            int sign = Math.Sign(endOfLeg.x - startOfLeg.x);
            for (int i = 1; i <= Math.Abs(endOfLeg.x - startOfLeg.x); i++){
                this.lenPermPath += 1;
                this.permPath[this.lenPermPath] = new Vector3Int(startOfLeg.x + i*sign, startOfLeg.y, 0);
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
                this.permPath[this.lenPermPath] = new Vector3Int(startOfLeg.x, startOfLeg.y + i*sign, 0);
                createPathSprite(this.permPath[lenPermPath].x, this.permPath[lenPermPath].y, this.lenPermPath);
            }
        }

        // Reset the bearing
        this.curr_bearing = "NONE";

        // Mark the square we left from as unoccupied
        GameManager.Instance.gamestateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;
        this.gameStateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;

        // update currPermPosition
        this.currPermPosition = this.permPath[lenPermPath];   

        // reset path and lenPath so it matches the newly cleaned lenPermPath
        this.path = this.permPath;
        this.lenPath = this.lenPermPath;

        // mark the newly occupied square as occupied by the ship
        GameManager.Instance.gamestateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = shipID;
        this.gameStateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = shipID;

        // add the now pathState to the stack
        pathHistory.Push(new PathState(this.currPermPosition, this.lenPermPath, this.pathHistory.Count));

        // tell the game manager to update its master list of moves
        GameManager.Instance.addMoveToList(this.shipID, startOfLeg, endOfLeg);

        // clean up the ship location
        superficialSnapToPermPath();

        // check to see if this new position is a valid solution
        checkSolution(this.shipID, this.currPermPosition);

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

    // returns true if there is an object at query location
    private Boolean isOccupied(Vector3Int queryLoc){

        try{
            // if ((this.gameStateGrid[queryLoc.x + 10, queryLoc.y + 10] == 0) || (this.gameStateGrid[queryLoc.x + 10, queryLoc.y + 10] == this.shipID)){
            if ((GameManager.Instance.gamestateGrid[queryLoc.x + 10, queryLoc.y + 10] == 0) || (GameManager.Instance.gamestateGrid[queryLoc.x + 10, queryLoc.y + 10] == this.shipID)){
                return false;
            }
            else{
                return true;
            }   
        }
        catch (IndexOutOfRangeException e){
            Debug.Log(e.Message);
            return false;
        }



    }


    void createPathSprite(int x, int y, int lenPath){

        // Create a new sprite that will represent the path
        GameObject newPathSprite = new GameObject();
        newPathSprite.name = "pathTextSprite";
        SpriteRenderer newSpriteRenderer = newPathSprite.AddComponent<SpriteRenderer>();
        newSpriteRenderer.sprite = pathSpriteTexture;
        newPathSprite.transform.localPosition = new Vector3(x+.5f, y+.5f, 0);
        newSpriteRenderer.sortingOrder = this.shipID*10;

        // Add a canvas that sprite
        newPathSprite.AddComponent<Canvas>();

        // Add text to canvas
        GameObject myText = new GameObject();
        myText.transform.parent = newPathSprite.transform;
        myText.name = "pathTextSpriteText";

        TMPro.TextMeshPro text = myText.AddComponent<TMPro.TextMeshPro>();
        // text.font = (Font)Resources.Load("MyFont");
        text.text = lenPath.ToString();
        text.color = Color.black;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.sortingOrder = (this.shipID*10 + 1);

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
        Debug.Log("set ID called. set to " + ID);
        this.shipID = ID;
    }

    // resets all the ships to their positions at the start of the round
    public void resetShipToRoundStart(){

        // Delete previously placed path sprites
        for (int i = 0; i <= this.lenPath; i++){
            Destroy(this.pathSpriteList[i]);
        }
        this.pathSpriteList = new GameObject[484];

        // mark the old square square as empty
        GameManager.Instance.gamestateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;
        this.gameStateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;

        // mark the new square as occupied
        GameManager.Instance.gamestateGrid[this.roundStartSquare.x + 10, this.roundStartSquare.y + 10] = this.shipID;
        this.gameStateGrid[this.roundStartSquare.x + 10, this.roundStartSquare.y + 10] = this.shipID;

        // Reset path, permPath, and lenPath
        this.permPath = new Vector3Int[484];
        this.currPermPosition = this.roundStartSquare;
        this.permPath[0] = this.roundStartSquare;
        
        // reset path history
        this.pathHistory = new Stack<PathState>();
        this.pathHistory.Push(new PathState(this.roundStartSquare, 0, 0));
    

        this.path = new Vector3Int[484];
        this.path[0] = this.roundStartSquare;

        this.lenPath = 0;
        this.lenPermPath = 0;

        // Reset ship location
        snapToPermPath();

    }

    // Walk the ship back one length 
    public void resetLeg(){

        // Pop off the current location
        this.pathHistory.Pop();

        // If we have reset all the way to the starting position add the starting buffer state
        if (this.pathHistory.Count == 0){
            this.pathHistory.Push(new PathState(this.roundStartSquare, 0, 0));
        }

        // look at location we want to reset back to with peek
        PathState returnState = this.pathHistory.Peek();
        Vector3Int returnLocation = returnState.location;
        int returnStepNumber = returnState.lenPermPath;


        // Delete previously placed path sprites
        for (int i = returnStepNumber + 1; i <= this.lenPermPath; i++){
            Destroy(this.pathSpriteList[i]);
            this.pathSpriteList[i] = null;
        }
        // if this return to round start square also delete the '0th' path sprite just for cleanliness
        if (returnStepNumber == 0){
            Destroy(this.pathSpriteList[0]);
            this.pathSpriteList[0] = null;
        }

        // update the gamestateGrid
        GameManager.Instance.gamestateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;
        this.gameStateGrid[this.currPermPosition.x + 10, this.currPermPosition.y + 10] = 0;
        GameManager.Instance.gamestateGrid[returnLocation.x + 10, returnLocation.y + 10] = this.shipID;
        this.gameStateGrid[returnLocation.x + 10, returnLocation.y + 10] = this.shipID;


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

        GameManager.Instance.markSelected(this.shipID);
        // report to the gameManager that this ship has been selected


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

    private void checkSolution(int shipID, Vector3Int endSquare){
        //TODO MANAGER FIX
        GameManager.Instance.checkSolution(shipID, endSquare);
    }

    public void setRoundStartPosition(){

        // tell the ship that it's current position (the position at the end of the last round) is it's starting position for round n + 1 
        // then erase all previous path and memory with resetShipToRoundStart()
        this.roundStartSquare = this.currPermPosition;
        resetShipToRoundStart();
    }

    public PathState reportPathState(){
        return this.pathHistory.Peek();

    }

    public void initializeShip(int[,] roundStartGamestateGrid, int shipID){
        // TODO, MAKE POSITION AN ARGUMENT THAT COMES IN HERE RATHER THAN SET TO xCURRCOORD
        this.shipID = shipID;
        this.gameStateGrid = roundStartGamestateGrid;
        this.gridObject = GameObject.Find("Grid");
        this.walls = GameObject.Find("Walls").GetComponent<Tilemap>();
        // Set staring position and add the ship's starting GridSquare to the permanent and temporary path        
        this.xCurrCoord = (int)Math.Floor(this.gameObject.transform.position.x);
        this.yCurrCoord = (int)Math.Floor(this.gameObject.transform.position.y);
        this.roundStartSquare = new Vector3Int(this.xCurrCoord, this.yCurrCoord, 0);
        this.path[0] = this.roundStartSquare;
        this.permPath[0] = this.roundStartSquare;
        this.currPermPosition = this.roundStartSquare;
        this.pathHistory.Push(new PathState(this.roundStartSquare, 0, 0));

        if (this.shipID == 0){
            Debug.Log("ERROR: SHIP ATTEMPTED TO MARK LOCATION WITHOUT VALID METADATA (SHIPID)");
        }
        else{
            GameManager.Instance.gamestateGrid[this.xCurrCoord + 10, this.yCurrCoord + 10] = this.shipID;
            this.gameStateGrid[this.xCurrCoord + 10, this.yCurrCoord + 10] = this.shipID;
        }
    }

}