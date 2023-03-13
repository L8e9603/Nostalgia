using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Title
/// </summary>

public class FullBodyLocomotion : MonoBehaviour
{
    #region FIELDS SERIALIZED
    #endregion

    #region FIELDS
    public enum Stance
    {
        Stand, Crouch, Prone, Walk, Run, Sprint
    }

    private FullBodyInputManager m_fullBodyInputManager;
    private Rigidbody m_rigidbody;
    private CapsuleCollider m_collider;
    private Animator m_animator;

    /// <summary>
    /// Animator
    /// </summary>
    private float animatorHorizon;
    private float animatorVertical;
    [SerializeField]
    private float animParameterLerpSpeed = 5f;

    public Vector3 moveDirection;
    [SerializeField] private Vector3 m_horizontal;
    [SerializeField] private Vector3 m_vertical;
    float inputMagnitudeX;
    float inputMagnitudeZ;
    [SerializeField] private float m_inputAmount;

    [SerializeField] private float m_offsetFloorY = 0.4f;
    [SerializeField] private float m_movementSpeed = 3f;
    private Vector3 m_raycastFloorPos;
    private Vector3 m_combinedRaycast;
    private Vector3 m_gravity;
    private Vector3 m_floorMovement;
    private float m_groundRayLenght;

    /// <summary>
    /// 마우스로 캐릭터 회전시 원하는 위치에 정지시키기 구현에 필요
    /// </summary>
    [SerializeField]
    private Rigidbody m_headTargetRigidbody = default;
    #endregion

    #region UNITY
    private void Start()
    {
        m_fullBodyInputManager = GetComponent<FullBodyInputManager>();
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();
        m_animator = GetComponent<Animator>();
    }

    private void Update()
    {
        DrawGizmos();
        AnimatorParameterControl();
    }

    private void FixedUpdate()
    {
        UpdateMovementInput();
        UpdatePhysics();
    }

    #endregion

    #region FUNCTION

    // 디버깅용 기즈모 그리기
    private void DrawGizmos()
    {
        Debug.DrawRay(transform.position, moveDirection, Color.yellow);
        Debug.DrawLine(transform.position, moveDirection);
    }

    // 애니메이터 파라미터 제어
    private void AnimatorParameterControl()
    {
        if (m_fullBodyInputManager.moveHorizontal == 0f)
        {
            animatorHorizon = Mathf.Lerp(animatorHorizon, 0f, Time.deltaTime * animParameterLerpSpeed);
        }

        if (m_fullBodyInputManager.moveVertical == 0f)
        {
            animatorVertical = Mathf.Lerp(animatorVertical, 0f, Time.deltaTime * animParameterLerpSpeed);
        }

        // 수평 이동시
        if (m_fullBodyInputManager.moveHorizontal > 0) // A
        {
            animatorHorizon = Mathf.Lerp(animatorHorizon, 1f, Time.deltaTime * animParameterLerpSpeed);
        }
        else if (m_fullBodyInputManager.moveHorizontal < 0) // D
        {
            animatorHorizon = Mathf.Lerp(animatorHorizon, -1f, Time.deltaTime * animParameterLerpSpeed);
        }

        // 수직 이동시
        if (m_fullBodyInputManager.moveVertical > 0) // W
        {

            animatorVertical = Mathf.Lerp(animatorVertical, 1f, Time.deltaTime * animParameterLerpSpeed);

        }
        else if (m_fullBodyInputManager.moveVertical < 0) // S
        {
            animatorVertical = Mathf.Lerp(animatorVertical, -1f, Time.deltaTime * animParameterLerpSpeed);
        }


        m_animator.SetFloat("Horizontal", animatorHorizon);
        m_animator.SetFloat("Vertical", animatorVertical);
    }

