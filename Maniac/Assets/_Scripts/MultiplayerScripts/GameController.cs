using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Scripts.MultiplayerScripts.LobbyScripts;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace _Scripts.MultiplayerScripts
{
    public class GameController : MonoBehaviourPunCallbacks
    {
        private const int SECONDS_TO_ON_LIGHTS = 60;
        private const int SECONDS_TO_TURN_OFF_ON_LIGHTS = 220;

        private PhotonView _photonView;

        private List<Light> _lightsOnScene = new List<Light>();
        private readonly List<float> _defaultLightsIntensityValue = new List<float>();
        
        private ParticleSystem _rainParticleSystem;
        private AudioSource _audioSource;

        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
        }

        private void Start()
        {
            _rainParticleSystem = GameObject.FindGameObjectWithTag("Rain").GetComponent<ParticleSystem>();
            _audioSource = _rainParticleSystem.GetComponent<AudioSource>();
            _rainParticleSystem.Stop();
            _audioSource.Stop();
            
            StartCoroutine(CallRandomFunctionLoop());
            FindLightsOnScene();
        }

        [PunRPC]
        public void TurnAllLightsOffRPC()
        {
            foreach (var light in _lightsOnScene)
            {
                if (light == null) continue;
                light.intensity = 0.0f;
            }
            _rainParticleSystem.Play();
            _audioSource.Play();
        }

        [PunRPC]
        public void TurnAllLightsOnRPC()
        {
            for (int i = 0; i < _lightsOnScene.Count; i++)
            {
                if (_lightsOnScene[i] == null) continue;
                _lightsOnScene[i].intensity = _defaultLightsIntensityValue[i];
            }
            _rainParticleSystem.Stop();
            _audioSource.Stop();
        }

        [PunRPC]
        public void FlickerLightsRPC()
        {
            StartCoroutine(FlickerRoutine());
        }

        private IEnumerator FlickerRoutine()
        {
            float flickerDuration = 3f;
            float timer = 0f;

            while (timer < flickerDuration)
            {
                foreach (var light in _lightsOnScene)
                {
                    if (light == null) continue;

                    light.intensity = Random.value > 0.5f ? 0f : _defaultLightsIntensityValue[_lightsOnScene.IndexOf(light)];
                }

                float interval = Random.Range(0.05f, 0.3f);
                timer += interval;
                yield return new WaitForSeconds(interval);
            }

            //TurnAllLightsOffRPC();
            _photonView.RPC("TurnAllLightsOffRPC", RpcTarget.AllBuffered);
        }

        private IEnumerator CallRandomFunctionLoop()
        {
            while (true)
            {
                float randomDelay = Random.Range(0f, SECONDS_TO_TURN_OFF_ON_LIGHTS); 
                yield return new WaitForSeconds(randomDelay);

                _photonView.RPC("FlickerLightsRPC", RpcTarget.AllBuffered);
                yield return new WaitForSeconds(SECONDS_TO_ON_LIGHTS);
                _photonView.RPC("TurnAllLightsOnRPC", RpcTarget.AllBuffered);

                float remainingTime = SECONDS_TO_TURN_OFF_ON_LIGHTS - randomDelay;
                yield return new WaitForSeconds(remainingTime);
            }
        }

        private void FindLightsOnScene()
        {
            _lightsOnScene = GameObject.FindObjectsOfType<Light>().ToList();

            foreach (var light in _lightsOnScene)
            {
                _defaultLightsIntensityValue.Add(light.intensity);
            }
        }
    }
}
