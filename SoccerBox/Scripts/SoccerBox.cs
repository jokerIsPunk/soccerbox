
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    // this class holds shared methods and references for the prefab
    public class SoccerBox : UdonSharpBehaviour
    {
        [Space(20)]
        [Header("(do not change below)")]
        public SBConfig userConfig;
        public Transform playArea;
        public Net net;
        public SBLoop loopRefs;
        public FootColliderCalibration calibProgram;
        public GameObject[] bodyCollidersGO;

        void Start()
        {
            // if user ignored the warning and scaled the top-level object, try to unbreak it
            if (playArea.parent.localScale != Vector3.one)
            {
                Debug.LogWarning("[SoccerBox] You scaled the top-level prefab GameObject. I specifically told you not to do that. Attempting runtime fix (also with an attitude)...");
                playArea.localScale = Vector3.Scale(playArea.localScale, playArea.parent.localScale);
                playArea.parent.localScale = Vector3.one;
            }

            // body colliders ignore collisions between themselves; otherwise they keep calling tons of ownership transfers
            _IgnoreSelfCollisions();
        }

        private void _IgnoreSelfCollisions()
        {
            // start with the GameObject array of body colliders
            // build an analogous Collider array, and populate it with the Collider component from each gameobject
            int colliderCount = bodyCollidersGO.Length;
            Collider[] bodyCollidersColl = new Collider[colliderCount];
            for (int i = 0; i < colliderCount; i++)
            {
                if (bodyCollidersGO[i] == null) { _UnexpectedFail(gameObject); continue; }

                Collider coll = (Collider)bodyCollidersGO[i].GetComponent(typeof(Collider));
                bodyCollidersColl[i] = coll;
            }

            // iterate through the new Collider array to generate every combination of two
            // send those combinations as arguments to Physics.IgnoreCollision
            for (int i = 0; i < bodyCollidersColl.Length; i++)
            {
                if (bodyCollidersColl[i] == null) { _UnexpectedFail(gameObject); continue; }
                #region rubber_duck0
                // each element of the array ignores every element subsequent to it
                // as each element is reached, it is already ignoring every element prior to it
                // the second-to-last element has only one element to ignore: the last element
                //      at that point i is at length - 2 (i.e. highest range index - 1); j is at length - 1 (i.e. highest range index)
                //      thus the loop executes that final IgnoreCollision call since both While conditions are met
                // the last element has no elements to ignore
                //      at that point i is at length - 1 (i.e. highest range index); j is at length - 0 (i.e. out of range)
                //      thus the inner loop does not execute because the condition is not met
                #endregion rubber_duck0
                for (int j = i + 1; j < bodyCollidersGO.Length; j++)
                {
                    if (bodyCollidersColl[j] == null) continue;
                    Debug.Log(string.Format("[SoccerBox] Ignoring collisions beween {0} and {1}...", bodyCollidersColl[i].name, bodyCollidersColl[j].name));
                    Physics.IgnoreCollision(bodyCollidersColl[i], bodyCollidersColl[j]);
                }
            }
        }

        public void _UnexpectedFail(GameObject context)
        {
            Debug.LogWarning(string.Format("[SoccerBox] Unexpected error on gameobject {0}! Is a reference broken on one of the Udon scripts?", context.name), context);
        }
    }

}
