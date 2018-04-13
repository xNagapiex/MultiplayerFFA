using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 13.4.2018 Taru Konttinen
// This script should be on both the controllable player and network player. It is used to process commands from PlayerControls or NetworkPlayerManager.

public class PlayerObject : MonoBehaviour
{    
    private Rigidbody2D rb;
    Vector3 mousePosition;
    Vector3 movement;
    Vector3 targetPos;
    bool network;

    // Initiating some variables
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mousePosition = Vector3.down;
        movement = Vector3.zero;

        if (GetComponent<PlayerControls>())
        {
            network = false;
        }
        else
        {
            network = true;
        }
    }

    void Update()
    {
        if (!network)
        {
            rb.MovePosition(transform.position + movement);
        }

        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, 0.4f);
        }

        Quaternion targetRot;
        targetRot = Quaternion.Euler(0, 0, Mathf.Atan2(mousePosition.y - transform.position.y, mousePosition.x - transform.position.x) * Mathf.Rad2Deg - 90);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.time * 1);
    }

    // Receiving the player color and applying it to hat and carrot
    internal void SetColor(Color32 color)
    {
        SpriteRenderer playerCarrot = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer playerHatLine = transform.GetChild(1).GetComponent<SpriteRenderer>();
        playerCarrot.color = color;
        playerHatLine.color = color;
    }

    // Receive the movement info and apply it in update
    internal void SetMovement(Vector3 newMovement)
    {
        movement = newMovement;
    }

    // Receive mousePos info and use it to rotate the player in udpate
    internal void SetMousePosition(Vector3 newMousePos)
    {
        mousePosition = newMousePos;
    }
    
    internal void SetPosition(Vector3 target)
    {
        targetPos = target;
    }
}
