using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

public class PlayerCameraSetUp : MonoBehaviourPun
{
    [Header("Camera Height Option")]
    public float cameraHeightChangeSpeed = 3f;
    public float standCameraHeight = 1.55f;
    public float crouchCameraHeight = 1.35f;
    public float proneCameraHeight = 0.8f;

    private CinemachineVirtualCamera tpsCinemachineVirtualCamera;
    private PlayerMovement playerMovement;
    private CinemachineFramingTransposer cinemachineFramingTransposer;

    void Start()
    {
        // 카메라 초기설정
        playerMovement = GetComponent<PlayerMovement>();
        cinemachineFramingTransposer = Camera.main.GetComponent<CameraControl>().tpsCinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        tpsCinemachineVirtualCamera = Camera.main.GetComponent<CameraControl>().tpsCinemachineVirtualCamera;

        if (photonView.IsMine)
        {
            tpsCinemachineVirtualCamera.Follow = transform; // tps 가상 카메라의 추적 대상을 자신의 트랜스폼으로 설정
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        TPSCameraHeightSetUp();
    }

    // 플레이어 자세에 따라 카메라 고도를 조절하는 메서드
    private void TPSCameraHeightSetUp()
    {
        if (playerMovement.isStanding)
        {
            cinemachineFramingTransposer.m_TrackedObjectOffset.y = Mathf.Lerp(cinemachineFramingTransposer.m_TrackedObjectOffset.y, standCameraHeight, Time.deltaTime * cameraHeightChangeSpeed);
        }
        else if (playerMovement.isCrouching)
        {
            cinemachineFramingTransposer.m_TrackedObjectOffset.y = Mathf.Lerp(cinemachineFramingTransposer.m_TrackedObjectOffset.y, crouchCameraHeight, Time.deltaTime * cameraHeightChangeSpeed);
        }
        else if (playerMovement.isProning)
        {
            cinemachineFramingTransposer.m_TrackedObjectOffset.y = Mathf.Lerp(cinemachineFramingTransposer.m_TrackedObjectOffset.y, proneCameraHeight, Time.deltaTime * cameraHeightChangeSpeed);
        }
    }

}
