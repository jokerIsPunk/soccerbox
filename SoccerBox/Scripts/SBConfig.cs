
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    // execute after default world settings in order to override jump impulse
    [DefaultExecutionOrder(1)]
    public class SBConfig : UdonSharpBehaviour
    {
        [Space(20)]
        [Header("Note: Do NOT scale this parent object. Scale child object \"Play Area\" instead.")]
        [Space(20)]
        [Header("User configuration options:")]
        [Tooltip("Set the gravity acceleration (meters/s) for all physical objects and players. 5.0 m/s is recommended.")]
        [Range(5.0f, 9.81f)] public float gravityAccel = 5.0f;
        [Tooltip("The net will catch any GameObject whose name contains any of these words. Additionally, those objects cannot be on the Pickup layer (use Walkthrough instead).")]
        public string[] catchNamesContaining = { "ball" };
        [Tooltip("Check this to ignore player body collisions with objects that don't contain the string \"ball\" in their name.")]
        public bool ballsOnly = false;

        private void Start()
        {
            // convert filter strings to lower case
            for (int i = 0; i < catchNamesContaining.Length; i++)
                catchNamesContaining[i] = catchNamesContaining[i].ToLower();

            // gravity
            _ConvertGravity();
        }

        private void _ConvertGravity()
        {
            // get reference to original gravity; assume the horizontal components are 0
            float origGrav = Physics.gravity.y;

            // construct and apply configured gravity
            Physics.gravity = new Vector3(0f, (-gravityAccel), 0f);

            // decrease jump impulse with gravity decrease so jump height is stabilized
            // but weight the calculation towards original jump impulse for better effect
            VRCPlayerApi lp = Networking.LocalPlayer;
            float jumpImpluse = lp.GetJumpImpulse();
            float jumpProportion = ((-gravityAccel + origGrav) / 2) / origGrav;
            lp.SetJumpImpulse(jumpImpluse * jumpProportion);
        }
    }
}
