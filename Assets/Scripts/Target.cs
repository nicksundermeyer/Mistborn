using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour {
    public Triggerable triggerResult;
    public float minimumVelocity = 20f;

    private void OnCollisionEnter (Collision other) {
        if (other.relativeVelocity.magnitude > minimumVelocity) {
            triggerResult.Triggered ();
        }
    }
}