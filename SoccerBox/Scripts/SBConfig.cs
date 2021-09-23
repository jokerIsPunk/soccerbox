
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    public class SBConfig : UdonSharpBehaviour
    {
        [Space(20)]
        [Header("Note: Do NOT scale this parent object. Scale child object \"Play Area\" instead.")]
        [Space(20)]
        [Header("User configuration options:")]
        [Tooltip("The net will catch any GameObject whose name contains any of these words.")]
        public string[] catchNamesContaining = { "ball" };
        [Tooltip("Check this to ignore player body collisions with objects that don't contain the string \"ball\" in their name.")]
        public bool ballsOnly = false;

        private void Start()
        {
            // convert filter strings to lower case
            for (int i = 0; i < catchNamesContaining.Length; i++)
                catchNamesContaining[i] = catchNamesContaining[i].ToLower();
        }
    }
}
