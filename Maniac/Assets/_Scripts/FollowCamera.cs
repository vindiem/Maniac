using UnityEngine;

namespace _Scripts
{
    public class FollowCamera : MonoBehaviour
    {
        public Transform cameraPosition;

        private void Update()
        {
            gameObject.transform.position = cameraPosition.position;
            gameObject.transform.rotation = cameraPosition.rotation;
        }
    }
}
