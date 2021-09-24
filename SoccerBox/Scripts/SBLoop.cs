
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    public class SBLoop : UdonSharpBehaviour
    {
        // config and gameobject refs
        public SoccerBox sb;

        // collider refs
        public Rigidbody rFootColl, lFootColl, rKneeColl, lKneeColl, headColl;
        public Transform chestColl;

        // child transforms of bone and tracking proxies; used for non-Udon space offset calculation
        public Transform rFootTarget, lFootTarget, headTarget;

        private VRCPlayerApi lp;
        private float maxCollStray = 0.6f;
        private float maxCollStraySqr = 0f;


        private void Start()
        {
            // init and refs
            lp = Networking.LocalPlayer;
            maxCollStraySqr = Mathf.Pow(maxCollStray, 2);
        }

        private void FixedUpdate()
        {
            // update offsetter pivot transforms
            VRCPlayerApi.TrackingData headTracking = lp.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            headTarget.parent.SetPositionAndRotation(headTracking.position, headTracking.rotation);
            rFootTarget.parent.SetPositionAndRotation(lp.GetBonePosition(HumanBodyBones.RightFoot), lp.GetBoneRotation(HumanBodyBones.RightFoot));
            lFootTarget.parent.SetPositionAndRotation(lp.GetBonePosition(HumanBodyBones.LeftFoot), lp.GetBoneRotation(HumanBodyBones.LeftFoot));

            // chest special case
            chestColl.position = lp.GetBonePosition(HumanBodyBones.Chest);

            // update velocities!
            _VelocityFollow(rFootTarget.position, rFootColl);
            _VelocityFollow(lFootTarget.position, lFootColl);
            _VelocityFollow(headTarget.position, headColl);
            _VelocityFollow(lp.GetBonePosition(HumanBodyBones.RightLowerLeg), rKneeColl);
            _VelocityFollow(lp.GetBonePosition(HumanBodyBones.LeftLowerLeg), lKneeColl);
        }

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

        // called from other scripts as part of enabling/disabling the whole body collider system
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
