using UnityEngine;
using RootMotion.FinalIK;
using Cinemachine;

public class UpperBodyIK : MonoBehaviour
{
    #region Variables
    /// <summary>
    /// Final IK 
    /// </summary>
    [Header("Final IK Modules")]
    [SerializeField]
    private LookAtIK m_headLookAtIK = default;
    [SerializeField]
    private LookAtIK m_bodyLookAtIK = default;
    
    [SerializeField]
    private ArmIK m_leftArmIK = default;
    [SerializeField]
    private ArmIK m_rightArmIK = default;

    [SerializeField]
    private FullBodyBipedIK m_fbbIK = default;

    /// <summary>
    /// Head Stabilization
    /// </summary>
    [Header("LookAt Settings")]
    [SerializeField]
    private Transform m_camera = default;
    [SerializeField]
    private Transform m_headTarget = default;
    [Header("Head Effector Settings")]
    [SerializeField]
    private Transform m_headEffector = default;

    /// <summary>
    /// ADS System
    /// </summary>
    [Header("Arms Settings")]
    [SerializeField]
    private Transform m_rightHandTarget = default;
    [SerializeField]
    private float m_rightHandPosSpeed = 1f;
    [SerializeField]
    private float m_rightHandRotSpeed = 1f;
    [Header("ADS Settings")]
    [SerializeField]
    private Transform m_rightHandHips = default;
    [SerializeField]
    private Transform m_rightHandADS = default;
    [SerializeField]
    private float m_adsTransitionTime = 1f; // 정조준 전환 속도
    [SerializeField]
    private float m_hipsFov = 60f;
    [SerializeField]
    private float m_adsFov = 40f;

    private float m_transitionADS; // 정조준 가중치
    private Vector3 m_rightHandFollow;
    private Quaternion m_rightHandFollowRot;
    private Vector3 m_refRightHandFollow;
    private FullBodyInputManager m_fullBodyInputManager;
    private CinemachineVirtualCamera m_firstPersonCVCam = default;

    /// <summary>
    /// Weapon Sway System
    /// </summary>
    [Header("Sway settings")]
    [SerializeField]
    private float m_A = 1;
    [SerializeField]
    private float m_B = 2;
    [SerializeField]
    private float m_sizeReducerFactor = 10f;
    [SerializeField]
    private float m_thetaIncreaseFactor = 0.01f;
    [SerializeField]
    private float m_swayLerpSpeed = 15f;

    private float m_theta;
    private Vector3 m_swayPos;

    /// <summary>
    /// Look Around By Mouse
    /// </summary>
    [SerializeField]
    private Transform m_bodyTarget = default;
    [Range(-89, 0)]
    [SerializeField]
    private float _maxAngleUp = -50f;
    [Range(0, 89)]
    [SerializeField]
    private float _maxAngleDown = 70f;
    [Range(-89f, 89f)]
    private float m_bodyOffsetAngle = 45f;

    /// <summary>
    /// 캐릭터 회전
    /// </summary>
    [SerializeField]
    private float m_rotateSpeed = 7f;
    private float m_currentBodyAngle;

    /// <summary>
    /// HeadEffector Position
    /// </summary>
    [SerializeField]
    private Transform m_headEffectorNeutral = default;
    [SerializeField]
    private Transform m_headEffectorUp = default;
    [SerializeField]
    private Transform m_headEffectorDown = default;

    #endregion
    #region BuiltIn Methods
    private void Start()
    {
        m_headLookAtIK.enabled = false;
        m_bodyLookAtIK.enabled = false;

        m_rightArmIK.enabled = false;
        m_leftArmIK.enabled = false;

        m_fbbIK.enabled = false;

        m_fullBodyInputManager = GetComponent<FullBodyInputManager>();

        m_firstPersonCVCam = GameObject.FindWithTag("FirstPersonCamera").GetComponent<CinemachineVirtualCamera>();

        m_currentBodyAngle = m_bodyOffsetAngle; // 캐릭터 회전

    }
    private void Update()
    {
        // 순서 매우 중요
        m_bodyLookAtIK.solver.FixTransforms();
        m_headLookAtIK.solver.FixTransforms();

        m_fbbIK.solver.FixTransforms();

        m_rightArmIK.solver.FixTransforms();
        m_leftArmIK.solver.FixTransforms();
    }
    private void LateUpdate()
    {
        LookAtIKUpdate();
        
        FBBIKUpdate();

        ArmsIKUpdate();
    }
    #endregion
    #region Custom Methods
    private void LookAtIKUpdate()
    {
        m_bodyLookAtIK.solver.Update();
        m_headLookAtIK.solver.Update();
    }

    private void ArmsIKUpdate()
    {
        UpdateSwayOffset();

        AimDownSightUpdate();

        m_rightArmIK.solver.Update();
        m_leftArmIK.solver.Update();
    }

