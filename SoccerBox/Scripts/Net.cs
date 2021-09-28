
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.SDK3.Components;

namespace jokerispunk
{
    // this class contains all the logic for moving the balls around
    // logic and references are used by subordinate class, SBNetCollider, which is the immediate handling of OnTriggerExit events
    public class Net : UdonSharpBehaviour
    {
        [Tooltip("When an object is caught in a physics net (inner), its velocity is multiplied componentwise by this vector. All components should be between 0 and -1.")]
        public Vector3 netReflection = new Vector3(-0.2f, 0, -0.2f);
        [Space(10)]
        [Header("(do not change)")]
        public Transform respawnPoint;
        public Transform respawnableBall;
        public SoccerBox sb;

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
            // for synced objects this will interpolate noticeably
            // I could add a bunch of logic to the NetCollider script to try and detect if ObjectSync is attached
            // but that's a lot of extra code and work, and nobody cares. WON'T FIX
        }

        // special method for the ball; allows calling from UI button
        public void _RespawnBall()
        {
            if (respawnableBall == null) { sb._UnexpectedFail(gameObject); return; }

            _Respawn(respawnableBall);
        }
    }
}
