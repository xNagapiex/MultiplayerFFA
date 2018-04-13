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

    // Initiating variables
    void Awake()
    {
    }

    // Gathering player input and sending it forward to the PlayerObject of this object which turns it into action
    void Update()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition = Camera.main.ScreenToWorldPoint(mousePosition);

        playerObject.SetMousePosition(mousePosition);
        playerNetworking.SetMousePosition(mousePosition);
    }
}
