using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DarkRift.Client.Unity;
using DarkRift.Client;
using System;
using DarkRift;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The DarkRift client to communicate on.")]
    UnityClient client;

    [SerializeField]
    [Tooltip("The controllable player prefab.")]
    GameObject controllablePrefab;

    [SerializeField]
    [Tooltip("The network player prefab.")]
    GameObject networkPrefab;

    [SerializeField]
    [Tooltip("The network player manager.")]
    NetworkPlayerManager networkPlayerManager;

    [SerializeField]
    [Tooltip("The main camera.")]
    CameraFollow camera;

    void Awake()
    {
        if (client == null)
        {
            Debug.LogError("Client unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (controllablePrefab == null)
        {
            Debug.LogError("Controllable Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        if (networkPrefab == null)
        {
            Debug.LogError("Network Prefab unassigned in PlayerSpawner.");
            Application.Quit();
        }

        // Upon receiving a message from server, do as instructed in the void SpawnPlayer
        client.MessageReceived += MessageReceived;
    }

    void MessageReceived(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage() as Message)
        {
            if (message.Tag == Tags.SpawnPlayerTag)
                SpawnPlayer(sender, e);
            else if (message.Tag == Tags.DespawnPlayerTag)
                DespawnPlayer(sender, e);
        }
    }

    void SpawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        using (Message message = e.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                /* Commented out because our spawn packet length still varies at this point given that 
                  we haven't figured out what we want from it yet */

                //if (reader.Length % 13 != 0)
                //{
                //    Debug.LogWarning("Received malformed spawn packet.");
                //    return;
                //}

                // Reading the spawn packet
                while (reader.Position < reader.Length)
                {
                    print(reader.Position);
                    // Reading ID and position
                    ushort id = reader.ReadUInt16();
                    Vector3 position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), 0);

                    /* This is an additional step taken to make sure that the controllable sprite renders 
                     on top of network player ones. Not essential, just a cosmetic thing. */
                    if (id == client.ID)
                    {
                        position.z = -2;
                    }

                    // Reading player color
                    Color32 color = new Color32(
                        reader.ReadByte(),
                        reader.ReadByte(),
                        reader.ReadByte(),
                        255
                        );


                    GameObject obj;

                    // If the spawned player is the controllable player, spawn them as the controllable prefab
                    if (id == client.ID)
                    {
                        obj = Instantiate(controllablePrefab, position, Quaternion.identity) as GameObject;

                        // Tell the player's networking script where the client script is
                        PlayerNetworking playerNetworking = obj.GetComponent<PlayerNetworking>();
                        playerNetworking.Client = client;

                        // Command camera to follow player
                        camera.SetCameraTarget(obj.transform);
                    }

                    // Otherwise, spawn a network player
                    else
                    {
                        obj = Instantiate(networkPrefab, position, Quaternion.identity) as GameObject;
                    }

                    // Tell PlayerObject to set the sprite color to whatever it should be
                    PlayerObject playerObj = obj.GetComponent<PlayerObject>();
                    playerObj.SetColor(color);

                    // Add spawned player to players that networkPlayerManager has to track
                    networkPlayerManager.Add(id, playerObj);
                }
            }
        }
    }

    // Despawning player (doesn't work)
    void DespawnPlayer(object sender, MessageReceivedEventArgs e)
    {
        print("Player despawn");
        using (Message message = e.GetMessage())
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                networkPlayerManager.DestroyPlayer(reader.ReadUInt16());
            }
        }
    }
}
