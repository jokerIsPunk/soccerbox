
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
        public SBConfig config;
        private Transform sbObj;

        // collider refs
        public Rigidbody rFootColl, lFootColl, rKneeColl, lKneeColl, headColl;
        public Transform chestColl;

        // bone and tracking point proxy refs
        public Transform rFootPivot, lFootPivot, headPivot;

        // child transforms of bone and tracking proxies; used for non-Udon space offset calculation
        public Transform rFootTarget, lFootTarget, headTarget;

        private VRCPlayerApi localPlayer;
        private float maxCollStray = 0.6f;
        private float maxCollStraySqr;


        private void Start()
        {
            // init and refs
            localPlayer = Networking.LocalPlayer;
            maxCollStraySqr = Mathf.Pow(maxCollStray, 2);
        }

        public override void OnPlayerTriggerStay(VRCPlayerApi player)
        {
            if (!player.isLocal) return;

            // update offsetter transforms
            VRCPlayerApi.TrackingData headTracking = localPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.Head);
            headPivot.SetPositionAndRotation(headTracking.position, headTracking.rotation);
            rFootPivot.SetPositionAndRotation(localPlayer.GetBonePosition(HumanBodyBones.RightFoot), localPlayer.GetBoneRotation(HumanBodyBones.RightFoot));
            lFootPivot.SetPositionAndRotation(localPlayer.GetBonePosition(HumanBodyBones.LeftFoot), localPlayer.GetBoneRotation(HumanBodyBones.LeftFoot));

            // chest special case
            chestColl.position = localPlayer.GetBonePosition(HumanBodyBones.Chest);

            // update velocities!
            _VelocityFollow(rFootTarget.position, rFootColl);
            _VelocityFollow(lFootTarget.position, lFootColl);
            _VelocityFollow(headTarget.position, headColl);
            _VelocityFollow(localPlayer.GetBonePosition(HumanBodyBones.RightLowerLeg), rKneeColl);
            _VelocityFollow(localPlayer.GetBonePosition(HumanBodyBones.LeftLowerLeg), lKneeColl);
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

        // a secondary net; this one just teleports the ball to the play area center
        private void OnTriggerExit(Collider other)
        {
            // owner only
            if (!Networking.IsOwner(other.gameObject)) return;

            // filter for named objects, let everything else go
            string goName = other.gameObject.name;
            goName = goName.ToLower();

            foreach (string str in config.catchNamesContaining)
                if (goName.Contains(str))
                {
                    //other.transform.position = transform.position;
                    config.net._Respawn(other.transform);
                    break;
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
