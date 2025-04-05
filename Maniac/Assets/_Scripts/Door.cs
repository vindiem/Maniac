using System;
using UnityEngine;
using System.Collections;
using _Scripts;
using Photon.Pun;

public class Door : MonoBehaviourPun
{
    public float rotationSpeed = 2f;
    public float activationDistance = 2f;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isOpen = false;
    private bool isMoving = false;
    
    private Quaternion closedRotation;
    private Quaternion openRotation;

    private Transform nearestPlayer = null;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }

        closedRotation = transform.rotation;
        openRotation = Quaternion.AngleAxis(100, Vector3.up) * closedRotation;
    }


    private void Update()
    {
        nearestPlayer = FindNearestPlayer();
        if (nearestPlayer == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, nearestPlayer.position);

        if (Input.GetKeyDown(KeyCode.E) && distanceToPlayer <= activationDistance 
                                        && !isMoving && objectRenderer.material.color == Color.green)
        {
            photonView.RPC("ToggleDoor", RpcTarget.AllBuffered, !isOpen);
        }
    }

    [PunRPC]
    private void ToggleDoor(bool open)
    {
        if (isMoving) return;

        isOpen = open;
        StartCoroutine(RotateDoor(isOpen ? openRotation : closedRotation));
    }

    private IEnumerator RotateDoor(Quaternion targetRotation)
    {
        isMoving = true;
        Quaternion startRotation = transform.rotation;
        float time = 0f;
        
        // Making sound
        SoundManager.Instance.PlayDoorSound();

        while (time < 1f)
        {
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, time);
            time += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        transform.rotation = targetRotation;
        isMoving = false;
    }

    private void OnMouseEnter()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = Color.green;
        }
    }

    private void OnMouseExit()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
    }

    private Transform FindNearestPlayer()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        Transform nearest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = player.transform;
            }
        }

        return nearest;
    }
}
