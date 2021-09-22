
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using jokerispunk;

namespace jokerispunk
{
    public class PilotTrigger : UdonSharpBehaviour
    {
        public SBLoop loopRefs;
        public Collider loopCollider;
        public FootColliderCalibration calibProgram;

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
            if (!calibProgram.manualDone)
                calibProgram._DoAutoCalib();
        }

        public override void OnPlayerTriggerExit(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            _SetSoccerBoxState(false);
        }

        public void _SetSoccerBoxState(bool state)
        {
            loopRefs._SetCollidersState(state);
            loopCollider.enabled = state;
        }
    }
}
