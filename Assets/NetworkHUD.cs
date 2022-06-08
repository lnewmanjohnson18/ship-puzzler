using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;


public class NetworkHUD : NetworkManagerHUD
{

    private MyNetworkManager NetworkManagerScript;
    private string networkAddressInput = "";

    void Awake()
    {
        NetworkManagerScript = this.gameObject.GetComponent<MyNetworkManager>();
    }

    // Start is called before the first frame update
    void Start()
    {   
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!(GameObject.Find("NetworkHUDUI") == null))
        {
            networkAddressInput = GameObject.Find("NetworkHUDUI").transform.GetChild(0).gameObject.GetComponent<TMP_InputField>().text;
        }
    }

    // have to override the OnGUI call from NetworkManagerHUD and Mirror does not provide an easy way of doing that
    void OnGUI()
    {
        // do nothing
    }

    public void startHostButtonCall()
    {
        NetworkManagerScript.StartHost();
        // NetworkClient.Ready();
        // if (NetworkClient.isConnected && !NetworkClient.ready)
        // {
        //     NetworkClient.Ready();
        //     if (NetworkClient.localPlayer == null)
        //     {
        //         NetworkClient.AddPlayer();
        //     }
        // }
        // if (NetworkClient.localPlayer == null)
        // {
        //     NetworkClient.AddPlayer();
        // }

    }

    public void connectToHostButtonCall()
    {
        NetworkManagerScript.networkAddress = networkAddressInput;

        NetworkManagerScript.StartClient();

    }    

}
