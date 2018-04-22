using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour {

    GameObject network;

	// Use this for initialization
	void Start () {

        network = GameObject.Find("Network");
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    // Goes to Lobby
    public void PlayButton()
    {
        SceneManager.LoadScene("Lobby");
    }

    // Quits
    public void QuitButton()
    {
        Application.Quit();
    }

    // Goes back to menu
    public void ToMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    // Starts game
    public void StartGame()
    {
        if (network != null)
        {
            network.GetComponent<PlayerSpawner>().StartGame();
        }
    }
}
