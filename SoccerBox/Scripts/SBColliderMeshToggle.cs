
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

namespace jokerispunk
{
    // toggle the meshes of the body colliders; for calibration and curiosity
    public class SBColliderMeshToggle : UdonSharpBehaviour
    {
        public Toggle toggle;
        public SoccerBox sb;
        private MeshRenderer[] meshes;

        void Start()
        {
            // create a MeshRenderer array and populate it with the meshes from the array of body collider gameobjects
            // should prbably be an editor script but meh
            int colliderCount = sb.bodyCollidersGO.Length;
            meshes = new MeshRenderer[colliderCount];
            for (int i = 0; i < colliderCount; i++)
            {
                if (sb.bodyCollidersGO[i] == null) { sb._UnexpectedFail(gameObject); continue; }

                MeshRenderer mesh = (MeshRenderer)sb.bodyCollidersGO[i].GetComponent(typeof(MeshRenderer));
                meshes[i] = mesh;
            }

            // keep meshes visible in editor and disable at runtime
            // I want to users to see the meshes because that makes it obvious that scaling the whole prefab is a mistake
            _SetMeshState(false);
        }

        public void _SetMeshState(bool state)
        {
            foreach (MeshRenderer mesh in meshes)
            {
                if (mesh == null) { sb._UnexpectedFail(gameObject); continue; }

                mesh.enabled = state;
            }
        }

        // paramater-less methods for calling via UI buttons
        public void _DisableMeshes()
            { _SetMeshState(false); }

        public void _EnableMeshes()
            { _SetMeshState(true); }

        public void _ToggleMeshes()
        { _SetMeshState(toggle.isOn); }
    }
}
