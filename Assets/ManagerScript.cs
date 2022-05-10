using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ManagerScript : MonoBehaviour
{

    public int totalSolutionLength;
    public GameObject ship1;

    // This is a simple gameStateGrid that can be conveniently referenced
    // It marks all walls as 1s, ships as 2s and empty spaces as 0s
    public int[,] gameStateGrid = new int[20, 20];   

    // COMMUNICATE THE NUMBER OF SHIPS TO THE SCRIPTS HERE
    public int numShips = 2;
    private Dictionary<int, GameObject> shipDict = new Dictionary<int, GameObject>();
    // NOTE: this tracks ships by NAME as it appears in the entity list i.e. ship1, ship2, and does not match their ship id
    //          shipID = shipName + 1 (currently)

    private GameObject grid;
    private Tilemap walls;


    // Called when the script instance is being loaded
    void Awake(){
        // Name all of the ships
        for (int i = 1; i <= this.numShips; i++){

            // referencce the ship            
            GameObject ship_i = GameObject.Find(("Ship" + i.ToString()));

            // add the ship to the shipDict
            this.shipDict.Add(i, ship_i);

            // set the ID. the + 1 is because ID is globally defined for tracking the whole gamestate
            // in this context ID 0 = unoccupied and ID 1 = wall so unfortunately ship1 has an ID of 2 etc.
            ship_i.GetComponent<velocity_dragging>().setID(i+1);

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

        // Iterate through the starting game state and add all walls to the grid
        for (int i = -10; i < 10; i++){
            for (int j = -10; j < 10; j++){
                // Debug.Log("Checking");
                // Debug.Log(new Vector3Int(i, j, 0));

                // Try to get the wall at (i, j). y = 0 always cause it's 2D
                TileBase wall = walls.GetTile(new Vector3Int(i, j, 0));
                if (wall != null){
                    // Debug.Log("Marking down");
                    gameStateGrid[i+10, j+10] = 1;

                    // Center of the cell transform
                    Vector3 worldPos = walls.GetCellCenterWorld(new Vector3Int(i, j, 0));
       

                    // OLD DEBUGGING CODE             
                    // // Debug sprite
                    // GameObject newPathSprite;
                    // newPathSprite = new GameObject();
                    // newPathSprite.name = "newPathSprite";
                    // SpriteRenderer newSpriteRenderer = newPathSprite.AddComponent<SpriteRenderer>();
                    // // newSpriteRenderer.sprite = pathSpriteTexture;
                    // newPathSprite.transform.localPosition = worldPos;

                    // // Add text to the top of the sprite to indicate how long the path is
                    // newPathSprite.AddComponent<Canvas>();

                    // GameObject myText = new GameObject();
                    // // myText.transform.parent = newPathSprite.transform;
                    // myText.name = "pathTextSprite";
  
        

                    // TMPro.TextMeshPro text = myText.AddComponent<TMPro.TextMeshPro>();
                    // // text.font = (Font)Resources.Load("MyFont");
                    // text.text = new Vector3Int(i, j, 0).ToString();
                    // text.alignment = TMPro.TextAlignmentOptions.Center;
                    // text.fontSize = 3;
                    // // walls.SetColor(new Vector3Int(i, j, 0), Color.black);
                    // // walls.DeleteCells(new Vector3Int(i, j, 0), new Vector3Int(1, 1, 1));
                    // RectTransform rectTransform = text.GetComponent<RectTransform>();
                    // rectTransform.localPosition = worldPos;
                    // rectTransform.sizeDelta = new Vector2(2, 0.8f);
                }
            
            }

        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int[,] getGamestateGrid(){
        return this.gameStateGrid;
    }

    // marks globally which ship is currently highlighted
    public void markSelected(int shipID){

        // deselect all other ships
        foreach (KeyValuePair<int, GameObject> kvp in this.shipDict){
            if (kvp.Key + 1 != shipID){
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
}
