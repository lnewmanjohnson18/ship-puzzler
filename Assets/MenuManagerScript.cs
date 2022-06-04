using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManagerScript : MonoBehaviour
{

    private Button demoGameButton;
    private GameObject canvas;

    public void loadSinglePlayer()
    {
        SceneManager.LoadScene("GameScene");
    }

    public void loadMultiplayer()
    {
        SceneManager.LoadScene("MultiplayerMenu");
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