    // 이동 입력 감지
    private void UpdateMovementInput()
    {
        // 이동할 방향 벡터 산출
        moveDirection = Vector3.zero;

        m_horizontal = m_fullBodyInputManager.moveHorizontalFloat * transform.right;
        m_vertical = m_fullBodyInputManager.moveVerticalFloat * new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;

        if (m_fullBodyInputManager.moveHorizontal > 0)
        {
            m_horizontal.x += Time.deltaTime * 5f;
        }
        else if(m_fullBodyInputManager.moveHorizontal < 0)
        {
            m_horizontal.x -= Time.deltaTime * 5f;
        }
        else
        {
            m_horizontal.x = Mathf.Lerp(m_horizontal.x, 0f, Time.deltaTime * 5f);
        }
        m_horizontal.x = Mathf.Clamp(m_horizontal.x, -1f, 1f);

        if (m_fullBodyInputManager.moveVertical > 0)
        {
            m_vertical.z += Time.deltaTime * 5f;
        }
        else if (m_fullBodyInputManager.moveVertical < 0)

        {
            m_vertical.z -= Time.deltaTime * 5f;
        }
        else
        {
            m_vertical.z = Mathf.Lerp(m_vertical.z, 0f, Time.deltaTime * 5f);
        }
        m_vertical.z = Mathf.Clamp(m_vertical.z, -1f, 1f);

        moveDirection = (m_horizontal + m_vertical);
        moveDirection = Vector3.ClampMagnitude(moveDirection, 1f); // 대각선 이동속도가 빨라지는 현상 제거, .normalize 사용하여 구현시 실시간 변화량 추적 불가능 하기때문에 ClampMagnitude() 메서드 사용


        // 입력량 산출
        if (m_fullBodyInputManager.moveHorizontal != 0)
        {
            inputMagnitudeX += Time.deltaTime * 10f;
        }
        else
        {
            inputMagnitudeX = Mathf.Lerp(inputMagnitudeX, 0f, Time.deltaTime * 10f);
        }

        if (m_fullBodyInputManager.moveVertical != 0)
        {
            inputMagnitudeZ += Time.deltaTime * 10f;
        }
        else
        {
            inputMagnitudeZ = Mathf.Lerp(inputMagnitudeZ, 0f, Time.deltaTime * 10f);
        }

        float inputMagnitude = Mathf.Abs(inputMagnitudeX) + Mathf.Abs(inputMagnitudeZ);
        m_inputAmount = Mathf.Clamp01(inputMagnitude);
    }

    // 물리 갱신
    private void UpdatePhysics()
    {
        m_groundRayLenght = (m_collider.height * 0.5f) + m_offsetFloorY;
        if (FloorRaycasts(0, 0, m_groundRayLenght).transform == null)
        {
            m_gravity += (Vector3.up * Physics.gravity.y * Time.fixedDeltaTime);
        }
        m_rigidbody.velocity = (m_movementSpeed * moveDirection * m_inputAmount) + m_gravity;

        m_headTargetRigidbody.velocity = m_rigidbody.velocity; // 마우스로 캐릭터 회전시 원하는 위치에 정지 가능

        m_floorMovement = new Vector3(m_rigidbody.position.x, FindFloor().y, m_rigidbody.position.z);
        if (FloorRaycasts(0, 0, m_groundRayLenght).transform != null && m_floorMovement != m_rigidbody.position)
        {
            m_rigidbody.MovePosition(m_floorMovement);
            m_gravity.y = 0;
        }
    }

    // 바닥 감지
    private Vector3 FindFloor()
    {
        float raycastWidth = 0.25f;
        int floorAverage = 1;
        m_combinedRaycast = FloorRaycasts(0, 0, m_groundRayLenght).point;
        floorAverage += (GetFloorAverage(raycastWidth, 0) + GetFloorAverage(-raycastWidth, 0) + GetFloorAverage(0, raycastWidth) + GetFloorAverage(0, -raycastWidth));
        return m_combinedRaycast / floorAverage;
    }

    // 바닥 감지 레이캐스트
    private RaycastHit FloorRaycasts(float t_offsetx, float t_offsetz, float t_raycastLength)
    {
        RaycastHit hit;
        m_raycastFloorPos = transform.TransformPoint(0 + t_offsetx, m_collider.center.y, 0 + t_offsetz);
        Debug.DrawRay(m_raycastFloorPos, Vector3.down * m_groundRayLenght, Color.magenta);
        Physics.Raycast(m_raycastFloorPos, -Vector3.up, out hit, t_raycastLength);
        return hit;
    }

    // 
    private int GetFloorAverage(float t_offsetx, float t_offsetz)
    {
        if (FloorRaycasts(t_offsetx, t_offsetz, m_groundRayLenght).transform != null)
        {
            m_combinedRaycast += FloorRaycasts(t_offsetx, t_offsetz, m_groundRayLenght).point;
            return 1;
        }
        else
        {
            return 0;
        }
    }
    #endregion

}
