using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class LobbyManager : MonoBehaviour
{
    SpriteRenderer spriteRenderer;

    [SerializeField]
    [Tooltip("LobbyPlayer 1.")]
    public GameObject LobbyPlayer1;

    [SerializeField]
    [Tooltip("LobbyPlayer 2.")]
    public GameObject LobbyPlayer2;

    [SerializeField]
    [Tooltip("LobbyPlayer 3.")]
    public GameObject LobbyPlayer3;

    [SerializeField]
    [Tooltip("LobbyPlayer 4.")]
    public GameObject LobbyPlayer4;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        
    }

    public void NewPlayerJoined(Color color, int id)
    {

        if(id == 0)
        {

                for (int i = 0; i < 4; i++)
                {
                    if (i == 1 || i == 3)
                    {
                        LobbyPlayer1.transform.GetChild(i).GetComponent<SpriteRenderer>().color = color;
                    }
                    else
                    {
                        LobbyPlayer1.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
                    }
                }
        }

        if(id == 1)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 1 || i == 3)
                {
                    LobbyPlayer2.transform.GetChild(i).GetComponent<SpriteRenderer>().color = color;
                }
                else
                {
                    LobbyPlayer2.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }

        if(id == 2)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 1 || i == 3)
                {
                    LobbyPlayer3.transform.GetChild(i).GetComponent<SpriteRenderer>().color = color;
                }
                else
                {
                    LobbyPlayer3.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }

        if(id == 3)
        {
            for (int i = 0; i < 4; i++)
            {
                if (i == 1 || i == 3)
                {
                    LobbyPlayer4.transform.GetChild(i).GetComponent<SpriteRenderer>().color = color;
                }
                else
                {
                    LobbyPlayer4.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.white;
                }
            }
        }
        //look through untaken prefab's sprites, change color to color if carrot or hatmid, white otherwise.
        //enable start button when player count is at least 2
    }
}
