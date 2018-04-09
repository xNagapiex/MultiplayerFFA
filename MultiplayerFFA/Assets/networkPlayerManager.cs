using DarkRift.Client.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class networkPlayerManager : MonoBehaviour {

        [SerializeField]
        [Tooltip("The DarkRift client to communicate on.")]
        UnityClient client;

        Dictionary<ushort, playerObject> networkPlayers = new Dictionary<ushort, playerObject>();

       public void Add(ushort id, playerObject player)
       {
           networkPlayers.Add(id, player);
       }
   }