using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class backgroundGridScript : MonoBehaviour
{

    public bool debugging = true;
    private Tilemap tilemap;

    // Start is called before the first frame update
    void Start()
    {
    this.tilemap = this.gameObject.GetComponent<Tilemap>();
        
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetMouseButtonDown(0)) && (this.debugging == true)){
            Debug.Log("coords: " + this.tilemap.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition)).ToString());
        }
    }

}