    private void FBBIKUpdate()
    {
        m_fbbIK.solver.Update();

        // Head Stabilization   
        m_camera.LookAt(m_headTarget);
        m_headEffector.LookAt(m_headTarget);

        // 마우스 입력으로 시야 방향 바꾸기 메서드
        UpdateLookTargetPos(); 

        // 시야가 일정범위 넘어서면 캐릭터 회전
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(new Vector3(m_camera.transform.forward.x, 0f, m_camera.transform.forward.z)), Time.smoothDeltaTime * m_rotateSpeed);
    }

    private void AimDownSightUpdate()
    {
        if (m_fullBodyInputManager.aim == false)
        {
            m_transitionADS = Mathf.Lerp(m_transitionADS, 0, Time.smoothDeltaTime * m_adsTransitionTime);
            m_rightHandTarget.rotation = m_rightHandHips.rotation;
        }
        else
        {
            m_transitionADS = Mathf.Lerp(m_transitionADS, 1, Time.smoothDeltaTime * m_adsTransitionTime);
            m_rightHandTarget.rotation = m_rightHandADS.rotation;
        }

        m_rightHandFollow = 
            Vector3.Lerp(m_rightHandHips.position, m_rightHandADS.position, m_transitionADS);

        m_rightHandFollowRot = 
            Quaternion.Lerp(m_rightHandHips.rotation, m_rightHandADS.rotation, m_transitionADS);

        m_firstPersonCVCam.m_Lens.FieldOfView = Mathf.Lerp(m_hipsFov, m_adsFov, m_transitionADS); // FOV 제어

        m_rightHandFollow += m_camera.TransformVector(m_swayPos); 

        m_rightHandTarget.position = 
            Vector3.SmoothDamp(m_rightHandTarget.position, m_rightHandFollow, ref m_refRightHandFollow, m_rightHandPosSpeed * Time.smoothDeltaTime);

        m_rightHandTarget.rotation = 
            Quaternion.Lerp(m_rightHandTarget.rotation, m_rightHandFollowRot, Time.smoothDeltaTime * m_rightHandRotSpeed);

    }

    private void UpdateSwayOffset()
    {
        Vector3 targetPos = (LissajousCurve(m_theta, m_A, Mathf.PI, m_B) / m_sizeReducerFactor);
        m_swayPos = Vector3.Lerp(m_swayPos, targetPos, Time.smoothDeltaTime * m_swayLerpSpeed);
        m_theta += m_thetaIncreaseFactor;
    }

    private Vector3 LissajousCurve(float theta, float A, float delta, float B)
    {
        Vector3 pos = Vector3.zero;
        pos.x = Mathf.Sin(theta);
        pos.y = A * Mathf.Sin(B * theta + delta);
        return pos;
    }

    private void UpdateLookTargetPos()
    {
        Vector3 targetForward = Quaternion.LookRotation(new Vector3(m_camera.transform.forward.x, 0f, m_camera.transform.forward.z)) * Vector3.forward;
        float angle = Vector3.SignedAngle(targetForward, m_camera.forward, m_camera.right);
        float percent;
        float maxY = 100f;
        float minY = -100f;
        if (angle < 0)
        {
            percent = Mathf.Clamp01(angle / _maxAngleUp);
            if (percent >= 1f)
            {
                maxY = 0f;
            }

            m_headEffector.position = Vector3.Lerp(m_headEffectorNeutral.position, m_headEffectorUp.position, percent); // 마우스 위 아래 이동시 HeadEffector 위치 수정시켜 다리 늘어나는 현상 방지
        }
        else
        {
            percent = Mathf.Clamp01(angle / _maxAngleDown);
            if (percent >= 1f)
            {
                minY = 0f;
            }

            m_headEffector.position = Vector3.Lerp(m_headEffectorNeutral.position, m_headEffectorDown.position, percent);
        }

        Vector3 offset = m_camera.right * m_fullBodyInputManager.XLookAxis + m_camera.up * Mathf.Clamp(m_fullBodyInputManager.YLookAxis, minY, maxY); 
        offset += m_headTarget.transform.position;
        Vector3 projectedPoint = (offset - m_camera.position).normalized * 20f + m_camera.position;

        m_currentBodyAngle = Mathf.Lerp(m_bodyOffsetAngle, 0, percent); // 캐릭터 회전

        // 헤드, 바디 타겟 위치 업데이트
        m_headTarget.transform.position = projectedPoint;
        // m_bodyTarget.transform.position = GetPosFromAngle(projectedPoint, m_bodyOffsetAngle, transform.right);
        m_bodyTarget.transform.position = GetPosFromAngle(projectedPoint, m_currentBodyAngle, transform.right);
    }

    private Vector3 GetPosFromAngle(Vector3 projectedPoint, float angle, Vector3 axis)
    {
        float dist = (projectedPoint - transform.position).magnitude * Mathf.Tan(angle * Mathf.Deg2Rad);
        return projectedPoint + (dist * axis);
    }

    #endregion
}