using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class clickndrag : MonoBehaviour
{
    private float startPosX;
    private float startPosY;
    private bool isBeingHeld = false;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()

    {
        if (isBeingHeld == true)
        {
            Vector3 mousePos;
            mousePos = Input.mousePosition;
            mousePos = Camera.main.ScreenToWorldPoint(mousePos);

            this.gameObject.transform.localPosition = new Vector3((float)Math.Floor(mousePos.x) + (float).5, (float)Math.Floor(mousePos.y) + (float).5);

        }

    }

    private void OnMouseDown(){

        if (Input.GetMouseButtonDown(0))
        {
                Vector3 mousePos;
                mousePos = Input.mousePosition;
                mousePos = Camera.main.ScreenToWorldPoint(mousePos);            
                isBeingHeld = true;
        }
        
    } 

    private void OnMouseUp(){

        isBeingHeld = false;

    }
}
