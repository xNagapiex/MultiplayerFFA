using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using DarkRift.Client;
using System;
using DarkRift;
using UnityEngine;

public class NetworkPlayerManager : MonoBehaviour
{

    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    // Contains the other players that the client should move around based on the position reports from the server
    Dictionary<ushort, PlayerObject> networkPlayers = new Dictionary<ushort, PlayerObject>();

    // Add a player to dictionary
    public void Add(ushort id, PlayerObject player)
    {
        networkPlayers.Add(id, player);
    }

    void Awake()
    {
        client.MessageReceived += NetworkMovement;
    }


    void NetworkMovement(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.MovePlayerTag)
            {

                using (DarkRiftReader reader = message.GetReader())
                {
                    print("Got movement update");

                    ushort id = reader.ReadUInt16();
                    Vector3 newPosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);
                    Vector3 newMousePosition = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);

                    networkPlayers[id].SetPosition(newPosition);
                    networkPlayers[id].SetMousePosition(newMousePosition);
                }

            }
        }
    }

    // Destroy disconnected player's gameobject and remove it from the player list
    public void DestroyPlayer(ushort id)
    {
        print("Destroying player " + id);
        PlayerObject o = networkPlayers[id];
        Destroy(o.gameObject);

        networkPlayers.Remove(id);
    }
}