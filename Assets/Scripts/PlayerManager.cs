using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;
using Mirror;


public class PlayerManager : NetworkBehaviour
{

    // this is a server only object that stores the state of the game between rounds
    private int[,] serverGamestateGrid;
    private MyNetworkManager NetworkManagerScript;

    private bool inServerCountdown = false;
    private float countdownTime = 30;
    private int lastSentTime;

    void Start()
    {   

        // get ref to network manager
        // cannot be done with singleton since mirror API implements its own singleton declaration for NetworkManager
        this.NetworkManagerScript = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>();

        if (isLocalPlayer)
        {

            // make ships
            GameManager.Instance.spawnShips();

            // fill in the game grid 
            GameManager.Instance.initializeGameGrid();
            
            // create the ships
            GameManager.Instance.initializeShips();
            
            // tell the game manager to start running the game
            GameManager.Instance.wakeManager(this.gameObject.GetComponent<PlayerManager>());

        }

        if (isServer)
        {


            // actually just leave this blank until first submission is posted
            // initializeServerGamestateGrid();
        }
    }


    // Update is called once per frame
    void Update()
    {
        if (isServer)
        {
            if (inServerCountdown)
            {
                countdownTime -= Time.deltaTime;
                if ((countdownTime % 1 < .1) && (Math.Floor(countdownTime) != lastSentTime))
                {
                    RpcBroadcastCountdownRemaining((int)Math.Floor(countdownTime));
                    lastSentTime = (int)Math.Floor(countdownTime);
                }
            }
        }

    }

    void Awake()
    {

    }

    public void postSolution(List<Tuple<int, Vector3Int, Vector3Int>> movesList, int numMoves, int lenPath, int[,] gamestateGrid)
    {
        if (isLocalPlayer)
        {
            int[] flatGamestateGrid = flattenGrid(gamestateGrid);
            string movesListString = deconstructMovesList(movesList);
            // Send solution to the client
            // TODO: RUN A CHECK IN HERE TO SEE IF ITS ACTUALLY A VALID SOLUTION
            CmdPostSolution(movesListString, numMoves, lenPath, flatGamestateGrid);

        }

    }

    // takes the moveslist and turns it into a string so it can be communicated over the network
    // List<Tuple<int, Vector3Int>> is not a valid type 
    private string deconstructMovesList(List<Tuple<int, Vector3Int, Vector3Int>> movesList)
    {
        Debug.Log("movesList count :");
        Debug.Log(movesList.Count);
        string resString = "";
        foreach (Tuple<int, Vector3Int, Vector3Int> tuple in movesList)
        {
            resString = resString + tuple.Item1.ToString() + "," + tuple.Item2.x.ToString() + ","  + tuple.Item2.y.ToString() + ","  + tuple.Item3.x.ToString() + ","  + tuple.Item3.y.ToString() + ':';
        }
        Debug.Log("resString");
        Debug.Log(resString);
        return resString;
    }

    // takes in a 2d array (likely the gamestateGrid) and flattens it since 2d arrays cannot be passed over the network
    private int[] flattenGrid(int[,] grid)
    {
        int[] resGrid = new int[grid.GetLength(0)*grid.GetLength(1)];

        // iterate over the whole grid and place the elems in the response object
        // j is the "x value" (dim0) and i is the "y value" (dim1)
        for (int i = 0; i < grid.GetLength(1); i++)
        {
            for (int j = 0; j < grid.GetLength(0); j++)
            {
                // Debug.Log("running flatten with i: " + i + "j: " + j);
                resGrid[(i*10 + j)] = grid[j, i];
            }

        }

        return resGrid;
    }


    // client calls this code to tell the server that it would like to post a solution to the current board
    [ Command ]
    public void CmdPostSolution(string deconstructedMovesList, int numMoves, int lenPath, int[] flatGamestateGrid)
    {
        //TODO run server side validation check

        // TODO this could be a clientRpc?
        foreach (NetworkConnectionToClient conn in NetworkManagerScript.getClientConnections())
            {
                RpcNotifySolutionPosted(conn, numMoves, lenPath);
                
            }

        // TODO save gamestate to server

        // RPC to finisher's client with "waiting for other players" screen which will counteract the affects of RpcNotifySolutionPosted
        RpcActivateWaitScreen();

        inServerCountdown = true;
    }

    // server calls this code to tell all the clients that someone has posted a solution to the current board
    [ TargetRpc ]
    public void RpcNotifySolutionPosted(NetworkConnection conn, int numMoves, int lenPath)
    {
        Debug.Log("Another client has posted a solution!");
        GameManager.Instance.activateCountdown(numMoves, lenPath);

    }

    [ TargetRpc ]
    public void RpcActivateWaitScreen()
    {
        // foreach (NetworkConnectionToClient conn in NetworkManagerScript.getClientConnections())
        //     {
        //         Debug.Log("conn");
        //         Debug.Log(conn);
        //     }
        GameManager.Instance.activateWaitScreen();
    }


    // broadcasts the timeremaining to all clients who are in countdown
    // sends as an int and only sends at second intervals
    [ ClientRpc]
    public void RpcBroadcastCountdownRemaining(int timeRemaining)
    {
        GameManager.Instance.updateTimerText(timeRemaining);
    }

}
