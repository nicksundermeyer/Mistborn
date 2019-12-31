using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : Triggerable {
    public override void Triggered () {
        Vector3 rot = transform.localEulerAngles;
        rot.x += 90;
        transform.localRotation = Quaternion.Euler (rot);
    }
}