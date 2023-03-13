using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class PlayerMovement : MonoBehaviourPun
{
    [Header("Basic Movements")]
    public bool isStanding = true;
    public float moveSpeed = 0f;
    public float defaultMoveSpeed = 3f;
    public float rotateSpeed = 10f;
    public Vector3 moveDirection;
    public Vector3 rotateDirection;
    [SerializeField] private float lastActionTime = 0f;
    private float horizontal;
    private float vertical;
    public float parameterChangeSpeed = 5f;

    [Header("Sprint")]
    public bool isSprinting = false;
    public float sprintMoveSpeed = 4f;

    [Header("Crouch")]
    public bool isCrouching = false;
    public float crouchMoveSpeed = 2f;
    public float crouchCoolDown = 1f;

    [Header("Prone")]
    public bool isProning = false;
    public float proneMoveSpeed = 1f;
    public float proneCoolDown = 1f;

    [Header("Jump")]
    public bool isJumping = false;
    public float jumpForce = 4f;
    public float jumpCoolDown = 1.2f;
    [SerializeField] private float lastJumpTime = 0f;

    private PlayerInput playerInput;
    private Rigidbody playerRigidbody;
    private Animator playerAnimator;
    private GroundCheck groundCheck;
    private ThirdPersonShooterController thirdPersonShooterController;


    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();
        groundCheck = GetComponentInChildren<GroundCheck>();
        thirdPersonShooterController = GetComponent<ThirdPersonShooterController>();
    }

    private void Update()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        Debug.DrawRay(transform.position, moveDirection, Color.yellow);
        AnimatorParameterControl();
        PlayerMoveSpeed();
        PlayerCrouch();
        PlayerProne();
        PlayerJump();
        Slip();
    }

    private void FixedUpdate()
    {
        if (!photonView.IsMine)
        {
            return;
        }

        PlayerRotate();
        PlayerMovePosition();
    }

    private void AnimatorParameterControl()
    {
        if (playerInput.moveHorizontal == 0f)
        {
            horizontal = Mathf.Lerp(horizontal, 0f, Time.deltaTime * parameterChangeSpeed);
        }

        if(playerInput.moveVertical == 0f)
        {
            vertical = Mathf.Lerp(vertical, 0f, Time.deltaTime * parameterChangeSpeed);

        }

        // 수평 이동시
        if (playerInput.moveHorizontal > 0) // A
        {
            horizontal = Mathf.Lerp(horizontal, 1f, Time.deltaTime * parameterChangeSpeed);
        }
        else if (playerInput.moveHorizontal < 0) // D
        {
            horizontal = Mathf.Lerp(horizontal, -1f, Time.deltaTime * parameterChangeSpeed);
        }
        
        // 수직 이동시
        if (playerInput.moveVertical > 0) // W
        {
            // W + LShift (전력질주시)
            if (isSprinting)
            {
                vertical = Mathf.Lerp(vertical, 5f, Time.deltaTime * 2f);
            }
            else
            {
                vertical = Mathf.Lerp(vertical, 1f, Time.deltaTime * parameterChangeSpeed);
            }
        }
        else if (playerInput.moveVertical < 0) // S
        {
            vertical = Mathf.Lerp(vertical, -1f, Time.deltaTime * parameterChangeSpeed);
        }

        playerAnimator.SetFloat("Horizontal", horizontal);
        playerAnimator.SetFloat("Vertical", vertical);

        if (isSprinting)
        {
            playerAnimator.SetBool("IsSprinting", true);
        }
        else if(!isSprinting)
        {
            playerAnimator.SetBool("IsSprinting", false);
        }
    }

    // 축 입력을 감지하고, 플레이어의 이동속도를 제어하는 메서드
    private void PlayerMoveSpeed()
    {
        if (!groundCheck.isGrounded)
        {
            return;
        }

        // 서있는 상태
        if (isStanding)
        {
            // 축 감지시 moveSpeed 가속
            if (playerInput.moveHorizontal != 0f || playerInput.moveVertical != 0f)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, defaultMoveSpeed, Time.deltaTime * 5f); // 기본 속도로 가속

                // 정면으로 이동하는 중, 정조준 상태가 아닐때, 전력질주 키 입력이 감지되면
                if (playerInput.moveVerticalFloat >= 1f && !thirdPersonShooterController.isAiming && playerInput.sprint)
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, sprintMoveSpeed, Time.deltaTime * 3f); // 달리기 속도로 가속
                    isSprinting = true;
                }
                else
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, defaultMoveSpeed, Time.deltaTime * 3f); // 기본 속도록 감속
                    isSprinting = false;
                }
            }
            // 축 감지가 되지 않는다면
            else if (playerInput.moveHorizontal == 0f || playerInput.moveVertical == 0f)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * 5f); // 0으로 감속
                isSprinting = false;
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * 5f); // 0으로 감속
            }
        }

        // 앉은 상태
        if (isCrouching)
        {
            if (playerInput.moveHorizontal != 0f || playerInput.moveVertical != 0f)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, crouchMoveSpeed, Time.deltaTime * 5f);
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * 5f);
            }
        }

        if (isProning)
        {
            if (playerInput.moveHorizontal != 0f || playerInput.moveVertical != 0f)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, proneMoveSpeed, Time.deltaTime * 5f);
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * 5f);
            }
        }
    }

    // Rigidbody를 이용해 플레이어의 위치를 변경시키는 메서드
    private void PlayerMovePosition()
    {
        if (!isJumping)
        {
            // 수평 이동 방향, 수직 이동방향 설정 -> moveDirection = 카메라 기준 수평 방향 벡터 + 카메라 기준 수직 방향 벡터
            Vector3 rightDirection = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;
            Vector3 forwardDirection = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
            moveDirection = playerInput.moveHorizontal * rightDirection + playerInput.moveVertical * forwardDirection;
        }

        // Rigidbody를 이용해 게임오브젝트 위치 변경, FixedUpdate에서 실행 시 Time.deltaTime은 Time.fixedDeltaTime으로 자동 변환
        // 대각선 이동시 더 빨리 이동하는 현상 방지하기 위해 ClampMagnitude() 메서드로 벡터의 절대값 제한
        //playerRigidbody.MovePosition(playerRigidbody.position + Vector3.ClampMagnitude(moveDirection, 1f) * moveSpeed * Time.deltaTime);
        playerRigidbody.velocity = Vector3.ClampMagnitude(moveDirection, 1f) * moveSpeed;
        playerAnimator.SetFloat("MoveSpeed", moveSpeed);
    }

    // 진행방향으로 플레이어를 회전시키는 메서드
    private void PlayerRotate()
    {
        if (thirdPersonShooterController.isAiming)
        {
            return;
        }

        // 축 입력 감지
        Vector2 moveInput = new Vector2(playerInput.moveHorizontal, playerInput.moveVertical);
        bool isMove = moveInput.magnitude != 0;

/*        if (!isJumping && isMove) // 점프 상태가 아니고, 땅 위에서 축 입력이 감지되면 입력된 방향으로 플레이어 회전
        {
            // 회전 방향 계산
            Vector3 lookRight = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;
            Vector3 lookForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
            // rotateDirection = lookRight * moveInput.x + lookForward * moveInput.y;
            rotateDirection = playerInput.moveHorizontal * lookRight + playerInput.moveVertical * lookForward;

            Quaternion rotation = Quaternion.LookRotation(rotateDirection);


            // 지금 바라보는 방향의 부호 != 나아갈 방향의 부호, 미리 회전을 조금 시켜줘서 정 반대 방향으로 이동할때 부자연스러운 현상 방지
            if (Mathf.Sign(transform.forward.x) != Mathf.Sign(moveDirection.x) || Mathf.Sign(transform.forward.z) != Mathf.Sign(moveDirection.z))
            {
                // transform.Rotate(0f, 1f, 0f);
                // transform.forward = Vector3.Lerp(transform.forward, rotateDirection, rotateSpeed * Time.deltaTime * 1.5f); // 회전 가속
                // playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * rotateSpeed);
            }

            //transform.forward = Vector3.Lerp(transform.forward, rotateDirection, rotateSpeed * Time.deltaTime);
            playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * rotateSpeed);
        }
*/

        if (!isJumping && isMove) // 점프 상태가 아니고, 땅 위에서 축 입력이 감지되면 입력된 방향으로 플레이어 회전
        {
            Vector3 aimDirection = Vector3.zero;
            aimDirection.x = Camera.main.transform.forward.x;
            aimDirection.z = Camera.main.transform.forward.z;
            // transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 17f);

            Quaternion rotation = Quaternion.LookRotation(aimDirection);
            // playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * rotateSpeed); // 물리기반 애니메이션 사용시 현재 구문 주석처리하기
            transform.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * rotateSpeed); // 물리 기반 애니메이션 사용시 현재 구문 사용하기
        }
    }

    // 점프 애니메이션 클립에서 실행되는 메서드
    private void PlayerRotateJump()
    {
        if(moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
    }

    // 앉기 메서드
    private void PlayerCrouch()
    {
        if (groundCheck.isGrounded && playerInput.crouch)
        {
            if (isStanding)
            {
                playerAnimator.SetBool("IsCrouching", true);
                playerAnimator.SetBool("IsStanding", false);
                isCrouching = true;
                isStanding = false;
            }
            else if (isCrouching)
            {
                playerAnimator.SetBool("IsCrouching", false);
                playerAnimator.SetBool("IsStanding", true);
                isCrouching = false;
                isStanding = true;
            }
            else if (isProning)
            {
                playerAnimator.SetBool("IsCrouching", true);
                playerAnimator.SetBool("IsProning", false);
                isCrouching = true;
                isProning = false;
            }
        }
    }

    // 엎드리기 메서드
    private void PlayerProne()
    {
        if (groundCheck.isGrounded && playerInput.prone && Time.time >= lastActionTime + proneCoolDown)
        {
            lastActionTime = Time.time;

            if (isStanding)
            {
                playerAnimator.SetBool("IsProning", true);
                playerAnimator.SetBool("IsStanding", false);
                isProning = true;
                isStanding = false;
            }
            else if (isCrouching)
            {
                playerAnimator.SetBool("IsProning", true);
                playerAnimator.SetBool("IsCrouching", false);
                isProning = true;
                isCrouching = false;
            }
            else if (isProning)
            {
                playerAnimator.SetBool("IsProning", false);
                playerAnimator.SetBool("IsStanding", true);
                isProning = false;
                isStanding = true;
            }
        }
    }

    // 점프 메서드
    private void PlayerJump()
    {
        // 앉기, 엎드리기, 조준 상태라면 점프 불가능
        if (isCrouching || isProning || thirdPersonShooterController.isAiming)
        {
            return;
        }
        
        // 땅 위에서 && 점프 키를 눌렀을 때 && 점프 쿨타임이 지났으면
        if (groundCheck.isGrounded && playerInput.jump && (Time.time >= lastJumpTime + jumpCoolDown) )
        {
            lastJumpTime = Time.time; // 마지막 점프 시간 갱신
            isJumping = true;
            playerAnimator.SetTrigger("Jump");
            playerRigidbody.AddForce( (moveDirection * (moveSpeed -1f)) + (transform.up.normalized * jumpForce) , ForceMode.VelocityChange);
        }
    }

    private void ReduceMoveSpeed()
    {
        // moveSpeed = Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * 5f);
    }

    private void OnTriggerStay(Collider other)
    {
        
        ReduceMoveSpeed();
    }

    // Land 애니메이션 클립에서 실행되는 메서드
    private void Land()
    {
        isJumping = false;
        groundCheck.isGrounded = true;
    }

    private void Slip()
    {
        // 수평 축 입력 없이, 일정 속도 이상 달리는 중, W키를 뗐다면 슬라이딩 애니메이션 트리거 작동
        if ((Mathf.Abs(horizontal) < 0.1f) && (vertical > 1.4f) &&  Input.GetKeyUp(KeyCode.W))
        {
            // 살짝 미끄러지기, 발 끄는 사운드 재생
            playerAnimator.SetTrigger("Slip");
            // Debug.Log("Slip");
        }
    }

    private void Slide()
    {
        if ((Mathf.Abs(horizontal) < 0.1f) && (vertical > 1.4f) && Input.GetKeyDown(KeyCode.C))
        {
            // 정면으로 슬라이딩
            // playerAnimator.SetTrigger("Slide");
            // Debug.Log("Slip");
        }

    }
}
