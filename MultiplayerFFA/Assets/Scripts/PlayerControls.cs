using DarkRift;
using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 13.4.2018 Taru Konttinen
// This script belongs to the controllable player and nothing else. It is used to gather player input and command PlayerObject based on that

public class PlayerControls : MonoBehaviour
{
    // The PlayerObject script of this gameobject
    public PlayerObject playerObject;
    public PlayerNetworking playerNetworking;
    public float speed; // Change to private eventually

    Vector3 lastPosition;
    Vector3 lastMousePosition;

    // Initiating variables
    void Awake()
    {
        lastPosition = transform.position;
        lastMousePosition = Input.mousePosition;
        lastMousePosition = Camera.main.ScreenToWorldPoint(lastMousePosition);
    }

    // Gathering player input and sending it forward to the PlayerObject of this object which turns it into action
    void Update()
    {
        float hAxis = Input.GetAxis("Horizontal");
        float vAxis = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(hAxis, vAxis, 0) * speed * Time.deltaTime;

        playerObject.SetMovement(movement);

        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        playerObject.SetMousePosition(mousePosition);
        playerNetworking.SetMousePosition(mousePosition);
    }
}
