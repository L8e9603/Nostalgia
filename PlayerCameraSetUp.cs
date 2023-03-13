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
        // ī�޶� �ʱ⼳��
        playerMovement = GetComponent<PlayerMovement>();
        cinemachineFramingTransposer = Camera.main.GetComponent<CameraControl>().tpsCinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        tpsCinemachineVirtualCamera = Camera.main.GetComponent<CameraControl>().tpsCinemachineVirtualCamera;

        if (photonView.IsMine)
        {
            tpsCinemachineVirtualCamera.Follow = transform; // tps ���� ī�޶��� ���� ����� �ڽ��� Ʈ���������� ����
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

    // �÷��̾� �ڼ��� ���� ī�޶� ���� �����ϴ� �޼���
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
