using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 13.4.2018 Taru Konttinen
// This script should be on both the controllable player and network player. It is used to process commands from PlayerControls or NetworkPlayerManager.

public class PlayerObject : MonoBehaviour
{    
    Vector3 mousePosition;
    Vector3 targetPos;
    bool network;

    // Initiating some variables
    void Awake()
    {
        mousePosition = Vector3.down;

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
        Quaternion targetRot;
        targetRot = Quaternion.Euler(0, 0, Mathf.Atan2(mousePosition.y - transform.position.y, mousePosition.x - transform.position.x) * Mathf.Rad2Deg - 90);

        if (!network)
        {
            if (Vector3.Distance(transform.position, mousePosition) > 8.2f)
            {
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(mousePosition.x, mousePosition.y, transform.position.z), Time.deltaTime * 4.5f);

                if (transform.position.x >= 20)
                {
                    transform.position = new Vector3(20, transform.position.y, transform.position.z);
                }

                if (transform.position.x <= -20)
                {
                    transform.position = new Vector3(-20, transform.position.y, transform.position.z);
                }

                if (transform.position.y >= 20)
                {
                    transform.position = new Vector3(transform.position.x, 20, transform.position.z);
                }

                if (transform.position.y <= -20)
                {
                    transform.position = new Vector3(transform.position.x, -20, transform.position.z);
                }
            }

            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.time * 2);
        }

        else
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Mathf.SmoothStep(0.0f, 1.0f, 0.1f));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Mathf.SmoothStep(0.0f, 1.0f, 0.1f));
        }
    }

    // Receiving the player color and applying it to hat and carrot
    internal void SetColor(Color32 color)
    {
        SpriteRenderer playerCarrot = transform.GetChild(0).GetComponent<SpriteRenderer>();
        SpriteRenderer playerHatLine = transform.GetChild(1).GetComponent<SpriteRenderer>();
        playerCarrot.color = color;
        playerHatLine.color = color;
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
