
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using jokerispunk;

namespace jokerispunk
{
    public class BodyCollider : UdonSharpBehaviour
    {
        public SBConfig config;
        private Collider thisCollider;

        private void Start()
        {
            // get implicit refs
            thisCollider = (Collider)GetComponent(typeof(Collider));
        }

        private void OnCollisionEnter(Collision collision)
        {
            // ignore all non-rigidbodies; a smart layer setup would be better but VRChat does not make that easy
            if (collision.rigidbody == null)
            {
                Physics.IgnoreCollision(thisCollider, collision.collider);
                return;
            }

            // take ownership of the object before any physics is applied
            _CheckOwnership(collision.collider);
        }

        private void OnTriggerEnter(Collider other)
        {
            // chest is special case because making it a flat surface would be difficult, so I'm emulating the physics with a trigger instead
            // any body collider set as a trigger will simply stall the rigidbody
            if (!thisCollider.isTrigger) return;

            _CheckOwnership(other);

            if (other.attachedRigidbody != null)
            {
                other.attachedRigidbody.velocity = Vector3.zero;
                other.attachedRigidbody.angularVelocity = Vector3.zero;
            }
            else Physics.IgnoreCollision(thisCollider, other);
        }

        public void _CheckOwnership(Collider other)
        {
            if (Networking.IsOwner(other.gameObject)) return;

            // take ownership of objects the player touches; filter for ball if configured
            if (!config.ballsOnly)
            {
                Debug.Log(string.Format("[SoccerBox] Collision with {0}, taking ownership of {1}...", gameObject.name, other.name));
                Networking.SetOwner(Networking.LocalPlayer, other.gameObject);
            }
            else
            {
                string goName = other.gameObject.name;
                goName = goName.ToLower();
                if (goName.Contains("ball"))
                {
                    Debug.Log(string.Format("[SoccerBox] Collision with {0}, taking ownership of {1}...", gameObject.name, other.name));
                    Networking.SetOwner(Networking.LocalPlayer, other.gameObject);
                }
                else
                    Physics.IgnoreCollision(thisCollider, other);
            }
        }
    }
}
