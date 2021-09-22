
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using jokerispunk;

namespace jokerispunk
{
    public class PilotTrigger : UdonSharpBehaviour
    {
        [Space(20)]
        [Header("This architecture enables the update loop trigger only if the local player is within. It thereby skips unnecessary calls of OnPlayerTriggerStay for non-local players.")]
        [Space(20)]
        [Header("(do not change)")]
        public SBConfig config;
        public Collider loopCollider;

        private Collider thisCollider;

        void Start()
        {
            // determine whether player has spawned inside the prefab
            Vector3 posAtStart = Networking.LocalPlayer.GetPosition() + Vector3.up;
            thisCollider = (Collider)GetComponent(typeof(Collider));
            bool initState = thisCollider.bounds.Contains(posAtStart);

            // set start state accordingly
            _SetSoccerBoxState(initState);
        }

        public override void OnPlayerTriggerEnter(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            _SetSoccerBoxState(true);

            // redo autocalibration, but not if player is using manual calibration
            if (!config.calibProgram.manualDone)
                config.calibProgram._DoAutoCalib();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            _SetSoccerBoxState(false);
        }

        public void _SetSoccerBoxState(bool state)
        {
            if (loopCollider == null) { config._UnexpectedFail(gameObject); return; }

            loopCollider.enabled = state;
            config.loopRefs._SetCollidersState(state);
        }
    }
}
