using System;
using Photon.Pun;
using UnityEngine;

public class PrintFootsteps : MonoBehaviourPun
{
    [Header("Footstep Settings")]
    [SerializeField] private string leftFootstepPrefabName = "LeftFootstep";
    [SerializeField] private string rightFootstepPrefabName = "RightFootstep";
    [SerializeField] private float footstepYOffset = 0.01f;

    private bool isLeftStep = true;

    public void FootstepPrinting()
    {
        if (!GetComponentInParent<PhotonView>().IsMine) return;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 2f))
        {
            Vector3 position = hit.point + Vector3.up * footstepYOffset;

            Vector3 forward = transform.forward;
            forward.y = 0;
            Quaternion rotation = Quaternion.LookRotation(forward) * Quaternion.Euler(90f, 11f, 0f);
            
            string prefabName = isLeftStep ? leftFootstepPrefabName : rightFootstepPrefabName;
            isLeftStep = !isLeftStep;

            PhotonNetwork.Instantiate(prefabName, position, rotation);
        }
    }

}