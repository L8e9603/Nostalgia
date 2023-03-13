using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement2 : MonoBehaviour
{
    [SerializeField]
    private float _mouseSensitivity = 3.0f;

    private float _rotationY;
    private float _rotationX;

    [SerializeField]
    private Transform _target;

    [SerializeField]
    private float _distanceFromTarget = 3.0f;

    private Vector3 _currentRotation;
    private Vector3 _smoothVelocity = Vector3.zero;

    [SerializeField]
    private float _smoothTime = 3.0f;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        float mouseX = -Input.GetAxis("Mouse Y") * _mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse X") * _mouseSensitivity;

        _rotationX += mouseX;
        _rotationY += mouseY;

        _rotationX = Mathf.Clamp(_rotationX, -60, 60);

        Vector3 nextRotation = new Vector3(_rotationX, _rotationY);
        _currentRotation = Vector3.SmoothDamp(_currentRotation, nextRotation, ref _smoothVelocity, _smoothTime);

        transform.localEulerAngles = new Vector3(_rotationX, _rotationY, 0);

        transform.position = _target.position - transform.forward * _distanceFromTarget;
    }
}
