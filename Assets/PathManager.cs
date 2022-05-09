// DEPRECATED 5.6.2022
// ALL FUNCTIONALITY ADDED TO velocity_dragging.cs


// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using TMPro;

// public class PathManager : MonoBehaviour
// {

//     // An updating array that has every GridSquare the ship has been through on its current path
//     // 400 is the longest possible path containing all squares of the 22 x 22 play area 
//     // NOTE: players could retrace their path and go over this. Probably do some cyclical graph checking to prohibit this.
//     public GridSquare[] path = new GridSquare[484];
//     public int lenPath = 0;
//     public int xCurrCoord;
//     public int yCurrCoord;
//     public Sprite pathSpriteTexture;
//     private SpriteRenderer sr;

//     // FUNCTIONALITY MOVED TO velocity_dragging.cs
//     // private string curr_bearing = "NONE";
//     // // This variable is a state describing whether the ship is currently considering a move along the X or Y-Axis
//     // // 'X' is the X-Axis 
//     // // 'Y' is the Y-Axis 
//     // // 'NONE' means the ship is not currently considering a move

//     void Awake(){
//         // Sprite Renderer to render path sprites later
//         sr = GetComponent<SpriteRenderer>();
    
//     }

//     // Start is called before the first frame update
//     void Start()
//     {
//         // Set staring position and add the ship's starting GridSquare to the path        
//         this.xCurrCoord = (int)Math.Floor(this.gameObject.transform.position.x);
//         this.yCurrCoord = (int)Math.Floor(this.gameObject.transform.position.y);
//         this.path[0] = new GridSquare(this.xCurrCoord, this.yCurrCoord, true);
//     }

//     // Update is called once per frame
//     void Update()
//     {
//         // Update location
//         this.xCurrCoord = (int)Math.Floor(this.gameObject.transform.position.x);
//         this.yCurrCoord = (int)Math.Floor(this.gameObject.transform.position.y);

//         // FUNCTIONALITY MOVED TO velocity_draggin.cs
//         // // If the ship is not yet considering a move dec
//         // if (this.curr_bearing == "NONE"){

//         //     // If the ship has moved out of its square to the left or right
//         //     if (this.path[lenPath].x != this.xCurrCoord){
//         //         this.curr_bearing = "X";
//         //     }
//         //     // Else if it has moved along out to the up or down
//         //     else if(this.path[lenPath].y != this.yCurrCoord){
//         //         this.curr_bearing = "Y";
//         //     }

//         // }

//         // // If the bearing is to the X and there is new movement in the cursor to the left or right
//         // if ((curr_bearing == "X") && (this.path[lenPath].x != this.xCurrCoord)){

//         //     // Add a sprite to mark the path on the departure square
//         //     this.createPathSprite(this.path[lenPath].x, this.path[lenPath].y, this.lenPath);

//         //     //Add the new 
//         // }

//         // Check to see if the ship is in a new GridSquare
//         // NOTE: currently this checks for strictly ANY change in X or Y. If the ship jumps between frames it will not correct.
//         if ((this.path[lenPath].x != this.xCurrCoord) || (this.path[lenPath].y != this.yCurrCoord)){

//             // Mark the old square as unoccupied
//             this.path[lenPath].isOccupied = false;

//             // Add a sprite to mark the path
//             this.createPathSprite(this.path[lenPath].x, this.path[lenPath].y, this.lenPath);

//             // Add the new GridSquare to the path and update lenPath, xCurrCoord, yCurrCoord
//             this.lenPath += 1;
//             this.path[lenPath] = new GridSquare(this.xCurrCoord, this.yCurrCoord, true);
//         }

//     }

//     // Create a sprite at grid square with coords (x, y), indicating the current path for the ship
//     // marked with a digit (lenPath) indicating how far the ship is at this step
//     void createPathSprite(int x, int y, int lenPath){

//         // Create a new sprite that will represent the path
//         GameObject newPathSprite;
//         newPathSprite = new GameObject();
//         SpriteRenderer newSpriteRenderer = newPathSprite.AddComponent<SpriteRenderer>();
//         newSpriteRenderer.sprite = pathSpriteTexture;
//         newPathSprite.transform.localPosition = new Vector3(x+.5f, y+.5f, 0);

//         // Add text to the top of the sprite to indicate how long the path is
//         newPathSprite.AddComponent<Canvas>();

//         // Text

//         GameObject myText = new GameObject();
//         myText.transform.parent = newPathSprite.transform;
//         myText.name = "pathTextSprite";

//         TMPro.TextMeshPro text = myText.AddComponent<TMPro.TextMeshPro>();
//         // text.font = (Font)Resources.Load("MyFont");
//         text.text = lenPath.ToString();
//         text.alignment = TMPro.TextAlignmentOptions.Center;

//         // Dynamically Determine Font Size and Text Box Attributes
//         // 1 digit numbers
//         if (Math.Floor(Math.Log10(lenPath) + 1) <= 1){
//             text.fontSize = 8;

//             // Text position
//             RectTransform rectTransform = text.GetComponent<RectTransform>();
//             rectTransform.localPosition = new Vector3(0, 0, 0);
//             rectTransform.sizeDelta = new Vector2(0, 0.8f);
        
//         }
//         // 2 digit numbers
//         if (Math.Floor(Math.Log10(lenPath) + 1) == 2){
//             text.fontSize = 7;
//             // Text position
//             RectTransform rectTransform = text.GetComponent<RectTransform>();
//             rectTransform.localPosition = new Vector3(0, 0, 0);
//             rectTransform.sizeDelta = new Vector2(2, 0.8f);
        
//         }
//         // 3 digit numbers
//         if (Math.Floor(Math.Log10(lenPath) + 1) >= 3){
//             text.fontSize = 5;
//             // Text position            
//             RectTransform rectTransform = text.GetComponent<RectTransform>();
//             rectTransform.localPosition = new Vector3(0, 0, 0);
//             rectTransform.sizeDelta = new Vector2(2, 0.8f);
//         } 

//         // TODO: MAKE SHIPS ONLY GO IN ONE DIRECTION

//         // TODO: MAKE THE SHIPS COLLIDE

//         // TODO: RUN GRAPH DETECTION TO MAKE SURE THE PATH IS THE MOST EFFICIENT

//         // TODO: MAKE THE TEXT FIT INTO THE SPHERE

//         // TODO: TIE THESE INTO THE MANAGERSCRIPT TO CHECK FOR SOLUTION COMPLETION

//         // TODO: CHANGE SCENE WHEN SOLUTIONS ARE SUBMITTED

//         // TODO: THINK ABOUT COMBINING VELOCITY_DRAGGING AND PATHMANAGER

//     }
// }

// public class GridSquare
// {
//     public int x;
//     public int y;
//     public bool isOccupied = false;

//     public GridSquare(int x, int y, bool isOccupied){
//         this.x = x;
//         this.y = y;
//         this.isOccupied = isOccupied;
//     }

// }
