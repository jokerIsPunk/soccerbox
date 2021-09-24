
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    // this class contains logic for placing the foot colliders near the middle of the player's foot
    // this is a challenge because player avatars' bone position, rotation, existence varies wildly
    public class FootColliderCalibration : UdonSharpBehaviour
    {
        [Space(10)]
        [Header("AutoCalibraion config")]
        [Tooltip("Distance away from the knee that the foot collider should move. Starting location of foot collider is actually the ankle.")]
        public float offsetLengthDistal = 0.05f;
        [Tooltip("Distance toward toes that the foot collider should move. Ignored if player's avatar has no toes.")]
        public float offsetLengthAnterior = 0.07f;
        [Tooltip("Time to wait after player loads in to try foot collider auto-calibration. Increase this if foot colliders are wrong on spawn and players take a long time to load in.")]
        public float autoCalibrationDelay = 3f;

        [Space(10)]
        [Header("(do not change)")]
        public Transform manualCalPoint;
        public SoccerBox sb;

        private VRCPlayerApi lp;
        [HideInInspector] public bool manualDone = false;

        void Start()
        {
            lp = Networking.LocalPlayer;
            
            // delay autocalibration for long enough for avatar bones to load
            SendCustomEventDelayedSeconds("_DoAutoCalib", autoCalibrationDelay);
        }

        public void _DoAutoCalib()
        {
            _AutoCalibrateLeg(sb.loopRefs.rFootTarget, true);
            _AutoCalibrateLeg(sb.loopRefs.lFootTarget, false);
        }

        public void _ClearCalibration()
        {
            sb.loopRefs.rFootTarget.localPosition = Vector3.zero;
            sb.loopRefs.lFootTarget.localPosition = Vector3.zero;
        }

        // autocalibration makes educated guesses about where the middle of the player's foot should be
        public void _AutoCalibrateLeg(Transform tf, bool isRLeg)
        {
            // determine bones to be used
            HumanBodyBones footBone;
            HumanBodyBones kneeBone;
            HumanBodyBones toeBone;
            if (isRLeg) {
                footBone = HumanBodyBones.RightFoot;
                kneeBone = HumanBodyBones.RightLowerLeg;
                toeBone = HumanBodyBones.RightToes; }
            else {
                footBone = HumanBodyBones.LeftFoot;
                kneeBone = HumanBodyBones.LeftLowerLeg;
                toeBone = HumanBodyBones.LeftToes; }

            // move the offset target's PARENT pivot to the position and orientation the update loop will be using during play
            Vector3 footPos = lp.GetBonePosition(footBone);
            tf.parent.SetPositionAndRotation(footPos, lp.GetBoneRotation(footBone));

            // find the vector from knee to foot; this defines the downward direction from the foot (i.e. distal)
            Vector3 kneePos = lp.GetBonePosition(kneeBone);
            Vector3 distal = footPos - kneePos;
            distal = distal.normalized;

            // find the point in world space that is offsetLength in the distal direction from the foot
            Vector3 distalPos = footPos + (distal * offsetLengthDistal);

            // move the offsetter target to that position
            // this establishes a relative position to its parent that will thereafter be maintained via inheritance as the local position
            tf.position = distalPos;
            Debug.Log(string.Format("[SoccerBox] [AutoCalib] Moved collider offset distally, displacement {0}", tf.localPosition));

            // does this leg have toes? if so, move anterially for a little more accuracy
            // but don't move TO the toes! you don't kick a soccer ball with your toes, you kick with the middle of your foot
            Vector3 toePos = lp.GetBonePosition(toeBone);
            if (toePos != Vector3.zero)
            {
                Debug.Log("[SoccerBox] [AutoCalib] Found toes, moving collider anterially...");
                Vector3 anterior = toePos - distalPos;
                anterior = anterior.normalized;
                Vector3 anteriorPos = distalPos + (anterior * offsetLengthAnterior);
                tf.position = anteriorPos;
            }
            else
                Debug.Log("[SoccerBox] [AutoCalib] No toes detected!", gameObject);
        }

        // uses a fixed world space point indicated to the player to establish the offset from the foot bone
        public void _ManualCalib()
        {
            // determine which foot is closest to the reference point and infer that that's the foot the player is trying to calibrate
            Vector3 referencePoint = manualCalPoint.position;
            Vector3 rFootPos = lp.GetBonePosition(HumanBodyBones.RightFoot);
            Vector3 lFootPos = lp.GetBonePosition(HumanBodyBones.LeftFoot);
            float rDistSqr = Vector3.SqrMagnitude(referencePoint - rFootPos);
            float lDistSqr = Vector3.SqrMagnitude(referencePoint - lFootPos);
            bool isRFoot = rDistSqr < lDistSqr;

            Transform tf;
            HumanBodyBones footBone;
            if (isRFoot) {
                tf = sb.loopRefs.rFootTarget;
                footBone = HumanBodyBones.RightFoot; }
            else {
                tf = sb.loopRefs.lFootTarget;
                footBone = HumanBodyBones.LeftFoot; }

            // move the offset target's PARENT pivot to the position and orientation the update loop will be using during play
            Vector3 footPos;
            if (isRFoot) footPos = rFootPos;
            else footPos = lFootPos;
            tf.parent.SetPositionAndRotation(footPos, lp.GetBoneRotation(footBone));

            // move the offsetter target to the reference point
            // this establishes a relative position to its parent that will be maintained via parenting as the local position
            tf.position = referencePoint;
            Debug.Log(string.Format("[SoccerBox] [ManualCalib] Displacement {0}", tf.localPosition));

            // flag to skip autocalibration thereafter
            manualDone = true;
        }
    }
}
