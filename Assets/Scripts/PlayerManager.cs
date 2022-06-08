// this script handles the interaction between Network and Gamestate/Gameboard
// NetworkManager -> PlayerManager  -> GameManager
//                                  -> ShipManager

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
    private NetworkIdentity NetworkIdentity;
    // private uint netID;
    public Tuple<List<Tuple<int, Vector3Int, Vector3Int>>, int, int, int[,]> currSolution;
    private bool inServerCountdown = false;
    private float countdownTime = 30;
    private int lastSentTime;
    
    void Start()
    {   

        // get ref to network manager
        // cannot be done with singleton since mirror API implements its own singleton declaration for NetworkManager
        this.NetworkManagerScript = GameObject.Find("NetworkManager").GetComponent<MyNetworkManager>();

        // // save network connection information
        // this.NetworkIdentity = this.gameObject.GetComponent<NetworkIdentity>();
        // this.netID = this.NetworkIdentity.netId;


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

        }
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            Debug.Log(NetworkManagerScript.getClientConnections());
            // int[,] testGrid = new int[20,20] {
            //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            //     {2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            //     {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
            //     {4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4},
            //     {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},
            //     {6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6},
            //     {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
            //     {8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8},
            //     {9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
            //     {10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10},
            //     {1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1},
            //     {2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2},
            //     {3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3, 3},
            //     {4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4},
            //     {5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5},
            //     {6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6},
            //     {7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7},
            //     {8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8},
            //     {9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9, 9},
            //     {10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10, 10}
            // };

            // Debug.Log("GamestateGrid Print");

            // GameManager.Instance.printGrid(GameManager.Instance.gamestateGrid);

            // GameManager.Instance.printGrid(testGrid);

            // int[] testGrid_flat = flattenGrid(testGrid); 

            // int[,] testGrid_unflat = unflattenGrid(testGrid_flat);

            // GameManager.Instance.printGrid(testGrid_unflat);

        }

        if (isServer)
        {
            // if (Input.GetKeyDown(KeyCode.X))
            // {
            //     cleanupRound();
            // }
            if (inServerCountdown)
            {
                // lower the timer by the frame delta
                countdownTime -= Time.deltaTime;

                // if the countdown is less than .1s send an update to clients to change their text to the next second value
                if ((countdownTime % 1 < .1) && (Math.Floor(countdownTime) != lastSentTime))
                {
                    RpcBroadcastCountdownRemaining((int)Math.Floor(countdownTime));
                    lastSentTime = (int)Math.Floor(countdownTime);
                }

                // if the countdown is over
                if (countdownTime < 0)
                {
                    // clean up the round
                    cleanupRound();
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
            // TODO: RUN A CHECK IN HERE TO SEE IF ITS ACTUALLY A VALID SOLUTION
            int[] flatGamestateGrid = flattenGrid(gamestateGrid);
            string movesListString = deconstructMovesList(movesList);
            
            // if this is the first solution that a client has posted save it and post to the server
            if (!inServerCountdown)
            {
                CmdPostSolution(movesListString, numMoves, lenPath, flatGamestateGrid);
                currSolution = new Tuple<List<Tuple<int, Vector3Int, Vector3Int>>, int, int, int[,]>(movesList, numMoves, lenPath, gamestateGrid);
            }
            // if another client has already posted and we are in the countdown to the end of the round then just save and wait
            else 
            {
                currSolution = new Tuple<List<Tuple<int, Vector3Int, Vector3Int>>, int, int, int[,]>(movesList, numMoves, lenPath, gamestateGrid);
            }


        }

    }

    // brings the round to an end. calls for all most recent solutions and then displays the end of round screen
    public void cleanupRound()
    {
        if (isServer)
        {
            inServerCountdown = false;
            // after the timer has ticked down ask the clients for any final solutions
            // NOTE: this call initiates a chain of network calls
            //  RpcRequestFinalSolutions() -> CmdSendFinalSolution() -> RpcActivateRoundEndScreen()
            RpcRequestFinalSolutions();

        }
    }

    // this is called if the player is playing solo and doesnt want to wait for the countdown
    public void finishCountdown()
    {
        if (isServer)
        {
            countdownTime = 0;
        }
        else
        {
            Debug.Log("not connected to server. solo play unfortunately still requires a host");
        }
    }


    // ===============================================================================================================
    //  NETWORKING METHODS 
    // =============================================================================================================== 

    // client calls this code to tell the server that it would like to post a solution to the current board
    [ Command ]
    public void CmdPostSolution(string deconstructedMovesList, int numMoves, int lenPath, int[] flatGamestateGrid)
    {
        //TODO run server side validation check

        // set the posted solution as the current best
        GameManager.Instance.currWinningGamestate_flat = flatGamestateGrid;
        GameManager.Instance.currBestScore = numMoves;

        // TODO this could be a clientRpc?
        foreach (NetworkConnectionToClient conn in NetworkManagerScript.getClientConnections())
            {
                RpcNotifySolutionPosted(conn, numMoves, lenPath);
                
            }


        // Target RPC to finisher's client with "waiting for other players" screen which will counteract the UI affects of RpcNotifySolutionPosted
        // if argument is true (the client is in solo play) the wait screen will have an end countdown button
        RpcActivateWaitScreen((NetworkManagerScript.getClientConnections().Count == 1) ? true : false);

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
    public void RpcActivateWaitScreen(bool isSolo)
    {
        GameManager.Instance.activateWaitScreen(isSolo);
    }


    // broadcasts the timeremaining to all clients who are in countdown
    // sends as an int and only sends at second intervals
    [ ClientRpc]
    public void RpcBroadcastCountdownRemaining(int timeRemaining)
    {
        GameManager.Instance.updateTimerText(timeRemaining);
    }

    // server calls this to ask if clients have any final solutions before closing the round
    [ ClientRpc ]
    public void RpcRequestFinalSolutions()
    {
        Debug.Log("sending final solution");
        CmdSendFinalSolution(deconstructMovesList(currSolution.Item1), currSolution.Item2, currSolution.Item3, flattenGrid(currSolution.Item4));
    }

    [ Command ]
    public void CmdSendFinalSolution(string deconstructedMovesList, int numMoves, int lenPath, int[] flatGamestateGrid)
    {
        // TODO run server side validation check

        // if the score being reported is lower then set as current best
        // TODO have some way to break a tie here. First? Should alert the players.
        if (numMoves < GameManager.Instance.currBestScore)
        {
            GameManager.Instance.currWinningGamestate_flat = flatGamestateGrid;
            GameManager.Instance.currBestScore = numMoves;
        }
        RpcActivateRoundEndScreen(numMoves, lenPath, netId, isServer);

    }

    [ ClientRpc ]
    public void RpcActivateRoundEndScreen(int numMoves, int lenPath, uint netId, bool isServer)
    {
        GameManager.Instance.activateRoundEndScreen(numMoves, lenPath , netId, isServer);
    }

    [ Command ]
    public void CmdRequestNewRound()
    {
        Debug.Log("fielding a request for a new round");

        // generate objective for the next round
        int finishingShipID = GameManager.Instance.randomGenerator.Next(1, GameManager.Instance.numShips);
        Vector3Int finishPoint = GameManager.Instance.generateFinishPoint(finishingShipID);

        // tell clients to start a new round with the winning gamestate from the previous round and a new objective
        RpcStartNewRound(GameManager.Instance.currWinningGamestate_flat, finishingShipID, finishPoint);

    }

    [ ClientRpc ]
    public void RpcStartNewRound(int[] roundStartGamestate_flat, int finishingShipID, Vector3Int finishPoint)
    {
        GameManager.Instance.startNextRound(unflattenGrid(roundStartGamestate_flat), finishingShipID, finishPoint);
    }

    // ===============================================================================================================
    //  DATA MANAGEMENT METHODS 
    // =============================================================================================================== 

    // takes the movesList and turns it into a string so it can be communicated over the network
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
                resGrid[(i*grid.GetLength(0) + j)] = grid[j, i];
            }

        }

        return resGrid;
    }

    // TODO MAKE THIS BE ABLE TO DO OTHER THAN 20 x 20 ALSO NOT JUST SQUARES
    // takes in a 1d array (likely from the network) and unflattens it into a 20 x 20 2d array 
    private int[,] unflattenGrid(int[] grid)
    {
        int[,] resGrid = new int[20, 20];
        for (int i = 0; i < 20; i++)
        {
            for (int j = 0; j < 20; j++)
            {   
                resGrid[j, i] = grid[i*20 + j];
            }
        }
    
        return resGrid;
    }
}
