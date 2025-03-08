using System;
using UnityEngine;
using System.Collections;
using Photon.Pun;

public class Door : MonoBehaviourPun
{
    public float rotationSpeed = 2f;
    public float activationDistance = 2f;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isOpen = false;
    private bool isMoving = false;
    private Vector3 closedRotation;
    private Vector3 openRotation;

    private Transform nearestPlayer = null;

    private void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }

        closedRotation = transform.eulerAngles;
        openRotation = closedRotation + new Vector3(0, 100, 0);
    }

    private void Update()
    {
        nearestPlayer = FindNearestPlayer();
        if (nearestPlayer == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, nearestPlayer.position);

        if (Input.GetKeyDown(KeyCode.E) && distanceToPlayer <= activationDistance && !isMoving)
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

    private IEnumerator RotateDoor(Vector3 targetRotation)
    {
        isMoving = true;
        Vector3 startRotation = transform.eulerAngles;
        float time = 0f;

        while (time < 1f)
        {
            transform.eulerAngles = Vector3.Lerp(startRotation, targetRotation, time);
            time += Time.deltaTime * rotationSpeed;
            yield return null;
        }

        transform.eulerAngles = targetRotation;
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
