using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    private NetworkConnection connectionToHost;
    // public NetworkConnection connectionToClient;
    public List<NetworkConnectionToClient> clientConnections = new List<NetworkConnectionToClient>();

    public override void OnStartServer()
    {
        Debug.Log("Server Start");
    }

    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        clientConnections.Add(conn);
        // connectionToClient = conn;
    }

    public override void OnStopServer()
    {
        Debug.Log("Server Stopped");
    }

    public override void OnClientConnect()
    {
        Debug.Log("Connected to Server");
        SceneManager.LoadScene("GameBoard");

        if (SceneManager.GetActiveScene().buildIndex != 2)
        {
            // starts a side routine that will wait for the board to load then spawn the player
            StartCoroutine("waitForBoardLoad", 2);
        }
    }

    IEnumerator waitForBoardLoad(int sceneNumber)
    {
        while (SceneManager.GetActiveScene().buildIndex != sceneNumber)
        {
            yield return null;
        }

        if (SceneManager.GetActiveScene().buildIndex == sceneNumber)
        {
            NetworkClient.Ready();
            if (NetworkClient.localPlayer == null)
            {
                NetworkClient.AddPlayer();
            }
        }

    }

    public override void OnClientDisconnect()
    {
        Debug.Log("Disconnect from Server");
    }

    public List<NetworkConnectionToClient> getClientConnections()
    {
        //TODO: add a check in here to make sure that no one has disconnected
        // or do it in OnServerDisconnect?
        return clientConnections;
    }
}

