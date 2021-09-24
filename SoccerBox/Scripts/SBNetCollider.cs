
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    // immediate handling of OnTriggerExit
    // establishes two modes, physics and teleport
    // depends on logic and references from the Net class and SBConfig
    public class SBNetCollider : UdonSharpBehaviour
    {
        [Tooltip("Checking this skips the physics script and just teleports the ball to the respawn point. Use it for the outer backup net.")]
        public bool teleport = false;
        public SoccerBox sb;
        private Collider thisCollider;

        private void Start()
        {
            // get implicit ref
            thisCollider = (Collider)GetComponent(typeof(Collider));
        }

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
                if (teleport) sb.net._Respawn(other.transform);
                else sb.net._Catch(otherRb);
            }
            else Physics.IgnoreCollision(thisCollider, other);
        }
    }
}
