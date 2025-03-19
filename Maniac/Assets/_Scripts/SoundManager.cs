using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SoundManager : MonoBehaviourPun
{
    public static SoundManager instance;

    public AudioSource musicSource; // (Backsound)
    public AudioSource sfxSource;   // (SFX)

    public AudioClip backsound;
    public AudioClip doorSound;
    public AudioClip lightSound;
    public AudioClip murderHitSound;
    public AudioClip trapCloseSound;
    public AudioClip trapCollectSound;
    public AudioClip trapPlaceSound;

    private PhotonView photonView;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        photonView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        if (PhotonNetwork.InRoom)
            photonView.RPC("RPC_PlayBacksound", RpcTarget.AllBuffered, (float)PhotonNetwork.Time);
        else PlayBacksound();

    }

    [PunRPC]
    private void RPC_PlayBacksound(float startTime)
    {
        Debug.Log("Backsound RPC received!");

        if (musicSource == null)
        {
            Debug.LogError("musicSource is NULL!");
            return;
        }

        if (backsound == null)
        {
            Debug.LogError("Backsound clip is NULL!");
            return;
        }

        PlayBacksound(startTime);
    }

    private void PlayBacksound(float startTime = 0.0f)
    {
        if (!musicSource.isPlaying)
        {
            musicSource.clip = backsound;
            musicSource.loop = true;
            musicSource.time = (float)(PhotonNetwork.Time - startTime) % backsound.length;
            musicSource.Play();
            Debug.Log("Backsound started at time: " + musicSource.time);
        }
    }

    public void PlayDoorSound()
    {
        photonView.RPC("RPC_PlayDoorSound", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayDoorSound()
    {
        sfxSource.PlayOneShot(doorSound);
    }

    public void PlayLightSound()
    {
        photonView.RPC("RPC_PlayLightSound", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayLightSound()
    {
        sfxSource.PlayOneShot(lightSound);
    }

    public void PlayMurderHitSound()
    {
        photonView.RPC("RPC_PlayMurderHitSound", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayMurderHitSound()
    {
        sfxSource.PlayOneShot(murderHitSound);
    }

    public void PlayTrapCloseSound()
    {
        photonView.RPC("RPC_PlayTrapCloseSound", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayTrapCloseSound()
    {
        sfxSource.PlayOneShot(trapCloseSound);
    }

    public void PlayTrapCollectSound()
    {
        photonView.RPC("RPC_PlayTrapCollectSound", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayTrapCollectSound()
    {
        sfxSource.PlayOneShot(trapCollectSound);
    }

    public void PlayTrapPlaceSound()
    {
        photonView.RPC("RPC_PlayTrapPlaceSound", RpcTarget.All);
    }

    [PunRPC]
    private void RPC_PlayTrapPlaceSound()
    {
        sfxSource.PlayOneShot(trapPlaceSound);
    }
}
