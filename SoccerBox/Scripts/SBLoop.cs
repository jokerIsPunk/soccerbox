
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using jokerispunk;

namespace jokerispunk
{
    public class SBLoop : UdonSharpBehaviour
    {
        // config and gameobject refs
        public SoccerBox sb;

        // collider refs
        public Rigidbody rFootColl, lFootColl, rKneeColl, lKneeColl, headColl;
        public Transform chestColl;

        // bone and tracking point proxy refs
        //public Transform rFootPivot, lFootPivot, headPivot;

        // child transforms of bone and tracking proxies; used for non-Udon space offset calculation
        public Transform rFootTarget, lFootTarget, headTarget;

        private VRCPlayerApi localPlayer;
        private float maxCollStray = 0.6f;
        private float maxCollStraySqr = 0f;


        private void Start()
        {
            // init and refs
            localPlayer = Networking.LocalPlayer;
            maxCollStraySqr = Mathf.Pow(maxCollStray, 2);
        }

        private void FixedUpdate()
        {
            // update offsetter pivot transforms
            VRCPlayerApi.TrackingData headTracking = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            headTarget.parent.SetPositionAndRotation(headTracking.position, headTracking.rotation);
            rFootTarget.parent.SetPositionAndRotation(localPlayer.GetBonePosition(HumanBodyBones.RightFoot), localPlayer.GetBoneRotation(HumanBodyBones.RightFoot));
            lFootTarget.parent.SetPositionAndRotation(localPlayer.GetBonePosition(HumanBodyBones.LeftFoot), localPlayer.GetBoneRotation(HumanBodyBones.LeftFoot));

            // chest special case
            chestColl.position = localPlayer.GetBonePosition(HumanBodyBones.Chest);

            // update velocities!
            _VelocityFollow(rFootTarget.position, rFootColl);
            _VelocityFollow(lFootTarget.position, lFootColl);
            _VelocityFollow(headTarget.position, headColl);
            _VelocityFollow(localPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg), rKneeColl);
            _VelocityFollow(localPlayer.GetBonePosition(HumanBodyBones.LeftLowerLeg), lKneeColl);
        }

        //public override void OnPlayerTriggerStay(VRCPlayerApi player)
        //{
        //    if (!player.isLocal) return;


        //}

        public void _VelocityFollow(Vector3 target, Rigidbody rb)
        {
            // get the difference in position from rb to target
            Vector3 delta = target - rb.transform.position;

            // if bounds are alright, set the rb velocity to cover that distance over the time of one frame
            // but if the stray is too far, just jump the position
            if (delta.sqrMagnitude <= maxCollStraySqr)
            {
                rb.velocity = delta / Time.deltaTime;
            }
            else
            {
                rb.transform.position = target;
                Debug.Log("[SoccerBox] Body collider out of bounds! Resetting..");
            }
        }

        public void _SetCollidersState(bool state)
        {
            rFootColl.gameObject.SetActive(state);
            lFootColl.gameObject.SetActive(state);
            rKneeColl.gameObject.SetActive(state);
            lKneeColl.gameObject.SetActive(state);
            headColl.gameObject.SetActive(state);
            chestColl.gameObject.SetActive(state);
        }
    }
}
