using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Photon.Pun;

/// <summary>
/// Full Body FPS
/// </summary>


public class FullBodyCameraSetUp : MonoBehaviourPun
{
    #region FIELDS SERIALIZED
    [SerializeField] private Transform target;
    [SerializeField] private Transform firstPersonCameraPosition;
    [SerializeField] private Transform thirdPersonCameraPosition;
    [SerializeField] private Transform headCameraPosition;
    #endregion

    #region FIELDS
    private CinemachineVirtualCamera firstPersonCinemachineVirtualCamera;
    private CinemachineVirtualCamera thirdPersonCinemachineVirtualCamera;
    private CinemachineVirtualCamera headCinemachineVirtualCamera;

    #endregion

    #region UNITY
    void Start()
    {
        if (photonView.IsMine)
        {
            CameraSetUp();
        }
    }   

    void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        ChangeCamera();
    }
    #endregion

    #region FUNCTION
    /// <summary>
    /// 카메라 초기설정, 1인칭 카메라로 게임 시작
    /// </summary>
    private void CameraSetUp()
    {
        // 1인칭 카메라 설정
        firstPersonCinemachineVirtualCamera = GameObject.FindWithTag("FirstPersonCamera").GetComponent<CinemachineVirtualCamera>();
        firstPersonCinemachineVirtualCamera.gameObject.transform.SetParent(firstPersonCameraPosition); // 1인칭 카메라를 플레이어의 자식으로 설정
        firstPersonCinemachineVirtualCamera.gameObject.transform.position = firstPersonCameraPosition.position; // 1인칭 카메라 위치 설정
        //firstPersonCinemachineVirtualCamera.LookAt = target; // 1인칭 카메라가 바라볼 타겟 설정

        // 3인칭 카메라 설정
        thirdPersonCinemachineVirtualCamera = GameObject.FindWithTag("ThirdPersonCamera").GetComponent<CinemachineVirtualCamera>();
        thirdPersonCinemachineVirtualCamera.gameObject.transform.SetParent(thirdPersonCameraPosition); // 1인칭 카메라를 플레이어의 자식으로 설정
        thirdPersonCinemachineVirtualCamera.gameObject.transform.position = thirdPersonCameraPosition.position; // 1인칭 카메라 위치 설정

        // 헤드캠 설정
        headCinemachineVirtualCamera = GameObject.FindWithTag("HeadCamera").GetComponent<CinemachineVirtualCamera>();
        headCinemachineVirtualCamera.gameObject.transform.SetParent(headCameraPosition); // 헤드 카메라를 플레이어의 자식으로 설정
        headCinemachineVirtualCamera.gameObject.transform.position = headCameraPosition.position; // 헤드캠 위치 설정
    }

    /// <summary>
    /// 카메라 변경
    /// </summary>
    private void ChangeCamera()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (firstPersonCinemachineVirtualCamera.enabled)
            {
                thirdPersonCinemachineVirtualCamera.enabled = true;

                firstPersonCinemachineVirtualCamera.enabled = false;
                headCinemachineVirtualCamera.enabled = false;
            }
            else if (thirdPersonCinemachineVirtualCamera.enabled)
            {
                headCinemachineVirtualCamera.enabled = true;

                firstPersonCinemachineVirtualCamera.enabled = false;
                thirdPersonCinemachineVirtualCamera.enabled = false;
            }
            else if (headCinemachineVirtualCamera.enabled)
            {
                firstPersonCinemachineVirtualCamera.enabled = true;

                thirdPersonCinemachineVirtualCamera.enabled = false;
                headCinemachineVirtualCamera.enabled = false;
            }
        }
    }
    #endregion
}
