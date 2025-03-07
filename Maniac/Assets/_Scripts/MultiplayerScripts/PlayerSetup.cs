using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class PlayerSetup : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public GameObject playerCamera;
    public GameObject playerCanvas;
    
    public void IsLocalPlayer()
    {
        playerMovement.enabled = true;
        playerCamera.gameObject.SetActive(true);
        playerCanvas.SetActive(true);
    }
}
