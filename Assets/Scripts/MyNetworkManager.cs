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

