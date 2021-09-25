
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    public class PilotTrigger : UdonSharpBehaviour
    {
        [Space(20)]
        [Header("(do not change)")]
        public SoccerBox sb;
        public UdonBehaviour loopUB;

        void Start()
        {
            // determine starting state of loop and objects
            bool initState = _DetermineInitState();
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
            if (loopUB == null) { sb._UnexpectedFail(gameObject); return; }

            // body collider update loop
            loopUB.enabled = state;

            // enable body collider GOs
            sb.loopRefs._SetCollidersState(state);

            // redo autocalibration on activate, but not if player is using manual calibration
            if (state && !sb.calibProgram.manualDone)
                sb.calibProgram._DoAutoCalib();
        }

        private bool _DetermineInitState()
        {
            // if the play area is null, disabled, or missing a collider then infer that the creator wants the loop to always run
            if (sb.playArea == null) return true;
            else
                if (!sb.playArea.gameObject.activeSelf) return true;

            Collider thisCollider = (Collider)GetComponent(typeof(Collider));
            if (thisCollider == null) return true;

            // if the play area is valid and enabled, then determine whether player has spawned inside or outside
            Vector3 posAtStart = Networking.LocalPlayer.GetPosition() + Vector3.up;
            return thisCollider.bounds.Contains(posAtStart);
        }
    }
}
