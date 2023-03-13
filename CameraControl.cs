using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraControl : MonoBehaviour
{
    [Header("Virtual Camera List")]
    public CinemachineVirtualCamera tpsCinemachineVirtualCamera;
    public CinemachineVirtualCamera fpsCinemachineVirtualCamera;

    [Header("Basic Camera Option")]
    public float mouseSensitivity = 100f; // ���콺 ����
    public float clampAngle = 60f; // ī�޶� ���� ����

    private float rotX; // ���콺 �Է��� ���� ���� (rotationX)
    private float rotY;

    private Vector3 currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;
    private float _smoothTime = 3.0f;

    public bool isAiming = false;

    public Transform fpsCameraTransform;

    void Start()
    {
/*        rotX = transform.localRotation.eulerAngles.x;
        rotY = transform.localRotation.eulerAngles.y;
*/        

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            if(fpsCameraTransform != null)
            {
                transform.SetParent(fpsCameraTransform);
            }
        }


        MouseInput();

        if (isAiming)
        {
            AimedCameraControl();
        }
        else
        {
            CameraControl1();
        }

        ToggleCamera();
    }

    // ���콺 �Է��� �����ϴ� �޼���
    private void MouseInput()
    {
        rotX += -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // ���콺�� Y��, �� ���Ʒ��� �����̸� X�� ȸ��
        rotY += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle); // X�� ����

/*        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = rot;
*/    }

    // �ε巯�� ������ ī�޶� ����1
    private void CameraControl1()
    {
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 15f);
    }

    // �ε巯�� ������ ī�޶� ����2
    private void CameraControl2()
    {
        Vector3 nextRotation = new Vector3(rotX, rotY);
        currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);

        transform.localEulerAngles = new Vector3(rotX, rotY, 0);
    }

    // �ﰢ���� ������ ī�޶� ����, ���ػ��¿��� ����ϴ� ī�޶� ����
    private void AimedCameraControl()
    {
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = rot;
    }

    // 1��Ī, 3��Ī ī�޶� ���
    private void ToggleCamera()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (tpsCinemachineVirtualCamera.enabled)
            {
                tpsCinemachineVirtualCamera.enabled = false;
                fpsCinemachineVirtualCamera.enabled = true;
            }
            else
            {
                tpsCinemachineVirtualCamera.enabled = true;
                fpsCinemachineVirtualCamera.enabled = false;
            }
        }
    }

}
