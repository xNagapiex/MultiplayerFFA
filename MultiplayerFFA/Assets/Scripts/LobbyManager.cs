using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class LobbyManager : MonoBehaviour {

    [SerializeField]
    [Tooltip("LobbyPrefab 1.")]
    public GameObject LobbyPrefab1;

    [SerializeField]
    [Tooltip("LobbyPrefab 2.")]
    public GameObject LobbyPrefab2;

    [SerializeField]
    [Tooltip("LobbyPrefab 3.")]
    public GameObject LobbyPrefab3;

    [SerializeField]
    [Tooltip("LobbyPrefab 4.")]
    public GameObject LobbyPrefab4;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void NewPlayerJoined(Color color, int playerCount)
    {
        //look through untaken prefab's sprites, change color to color if carrot or hatmid, white otherwise.
        //enable start button when player count is at least 2
    }
}
