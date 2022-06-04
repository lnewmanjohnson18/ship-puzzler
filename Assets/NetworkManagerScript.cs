using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkManagerScript : MonoBehaviour
{

    void Awake()
    {
        // tells the engine to persist the NetworkManager when scenes are changed
        DontDestroyOnLoad(this.gameObject);


        // this code was included above the previous line in the docs page i took this from but i dont understand why yet
        // GameObject[] objs = GameObject.FindGameObjectsWithTag("music");

        // if (objs.Length > 1)
        // {
        //     Destroy(this.gameObject);
        // }


    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
