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
    public float mouseSensitivity = 100f; // 마우스 감도
    public float clampAngle = 60f; // 카메라 제한 각도

    private float rotX; // 마우스 입력을 받을 변수 (rotationX)
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

    // 마우스 입력을 감지하는 메서드
    private void MouseInput()
    {
        rotX += -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime; // 마우스를 Y축, 즉 위아래로 움직이면 X축 회전
        rotY += Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;

        rotX = Mathf.Clamp(rotX, -clampAngle, clampAngle); // X축 제한

/*        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = rot;
*/    }

    // 부드러운 느낌의 카메라 조작1
    private void CameraControl1()
    {
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * 15f);
    }

    // 부드러운 느낌의 카메라 조작2
    private void CameraControl2()
    {
        Vector3 nextRotation = new Vector3(rotX, rotY);
        currentRotation = Vector3.SmoothDamp(currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);

        transform.localEulerAngles = new Vector3(rotX, rotY, 0);
    }

    // 즉각적인 반응의 카메라 조작, 조준상태에서 사용하는 카메라 조작
    private void AimedCameraControl()
    {
        Quaternion rot = Quaternion.Euler(rotX, rotY, 0);
        transform.rotation = rot;
    }

    // 1인칭, 3인칭 카메라 토글
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
