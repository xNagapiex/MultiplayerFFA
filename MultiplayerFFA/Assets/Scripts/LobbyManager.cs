using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift;
using DarkRift.Client;
using DarkRift.Client.Unity;

public class LobbyManager : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Dictionary<ushort, int> players = new Dictionary<ushort, int>();

    public GameObject startButton;
    int playerCount = 0;
    int disconnectedPlayer;

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

    public void NewPlayerJoined(Color color, ushort clientID)
    {
        playerCount++;
        addPlayerToLibrary(clientID, playerCount);

        //look through untaken prefab's sprites, change color to 'color' if carrot or hatmid, white otherwise.
        if (playerCount == 1)
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

        if(playerCount == 2)
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

        if(playerCount == 3)
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

        if(playerCount == 4)
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
        
        //enable start button when player count is at least 
        if (playerCount >= 2)
        {
            startButton.gameObject.SetActive(true);
        }
    }

    public void addPlayerToLibrary(ushort clientID, int playerCount)
    {
        players.Add(clientID, playerCount);
    }

    public void UpdateLobbyPlayer(ushort clientID)
    {
        playerCount--;
        disconnectedPlayer = players[clientID];
        players.Remove(clientID);

        print("Player disconnected: " + disconnectedPlayer);

        Color tempColor;

        if (disconnectedPlayer <= playerCount)
        {
            if (disconnectedPlayer == 1)
            {
                foreach (KeyValuePair<ushort, int> player in players)
                {
                        ushort tempClientID = player.Key;
                        int tempPlayerCount = player.Value;
                        players.Remove(tempClientID);
                        --tempPlayerCount;
                        players.Add(tempClientID, tempPlayerCount);
                }

                if (players.ContainsValue(2))
                {
                    tempColor = LobbyPlayer2.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                    LobbyPlayer1.transform.GetChild(1).GetComponent<SpriteRenderer>().color = tempColor;
                    LobbyPlayer1.transform.GetChild(3).GetComponent<SpriteRenderer>().color = tempColor;

                }

                if (players.ContainsValue(3))
                {
                    tempColor = LobbyPlayer3.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                    LobbyPlayer2.transform.GetChild(1).GetComponent<SpriteRenderer>().color = tempColor;
                    LobbyPlayer2.transform.GetChild(3).GetComponent<SpriteRenderer>().color = tempColor;
                }

                if (players.ContainsValue(4))
                {
                    tempColor = LobbyPlayer4.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                    LobbyPlayer3.transform.GetChild(1).GetComponent<SpriteRenderer>().color = tempColor;
                    LobbyPlayer3.transform.GetChild(3).GetComponent<SpriteRenderer>().color = tempColor;
                }
            }
            else if (disconnectedPlayer == 2)
            {
                foreach (KeyValuePair<ushort, int> player in players)
                {
                    if (player.Value > 2)
                    {
                        ushort tempClientID = player.Key;
                        int tempPlayerCount = player.Value;
                        players.Remove(tempClientID);
                        --tempPlayerCount;
                        players.Add(tempClientID, tempPlayerCount);
                    }
                }

                if (players.ContainsValue(2))
                {
                    tempColor = LobbyPlayer3.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                    LobbyPlayer2.transform.GetChild(1).GetComponent<SpriteRenderer>().color = tempColor;
                    LobbyPlayer2.transform.GetChild(3).GetComponent<SpriteRenderer>().color = tempColor;
                }

                if (players.ContainsValue(3))
                {
                    tempColor = LobbyPlayer4.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                    LobbyPlayer3.transform.GetChild(1).GetComponent<SpriteRenderer>().color = tempColor;
                    LobbyPlayer3.transform.GetChild(3).GetComponent<SpriteRenderer>().color = tempColor;
                }

            }
            else if (disconnectedPlayer == 3)
            {
                    foreach (KeyValuePair<ushort, int> player in players)
                    {
                        if (player.Value < 3)
                        {
                            ushort tempClientID = player.Key;
                            int tempPlayerCount = player.Value;
                            players.Remove(tempClientID);
                            --tempPlayerCount;
                            players.Add(tempClientID, tempPlayerCount);
                        }
                    }
                if (players.ContainsValue(3))
                {
                    tempColor = LobbyPlayer4.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                    LobbyPlayer3.transform.GetChild(1).GetComponent<SpriteRenderer>().color = tempColor;
                    LobbyPlayer3.transform.GetChild(3).GetComponent<SpriteRenderer>().color = tempColor;
                }
            }

        }
        else
        {
            if (disconnectedPlayer == 1)
            {
                for (int i = 0; i < 4; i++)
                {
                    LobbyPlayer1.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.black;
                }
            }

            else if (disconnectedPlayer == 2)
            {
                for (int i = 0; i < 4; i++)
                {
                    LobbyPlayer2.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.black;
                }
            }

            else if (disconnectedPlayer == 3)
            {
                for (int i = 0; i < 4; i++)
                {
                    LobbyPlayer3.transform.GetChild(i).GetComponent<SpriteRenderer>().color = Color.black;
                }
            }

            else if (disconnectedPlayer == 4)
            {
                for (int i = 0; i < 4; i++)
                {
                    LobbyPlayer4.transform.GetComponent<SpriteRenderer>().color = Color.black;
                }
            }
        }

        //enable start button when player count is at least 
        if (playerCount < 2)
        {
            startButton.gameObject.SetActive(false);
        }
    }
}
