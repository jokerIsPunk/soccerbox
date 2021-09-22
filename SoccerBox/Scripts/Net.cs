
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    public class Net : UdonSharpBehaviour
    {
        public SBConfig config;
        [Tooltip("When an object is caught in the net, its velocity is multiplied componentwise by this vector. All components should be between 0 and -1.")]
        public Vector3 netReflection = new Vector3(-0.2f, 0, -0.2f);
        [Space(10)]
        [Header("(do not change)")]
        public Transform respawnPoint;
        public Transform ball;

        private void OnTriggerExit(Collider other)
        {
            // check for a rigidbody
            Rigidbody otherRb = (Rigidbody)other.GetComponent(typeof(Rigidbody));
            if (otherRb == null) return;

            // filter for named objects, let everything else go
            string goName = other.gameObject.name;
            goName = goName.ToLower();
            bool catchFound = false;
            foreach (string str in config.catchNamesContaining)
                if (goName.Contains(str))
                { catchFound = true; break; }
            if (catchFound)
                _Catch(otherRb);
        }

        public void _Catch(Rigidbody rb)
        {
            // halt all angular velocity, and reduce and reflect velocity components
            rb.velocity = Vector3.Scale(rb.velocity, netReflection);
            rb.angularVelocity = Vector3.zero;
        }

        public void _Respawn(Transform tf)
        {
            if (respawnPoint == null) { config._UnexpectedFail(gameObject); return; }

            Networking.SetOwner(Networking.LocalPlayer, tf.gameObject);
            tf.position = respawnPoint.position;
        }

        // special method callable without parameters, for UI buttons
        public void _RespawnBall()
        {
            if (ball == null) { config._UnexpectedFail(gameObject); return; }

            _Respawn(ball);                
        }
    }
}
