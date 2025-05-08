using Photon.Pun;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Scripts
{
    public class SoundManager : MonoBehaviourPun
    {
        public static SoundManager Instance;

        [FormerlySerializedAs("musicSource")] public AudioSource GlobalMusicSource; // (Backsound)
        [FormerlySerializedAs("sfxSource")] public AudioSource GlobalSfxSource; // (SFX)

        public AudioClip backgroundSound;
        public AudioClip doorActionSound;
        public AudioClip lightActionSound;
        public AudioClip murderHitSound;
        public AudioClip trapActionSound;
        public AudioClip trapCollectingSound;
        public AudioClip trapPlacingSound;
        public AudioClip shotSound;
        
        public AudioClip backButtonPressedSound;
        public AudioClip backgroundSound2;
        public AudioClip gameStartSound;
        public AudioClip gameOverSound;
        
        public AudioClip walkingSound;

        private PhotonView _photonView;

        private float musicVolume = 0.2f;
        private float soundVolume = 0.2f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }
            _photonView = GetComponent<PhotonView>();

            if (PlayerPrefs.HasKey("MusicVolume"))
            {
                musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            }
            else
            {
                PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            }
            if (PlayerPrefs.HasKey("SoundVolume"))
            {
                soundVolume = PlayerPrefs.GetFloat("SoundVolume");
            }
            else
            {
                PlayerPrefs.SetFloat("SoundVolume", soundVolume);
            }
            
            GlobalMusicSource.volume = musicVolume;
            GlobalSfxSource.volume = soundVolume;
            
        }

        private void Start()
        {
            if (PhotonNetwork.InRoom)
                _photonView.RPC("RPC_PlayBacksound", RpcTarget.AllBuffered, (float)PhotonNetwork.Time);
            else PlayBacksound();
        }

        // Background sound
        [PunRPC]
        private void RPC_PlayBacksound(float startTime)
        {
            Debug.Log("Backsound RPC received!");

            if (GlobalMusicSource == null)
            {
                Debug.LogError("musicSource is NULL!");
                return;
            }

            if (backgroundSound == null)
            {
                Debug.LogError("Backsound clip is NULL!");
                return;
            }

            PlayBacksound(startTime);
        }

        private void PlayBacksound(float startTime = 0.0f)
        {
            if (!GlobalMusicSource.isPlaying)
            {
                GlobalMusicSource.clip = backgroundSound;
                GlobalMusicSource.loop = true;
                GlobalMusicSource.time = (float)(PhotonNetwork.Time - startTime) % backgroundSound.length;
                GlobalMusicSource.Play();
                Debug.Log("Backsound started at time: " + GlobalMusicSource.time);
            }
        }
        // doorActionSound
        public void PlayDoorSound() => _photonView.RPC("RPC_PlayDoorSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayDoorSound() => GlobalSfxSource.PlayOneShot(doorActionSound);

        // lightActionSound
        public void PlayLightSound() => _photonView.RPC("RPC_PlayLightSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayLightSound() => GlobalSfxSource.PlayOneShot(lightActionSound);
        
        // murderHitSound
        public void PlayMurderHitSound() => _photonView.RPC("RPC_PlayMurderHitSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayMurderHitSound() => GlobalSfxSource.PlayOneShot(murderHitSound);
        
        // trapActionSound
        public void PlayTrapActionSound() => _photonView.RPC("RPC_PlayTrapActionSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayTrapActionSound() => GlobalSfxSource.PlayOneShot(trapActionSound);
        
        // trapCollectingSound
        public void PlayTrapCollectingSound() => _photonView.RPC("RPC_trapCollectingSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayTrapCollectingSound() => GlobalSfxSource.PlayOneShot(trapCollectingSound);
        
        // trapPlacingSound
        public void PlayTrapPlacingSound() => _photonView.RPC("RPC_PlayTrapPlacingSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayTrapPlacingSound() => GlobalSfxSource.PlayOneShot(trapPlacingSound);
        
        // shotSound
        public void PlayShotSound()
        {
            _photonView.RPC("RPC_PlayShotSound", RpcTarget.OthersBuffered);
            GlobalSfxSource.PlayOneShot(shotSound);
        }
        [PunRPC] private void RPC_PlayShotSound() => GlobalSfxSource.PlayOneShot(shotSound);
        
        // backButtonPressedSound
        public void PlayBackButtonPressedSound() => GlobalSfxSource.PlayOneShot(backButtonPressedSound);
        
        // gameStartSound
        public void PlayGameStartSound() => _photonView.RPC("RPC_PlayGameStartSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayGameStartSound() => GlobalSfxSource.PlayOneShot(gameStartSound);
        
        // gameOverSound
        public void PlayGameOverSound() => _photonView.RPC("RPC_PlayGameOverSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayGameOverSound() => GlobalSfxSource.PlayOneShot(gameOverSound);
        
        // walkingSound
        public void PlayWalkingSound() => _photonView.RPC("RPC_PlayWalkingSound", RpcTarget.All);
        [PunRPC] private void RPC_PlayWalkingSound() => GlobalSfxSource.PlayOneShot(walkingSound);
        
        
    }
}
