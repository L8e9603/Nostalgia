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
    /// ī�޶� �ʱ⼳��, 1��Ī ī�޶�� ���� ����
    /// </summary>
    private void CameraSetUp()
    {
        // 1��Ī ī�޶� ����
        firstPersonCinemachineVirtualCamera = GameObject.FindWithTag("FirstPersonCamera").GetComponent<CinemachineVirtualCamera>();
        firstPersonCinemachineVirtualCamera.gameObject.transform.SetParent(firstPersonCameraPosition); // 1��Ī ī�޶� �÷��̾��� �ڽ����� ����
        firstPersonCinemachineVirtualCamera.gameObject.transform.position = firstPersonCameraPosition.position; // 1��Ī ī�޶� ��ġ ����
        //firstPersonCinemachineVirtualCamera.LookAt = target; // 1��Ī ī�޶� �ٶ� Ÿ�� ����

        // 3��Ī ī�޶� ����
        thirdPersonCinemachineVirtualCamera = GameObject.FindWithTag("ThirdPersonCamera").GetComponent<CinemachineVirtualCamera>();
        thirdPersonCinemachineVirtualCamera.gameObject.transform.SetParent(thirdPersonCameraPosition); // 1��Ī ī�޶� �÷��̾��� �ڽ����� ����
        thirdPersonCinemachineVirtualCamera.gameObject.transform.position = thirdPersonCameraPosition.position; // 1��Ī ī�޶� ��ġ ����

        // ���ķ ����
        headCinemachineVirtualCamera = GameObject.FindWithTag("HeadCamera").GetComponent<CinemachineVirtualCamera>();
        headCinemachineVirtualCamera.gameObject.transform.SetParent(headCameraPosition); // ��� ī�޶� �÷��̾��� �ڽ����� ����
        headCinemachineVirtualCamera.gameObject.transform.position = headCameraPosition.position; // ���ķ ��ġ ����
    }

    /// <summary>
    /// ī�޶� ����
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
