
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    // this class contains all the logic for keeping the balls and other objects inside
    // some different types of boundaries
    public class Net : UdonSharpBehaviour
    {
        [Tooltip("Checking this skips the physics script and just teleports the ball to the respawn point. Use it for the outer backup net.")]
        public bool teleport = false;
        [Tooltip("When an object is caught in the net, its velocity is multiplied componentwise by this vector. All components should be between 0 and -1.")]
        public Vector3 netReflection = new Vector3(-0.2f, 0, -0.2f);
        [Space(10)]
        [Header("(do not change)")]
        public Transform respawnPoint;
        public Transform ball;
        public SoccerBox sb;

        private void OnTriggerExit(Collider other)
        {
            // owner only
            if (!Networking.IsOwner(other.gameObject)) return;

            // check for a rigidbody
            Rigidbody otherRb = (Rigidbody)other.GetComponent(typeof(Rigidbody));
            if (otherRb == null) return;

            // filter for named objects, let everything else go
            string goName = other.gameObject.name;
            goName = goName.ToLower();
            bool catchFound = false;

            foreach (string str in sb.userConfig.catchNamesContaining)
                if (goName.Contains(str))
                { catchFound = true; break; }

            if (catchFound)
            {
                if (teleport) _Respawn(other.transform);
                else _Catch(otherRb);
            }
        }

        public void _Catch(Rigidbody rb)
        {
            // halt all angular velocity, and reduce and reflect velocity components
            rb.velocity = Vector3.Scale(rb.velocity, netReflection);
            rb.angularVelocity = Vector3.zero;
        }

        public void _Respawn(Transform tf)
        {
            if (respawnPoint == null) { sb._UnexpectedFail(gameObject); return; }

            Networking.SetOwner(Networking.LocalPlayer, tf.gameObject);
            tf.position = respawnPoint.position;
        }

        // special method callable without parameters, for UI buttons
        public void _RespawnBall()
        {
            if (ball == null) { sb._UnexpectedFail(gameObject); return; }

            _Respawn(ball);                
        }
    }
}
