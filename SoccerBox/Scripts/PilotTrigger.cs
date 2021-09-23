
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
        public SoccerBox sb;
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
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            _SetSoccerBoxState(false);
        }

        public void _SetSoccerBoxState(bool state)
        {
            if (loopCollider == null) { sb._UnexpectedFail(gameObject); return; }

            loopCollider.enabled = state;
            sb.loopRefs._SetCollidersState(state);

            // redo autocalibration on activate, but not if player is using manual calibration
            if (state && !sb.calibProgram.manualDone)
                sb.calibProgram._DoAutoCalib();
        }
    }
}
