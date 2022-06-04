using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using Mirror;


public class PlayerManager : NetworkBehaviour
{

    void Start()
    {   

        if (isLocalPlayer)
        {
            GameManager.Instance.spawnShips();
            GameManager.Instance.initializeGameGrid();
            GameManager.Instance.initializeShips();
            GameManager.Instance.wakeManager(this.gameObject.GetComponent<PlayerManager>());
        }
    }


    // Update is called once per frame
    void Update()
    {

    }

    void Awake()
    {
        if (isLocalPlayer)
        {

        }

    }

    public void postSolution(Tuple<int, Vector3>[] movesList, int numMoves, int lenPath, int[,] gamestateGrid)
    {

        int[] flatGamestateGrid = flattenGrid(gamestateGrid);



        // Send solution to the client
        // TODO: RUN A CHECK IN HERE TO SEE IF ITS ACTUALLY A VALID SOLUTION
    }


    // takes in a 2d array (likely the gamestateGrid) and flattens it since 2d arrays cannot be passed over the network
    private int[] flattenGrid(int[,] grid)
    {
        int[] resGrid = new int[grid.GetLength(0)*grid.GetLength(1)];

        // iterate over the whole grid and place the elems in the response object
        // j is the "x value" (dim0) and i is the "y value" (dim1)
        for (int i = 0; i < grid.GetLength(1); i++)
        {
            for (int j = 0; i < grid.GetLength(0); j++)
            {
                resGrid[i*10 + j] = grid[j, i];
            }

        }

        return resGrid;
    }


    [ Command ]
    private void CmdPostSolution(Tuple<int, Vector3>[] movesList, int numMoves, int lenPath, int[,] flatGamestateGrid)
    {
        //TODO run server side validation check

        // save gamestate to server

        // send message to all clients that a solution has been posted

        // RPC to finisher's client with "waiting for other players" Screen
    }

}
