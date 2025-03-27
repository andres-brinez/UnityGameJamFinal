using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform followTarget;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float TopClamp = 70f;
    [SerializeField] private float BottomClamp = -40f;

    float cinemachineTargetYaw;
    float cinemachineTargetPitch;
    private void LateUpdate()
    {
        CameraLogic();
    }
    private void CameraLogic()
    {
        float mouseX = GetMouseInput("Mouse X");
        float mouseY = GetMouseInput("Mouse Y");

        cinemachineTargetPitch = UpdateRotation(cinemachineTargetPitch, mouseY, BottomClamp, TopClamp, true);
        cinemachineTargetYaw = UpdateRotation(cinemachineTargetYaw, mouseX, float.MinValue, float.MaxValue, false);
        ApplyRotations(cinemachineTargetPitch, cinemachineTargetYaw);
    }
    void ApplyRotations(float pitch, float yaw)
    {
        followTarget.rotation = Quaternion.Euler(pitch, yaw, followTarget.eulerAngles.z);
    }
    private float UpdateRotation(float currentRot, float input, float min, float max, bool isXAxis)
    {
        currentRot += isXAxis ? -input : input;
        return Mathf.Clamp(currentRot, min, max);
    }
    private float GetMouseInput(string axis)
    {
        return Input.GetAxis(axis) * rotationSpeed * Time.deltaTime;
    }
}
