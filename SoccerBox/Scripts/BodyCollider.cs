
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using jokerispunk;

namespace jokerispunk
{
    // this class does two things:
    // 1. handles network ownership consequences of body interaction with synced objects
    // 2. does a little physics
    public class BodyCollider : UdonSharpBehaviour
    {
        public SoccerBox sb;
        private Collider thisCollider;

        private void Start()
        {
            // get implicit refs
            thisCollider = (Collider)GetComponent(typeof(Collider));
        }

        private void OnCollisionEnter(Collision collision)
        {
            bool checksPassed = _Checks(collision.rigidbody, collision.gameObject.name);
            if (!checksPassed)
            {
                Physics.IgnoreCollision(thisCollider, collision.collider);
                return;
            }
            
            // take ownership of the object before any physics is applied (via physics engine later in frame)
            _ChangeOwnership(collision.gameObject);
        }

        // chest is special case because making it a flat surface would be difficult, so I'm emulating the physics with a trigger instead
        // any body collider set as a trigger will simply stall the rigidbody
        private void OnTriggerEnter(Collider other)
        {
            bool checksPassed = _Checks(other.attachedRigidbody, other.name);
            if (!checksPassed)
            {
                Physics.IgnoreCollision(thisCollider, other);
                return;
            }

            // take ownership of the object before any physics is applied
            _ChangeOwnership(other.gameObject);

            // instead of physics, on triggers it's just this physics-like script
            other.attachedRigidbody.velocity = Vector3.zero;
            other.attachedRigidbody.angularVelocity = Vector3.zero;
        }

        private bool _Checks(Rigidbody rb, string name)
        {
            // ignore all non-rigidbodies
            if (rb == null) return false;

            // skip all non-ball objects if configured
            if (sb.userConfig.ballsOnly)
            {
                name = name.ToLower();
                if (!name.Contains("ball")) return false;
            }

            return true;
        }

        public void _ChangeOwnership(GameObject go)
        {
            if (Networking.IsOwner(go)) return;

            // take ownership of objects the player touches
            Debug.Log(string.Format("[SoccerBox] Collision with {0}, taking ownership of {1}...", gameObject.name, go.name));
            Networking.SetOwner(Networking.LocalPlayer, go);
        }
    }
}
