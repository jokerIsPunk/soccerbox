
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace jokerispunk
{
    [DefaultExecutionOrder(1)] // after other scripts determine ownership
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class SBBall : UdonSharpBehaviour
    {
        [Header("Ball sound settings")]
        private AudioSource hitSound;
        public float minVelocitySound = 1.0f;
        private float minVelocitySndSqr = 1.0f;
        public float volumeSlope = 0.1f;
        public float pitchSlope = 0.05263158f;
        public float pitchOffset = 0.2f;

        [Space(10)]
        [Header("Motion sync smoothing")]
        [Tooltip("Minimum distance a ball in freefall must be out of sync before rollback")]
        public float minSyncDifference = 0.5f;
        private float minSyncDiffSqr = 0.25f;

        private Rigidbody rb;
        private float clock = 0f;
        private bool sleep = false;
        private float interval = 0.5f;
        private float sleepInterval = 5f;
        [UdonSynced] private Vector3 pos = Vector3.zero;
        [UdonSynced] private Vector3 vel = Vector3.zero;
        [UdonSynced] private Vector3 angVel = Vector3.zero;
        [UdonSynced] private bool isPollUpdate = false;

        private void Start()
        {
            // refs
            rb = (Rigidbody)GetComponent(typeof(Rigidbody));
            hitSound = (AudioSource)GetComponent(typeof(AudioSource));

            // math
            minSyncDiffSqr = Mathf.Pow(minSyncDifference, 2);
            minVelocitySndSqr = Mathf.Pow(minVelocitySound, 2);
        }

        private void Update()
        {
            // everybody runs this because it's cheaper to check time than to call Networking.IsOwner() every frame
            if (clock >= Time.time)
            {
                sleep = rb.IsSleeping();
                if (sleep)
                    clock = Time.time + sleepInterval;
                else
                {
                    isPollUpdate = true;
                    clock = Time.time + interval;
                    _SendData();
                }
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            // sound
            _BallSound(collision.relativeVelocity);

            // motion sync
            sleep = false;
            if (Networking.IsOwner(gameObject))
            {
                isPollUpdate = false;
                clock = Time.time + interval;
                _SendData();
            }
        }

        public void _SendData()
        {
            // only if you're the owner of a moving rigidbody
            if (!Networking.IsOwner(gameObject) || sleep) return;

            RequestSerialization();
        }

        public override void OnPreSerialization()
        {
            // update data
            pos = transform.position;
            vel = rb.velocity;
            angVel = rb.angularVelocity;

            // reset clock
            clock = Time.time + interval;
        }

        public override void OnDeserialization()
        {
            _ReceiveUpdate();
        }

        private void _ReceiveUpdate()
        {
            // tries to smooth paths through the air
            // if this update is only a periodic one during uninterrupted freefall (i.e. not called by collision), then presume the physics simulation to be faithful
            // and only roll back if the position desync is substantial
            if (isPollUpdate)
            {
                Vector3 stray = pos - transform.position;
                if (stray.sqrMagnitude < minSyncDiffSqr) return;
            }

            rb.velocity = vel;
            rb.angularVelocity = angVel;
            transform.position = pos;
        }

        private void _BallSound(Vector3 relativeVelocity)
        {
            if (hitSound == null) return;

            // skip sound if hit is below threshold
            // also skips expensive squrt calc for now
            float magnitude = relativeVelocity.sqrMagnitude;
            if (magnitude < minVelocitySndSqr) return;

            // calculate volume and pitch based on hit magnitude
            magnitude = Mathf.Sqrt(magnitude);

            // multiply volume by a coefficient to get a usable volume
            float volume = magnitude * volumeSlope;
            if (volume > 1.0f) volume = 1.0f;
            hitSound.volume = volume;

            // similarly multiply pitch, but restrict the slope to a smaller range for best effect
            float pitch = (magnitude * pitchSlope) + pitchOffset;
            if (pitch > 1.0f) pitch = 1.0f;
            hitSound.pitch = pitch;

            // play
            hitSound.Play();
        }
    }

}
