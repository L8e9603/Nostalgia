using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Cinemachine;
using UnityEngine.Animations.Rigging;
//using UnityEditor.Animations;

public class ThirdPersonShooterController: MonoBehaviourPun
{
    public enum WeaponType
    {
        Hand, 
        OneHandedMeleeWeapon,
        TwoHandedMeleeWeapon,
        Rifle,
        Pistol,
        Grenade
    }
    public WeaponType weaponType { get; set; }

    [Header("Rig Control")]
    [SerializeField] private Rig rig;
    public float rigWeightChangeSpeed = 5f;
    public float rigWeightAimed = 1f;
    public float rigWeightNotAimed = 1f;
    //public MultiAimConstraint bodyMultiAimConstraint;
    public float bodyWeightAimed = 0.6f;
    public float bodyWeightNotAimed = 0f;
    //public MultiAimConstraint headMultiAimConstraint;
    public float headWeightAimed = 1f;
    public float headWeightNotAimed = 0f;
/*    public TwoBoneIKConstraint leftHandTwoBoneIKConstraint;
    public float leftHandWeightAimed = 1f;
    public float leftHandWeightNotAimed = 0f;
    public TwoBoneIKConstraint rightHandTwoBoneIKConstraint;
    public float rightHandWeightAimed = 1f;
    public float rightHandWeightNotAimed = 0f;
*/
    [Header("Weapon List")]
    public GameObject[] weapons;
    public GameObject testWeapon;

    [Header("Aim Target Object")]
    public GameObject thirdPersonAimTarget;
    public float rayOffset = 3.5f;
    public float aimTargetObjectSpeed = 7.5f;

    [Header("Aim Setting")]
    public bool isAiming = false;
    public float aimedFOV = 25f;
    public float transitionSpeed = 6.2f;
    public float defaultTrackedObjectOffset_x = 0f;
    public float aimedTrackedObjectOffset_x = 0.5f;
    public float defaultScreen_x = 0.34f;
    public float aimedScreen_x = 0.5f;
    private float defaultFOV;
    private float horizontal;
    private float vertical;

    [Header("Punch")]
    public float punchDamage = 10f;
    public float lastPunchTime;
    public float punchDelay = 0.2f;

    [Header("Interact")]
    public float interactRayLength = 1.8f;
    public float interactRayStartOffset = 2.5f;

    private PlayerInput playerInput;
    private PlayerMovement playerMovement;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    private CameraControl cameraMovement;
    private CinemachineVirtualCamera tpsCinemachineVirtualCamera;
    private CinemachineFramingTransposer tpsCinemachineVirtualCameraFramingTransposer;

    [Header("Weapon")]
    public Gun gun;
    public TwoBoneIKConstraint leftHandTwoBoneIKConstraint;
    public TwoBoneIKConstraint rightHandTwoBoneIKConstraint;
    /*    public Transform leftHandIKTransform;
        public Transform rightHandIKTransform;
    */

    public Transform fpsCameraTransform;

    private void Start()
    {
        // thirdPersonAimTarget = GameObject.FindGameObjectWithTag("ThirdPersonAimTarget");

        /*        WeightedTransform weightedTransform = new WeightedTransform { transform = thirdPersonAimTarget.transform, weight = 1f };
                var sourceObjects = new WeightedTransformArray { weightedTransform };
                bodyMultiAimConstraint.data.sourceObjects = sourceObjects;
                headMultiAimConstraint.data.sourceObjects = sourceObjects;
                rightHandMultiAimConstraint.data.sourceObjects = sourceObjects;
        */

        // ���� �ݱ� ���� ������ ����
        weaponType = WeaponType.Hand;

        rig = GetComponentInChildren<Rig>();

        playerInput = GetComponent<PlayerInput>();
        playerMovement = GetComponent<PlayerMovement>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        cameraMovement = Camera.main.GetComponent<CameraControl>();
        tpsCinemachineVirtualCamera = cameraMovement.tpsCinemachineVirtualCamera;
        defaultFOV = tpsCinemachineVirtualCamera.m_Lens.FieldOfView; // Lens �Ӽ� �⺻�� ����
        tpsCinemachineVirtualCameraFramingTransposer = tpsCinemachineVirtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>(); // Body �Ӽ� �⺻�� ����

        cameraMovement.fpsCameraTransform = this.fpsCameraTransform;
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }
        
