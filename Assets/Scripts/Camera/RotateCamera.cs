using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateCamera : MonoBehaviour
{
    public AudioPeer audioPeer;
    public Vector3 rotateAxis, rotateSpeed;

    // Update is called once per frame
    void Update()
    {
        transform.GetChild(0).transform.LookAt(transform);
        transform.Rotate(rotateAxis.x * rotateSpeed.x * Time.deltaTime * audioPeer.GetAmplitudeBuffer(),
                         rotateAxis.y * rotateSpeed.y * Time.deltaTime * audioPeer.GetAmplitudeBuffer(),
                         rotateAxis.z * rotateSpeed.z * Time.deltaTime * audioPeer.GetAmplitudeBuffer());
    }
}
