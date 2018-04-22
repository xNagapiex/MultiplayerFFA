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
                ushort cID1 = 0;
                int count1 = 0;
                ushort cID2 = 0;
                int count2 = 0;
                ushort cID3 = 0;
                int count3 = 0;

                foreach (KeyValuePair<ushort, int> player in players)
                {
                    if (player.Value == 2)
                    {
                        cID1 = player.Key;
                        count1 = player.Value - 1;
                    }

                    else if (player.Value == 3)
                    {
                        cID2 = player.Key;
                        count2 = player.Value - 1;
                    }

                    else if (player.Value == 4)
                    {
                        cID3 = player.Key;
                        count3 = player.Value - 1;
                    }
                }

                players.Clear();
                players.Add(cID1, count1);
                players.Add(cID2, count2);
                players.Add(cID3, count3);

                if (players.ContainsValue(1))
                {
                    tempColor = LobbyPlayer2.transform.GetChild(1).GetComponent<SpriteRenderer>().color;
                    LobbyPlayer1.transform.GetChild(1).GetComponent<SpriteRenderer>().color = tempColor;
                    LobbyPlayer1.transform.GetChild(3).GetComponent<SpriteRenderer>().color = tempColor;

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

            else if (disconnectedPlayer == 2)
            {
                ushort cID2 = 0;
                int count2 = 0;
                ushort cID3 = 0;
                int count3 = 0;

                foreach (KeyValuePair<ushort, int> player in players)
                {

                    if (player.Value == 3)
                    {
                        cID2 = player.Key;
                        count2 = player.Value - 1;
                    }

                    else if (player.Value == 4)
                    {
                        cID3 = player.Key;
                        count3 = player.Value - 1;
                    }
                }

                players.Remove(cID2);
                players.Remove(cID3);
                players.Add(cID2, count2);
                players.Add(cID3, count3);

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
                ushort cID3 = 0;
                int count3 = 0;

                foreach (KeyValuePair<ushort, int> player in players)
                {
                    if (player.Value == 4)
                    {
                        cID3 = player.Key;
                        count3 = player.Value - 1;
                    }
                }

                players.Remove(cID3);
                players.Add(cID3, count3);

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
