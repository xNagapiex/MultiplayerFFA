using DarkRift.Client.Unity;
using DarkRift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 13.4.2018 Taru Konttinen
// This script sends the player's status to server. To be attached to controllable player only.

public class PlayerNetworking : MonoBehaviour
{
    const byte MOVEMENT_TAG = 1;
    const ushort MOVE_SUBJECT = 0;

    [SerializeField]
    [Tooltip("The distance we can move before we send a position update.")]
    float moveDistance = 0.05f;

    Vector3 mousePosition;
    Vector3 movement;

    // Insert Client script of the Network object in scene here
    public UnityClient Client { get; set; }

    Vector3 lastPosition;
    Vector3 lastMousePosition;

    void Awake()
    {
        lastPosition = transform.position;
        lastMousePosition = Vector3.down;
        mousePosition = Vector3.down;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, lastPosition) > moveDistance || Vector3.Distance(mousePosition, lastMousePosition) > moveDistance)
        {
            // Sending player status to server (position, mouse position)
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(transform.position.x);
                writer.Write(transform.position.y);

                writer.Write(mousePosition.x);
                writer.Write(mousePosition.y);

                using (Message message = Message.Create(Tags.MovePlayerTag, writer))
                    Client.SendMessage(message, SendMode.Unreliable);
            }

            lastPosition = transform.position;
            lastMousePosition = mousePosition;
        }
    }

    // Receive mouse position info
    internal void SetMousePosition(Vector3 newMousePos)
    {
        mousePosition = newMousePos;
    }
}