        RigControl();
        AnimatorParameterControl();
        Aim();
        Punch();
        MoveAimTargetObject();
        Interact();
        ChangeWeapon();
        // RotatePlayerWhileAiming();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        RotatePlayerWhileAiming();
    }

    // ���� �ҽ� ������Ʈ�� ��ġ�� ī�޶� �߾����� �̵���Ű�� �޼���
    private void MoveAimTargetObject()
    {
        Debug.DrawRay((Camera.main.transform.position + Camera.main.transform.forward * rayOffset), Camera.main.transform.forward, Color.red); // �� �信 Ray �׸���

        RaycastHit raycastHit;
        if (Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward * rayOffset, Camera.main.transform.forward, out raycastHit))
        {
            if (isAiming)
            {
                thirdPersonAimTarget.transform.position = Vector3.Lerp(thirdPersonAimTarget.transform.position, raycastHit.point, Time.deltaTime * aimTargetObjectSpeed);
            }
            else
            {
                thirdPersonAimTarget.transform.position = Vector3.Lerp(thirdPersonAimTarget.transform.position, raycastHit.point, Time.deltaTime * aimTargetObjectSpeed);
            }
        }
    }

    // ���� ����ġ ���� �޼���
    private void RigControl()
    {
        if (weaponType == WeaponType.Hand)
        {
            // �ӽ�, �Ʒ�ó�� ������ ��
            leftHandTwoBoneIKConstraint.weight = 0f;
            rightHandTwoBoneIKConstraint.weight = 0f;
        }
        else if (weaponType == WeaponType.Rifle)
        {
        }

        // ���ؽ�
        if (isAiming)
        {
/*            rig.weight = Mathf.Lerp(rig.weight, rigWeightAimed, Time.deltaTime * rigWeightChangeSpeed); // ��ü ���� ����ġ ����
            headMultiAimConstraint.weight = Mathf.Lerp(headMultiAimConstraint.weight, headWeightAimed, Time.deltaTime * 5f); // �Ӹ� ����ġ
            bodyMultiAimConstraint.weight = Mathf.Lerp(bodyMultiAimConstraint.weight, bodyWeightAimed, Time.deltaTime * 5f); // ��ü ����ġ
            leftHandTwoBoneIKConstraint.weight = leftHandWeightAimed; // �޼� ����ġ
*/        }
        else
        {
/*            rig.weight = Mathf.Lerp(rig.weight, rigWeightNotAimed, Time.deltaTime * rigWeightChangeSpeed);
            headMultiAimConstraint.weight = Mathf.Lerp(headMultiAimConstraint.weight, headWeightNotAimed, Time.deltaTime * 5f); // �Ӹ� ����ġ
            bodyMultiAimConstraint.weight = Mathf.Lerp(bodyMultiAimConstraint.weight, bodyWeightNotAimed, Time.deltaTime * 5f); // ��ü ����ġ
            leftHandTwoBoneIKConstraint.weight = leftHandWeightNotAimed;
*/        }
    }

    // ���� �޼���
    private void Aim()
    {
        // ���� �Է� ������
        if (playerInput.aim)
        {
            isAiming = true;
            cameraMovement.isAiming = true; // AimedCameraControl()�� �̿��� ���ۿ��� CameraControl1()�� �̿��� �������� �����ϱ� ����

            // ����ī�޶� ��ġ �� �þ߰� ����
            tpsCinemachineVirtualCameraFramingTransposer.m_TrackedObjectOffset.x = Mathf.Lerp(tpsCinemachineVirtualCameraFramingTransposer.m_TrackedObjectOffset.x, aimedTrackedObjectOffset_x, Time.deltaTime * transitionSpeed);
            tpsCinemachineVirtualCameraFramingTransposer.m_ScreenX = Mathf.Lerp(tpsCinemachineVirtualCameraFramingTransposer.m_ScreenX, aimedScreen_x, Time.deltaTime * transitionSpeed);
            tpsCinemachineVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(tpsCinemachineVirtualCamera.m_Lens.FieldOfView, aimedFOV, Time.deltaTime * transitionSpeed); // FOV ����

        }
        // ���� ���� �ƴ� ��
        else
        {
            isAiming = false;
            cameraMovement.isAiming = false;

            // ����ī�޶� ����
            tpsCinemachineVirtualCameraFramingTransposer.m_TrackedObjectOffset.x = Mathf.Lerp(tpsCinemachineVirtualCameraFramingTransposer.m_TrackedObjectOffset.x, defaultTrackedObjectOffset_x, Time.deltaTime * transitionSpeed);
            tpsCinemachineVirtualCameraFramingTransposer.m_ScreenX = Mathf.Lerp(tpsCinemachineVirtualCameraFramingTransposer.m_ScreenX, defaultScreen_x, Time.deltaTime * transitionSpeed);
            tpsCinemachineVirtualCamera.m_Lens.FieldOfView = Mathf.Lerp(tpsCinemachineVirtualCamera.m_Lens.FieldOfView, defaultFOV, Time.deltaTime * transitionSpeed); // FOV ����
        }
    }

    private void AnimatorParameterControl()
    {

        /*        if (playerInput.moveHorizontal == 0f || playerInput.moveVertical == 0f)
                {
                    horizontal = Mathf.Lerp(horizontal, 0f, Time.deltaTime * 5f);
                    vertical = Mathf.Lerp(vertical, 0f, Time.deltaTime * 5f);
                }

                if (playerInput.moveHorizontal > 0)
                {
                    horizontal += Time.deltaTime * 10f;
                }
                else if (playerInput.moveHorizontal < 0)
                {
                    horizontal -= Time.deltaTime * 10f;
                }

                if (playerInput.moveVertical > 0)
                {
                    vertical += Time.deltaTime * 10f;
                }
                else if (playerInput.moveVertical < 0)
                {
                    vertical -= Time.deltaTime * 10f;
                }

                playerAnimator.SetFloat("Horizontal", Mathf.Clamp(horizontal, -1f, 1f));
                playerAnimator.SetFloat("Vertical", Mathf.Clamp(vertical, -1f, 1f));
        */

        // ���� Ÿ�Կ� ���� �ִϸ��̼� ����
        if (weaponType == WeaponType.Hand)
        {
            playerAnimator.SetTrigger("HandEquiped");
            playerAnimator.ResetTrigger("RifleEquiped");
        }
        else if (weaponType == WeaponType.Rifle)
        {
            playerAnimator.SetTrigger("RifleEquiped");
            playerAnimator.ResetTrigger("HandEquiped");
        }

        // ������
        if (playerInput.aim)
        {
            // BaseLayer ����
            playerAnimator.SetBool("IsAiming", true);

            // Upper Body Layer ����
            if (weaponType == WeaponType.Hand)
            {
                playerAnimator.SetBool("IsHandAiming", true);
            }
            else if (weaponType == WeaponType.Rifle)
            {
                playerAnimator.SetBool("IsRifleAiming", true);
            }
        }
        // ���� ����
        else
        {
            playerAnimator.SetBool("IsAiming", false);

            if (weaponType == WeaponType.Hand)
            {
                playerAnimator.SetBool("IsHandAiming", false);
            }
            else if (weaponType == WeaponType.Rifle)
            {
                playerAnimator.SetBool("IsRifleAiming", false);
            }
        }
    }

    // ���� ���̸� �÷��̾ ī�޶� ���� �������� ȸ����Ű�� �޼���
    private void RotatePlayerWhileAiming()
    {
        // ���� ���϶��� �÷��̾ ī�޶� ���� �������� ȸ��
        if (isAiming)
        {
            // ���� ����� �����Ϸ��� ������ �ݴ뿴�ٸ� ���� ��¦ ���� ȸ���� ������� �ʵ��� ��
            if (Mathf.Sign(transform.forward.x) != Mathf.Sign(playerMovement.moveDirection.x) || Mathf.Sign(transform.forward.z) != Mathf.Sign(playerMovement.moveDirection.z))
            {
                //transform.Rotate(0f, 1f, 0f);
            }

            // ���� �������� �÷��̾� ȸ��
            Vector3 aimDirection = Vector3.zero;
            aimDirection.x = Camera.main.transform.forward.x;
            aimDirection.z = Camera.main.transform.forward.z;
            // transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 17f);

            Quaternion rotation = Quaternion.LookRotation(aimDirection);
            //playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * 15f);
            transform.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * 15f);
        }
    }

    // �ָ� ���� �޼���
    private void Punch()
    {
        if (isAiming)
        {
            if (playerInput.punch)
            {
                if (lastPunchTime > Time.time + punchDelay)
                    lastPunchTime = Time.time;
                playerAnimator.SetTrigger("Punch1");
            }
        }
    }

    // �ָ� �ִϸ��̸��̼� Ŭ������ ����Ǵ� �޼���
    private void PunchAnimationTriggerReset()
    {
        playerAnimator.ResetTrigger("Punch1");
        playerAnimator.ResetTrigger("Punch2");
    }

    private void QuickSlot()
    {

    }

    private void Interact()
    {
        Debug.DrawRay((Camera.main.transform.position + Camera.main.transform.forward * interactRayStartOffset), Camera.main.transform.forward.normalized * interactRayLength, Color.blue); // �� �信 Ray �׸���

        RaycastHit raycastHit;
        if (Physics.Raycast(Camera.main.transform.position + Camera.main.transform.forward * interactRayStartOffset, Camera.main.transform.forward, out raycastHit, interactRayLength))
        {
            if (raycastHit.collider.CompareTag("Weapon"))
            {
                Debug.Log(raycastHit.collider.name);
                
                // UI ǥ�� �����ϱ�
                
                if (playerInput.interact)
                {
                    weaponType = WeaponType.Rifle; // �÷��̾ ���� ����ִ� ���� Ÿ�� ����
                    raycastHit.collider.gameObject.SetActive(false);
                    testWeapon.SetActive(true);
                }
            }
        }
    }

    private void ChangeWeapon()
    {
        if (playerInput.weapon1)
        {
            weaponType = WeaponType.Rifle;
            testWeapon.SetActive(true);
        }
        else if (playerInput.weapon2)
        {
            weaponType = WeaponType.Pistol;
        }
        else if (playerInput.weapon3)
        {
            weaponType = WeaponType.Hand;
            testWeapon.SetActive(false);
        }
    }

    public void GetGunIKPosition(Transform leftHandPosition, Transform rightHandPosition)
    {
        leftHandTwoBoneIKConstraint.data.target = leftHandPosition;
        rightHandTwoBoneIKConstraint.data.target = rightHandPosition;
    }
}
