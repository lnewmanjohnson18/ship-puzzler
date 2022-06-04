using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class MyNetworkManager : NetworkManager
{
    private NetworkConnection connectionToHost;

    public override void OnStartServer(){
        Debug.Log("Server Start");
    }

    public override void OnStopServer(){
        Debug.Log("Server Stopped");
    }

    public override void OnClientConnect(NetworkConnection conn){
        Debug.Log("Connected to Server");
        connectionToHost = conn;
        SceneManager.LoadScene("GameBoard");
    }

    public override void OnClientDisconnect(NetworkConnection conn){
        Debug.Log("Disconnect from Server");
    }

    void Update()
    {
    }
}
