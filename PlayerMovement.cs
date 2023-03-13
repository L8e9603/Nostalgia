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

        // ���� �̵���
        if (playerInput.moveHorizontal > 0) // A
        {
            horizontal = Mathf.Lerp(horizontal, 1f, Time.deltaTime * parameterChangeSpeed);
        }
        else if (playerInput.moveHorizontal < 0) // D
        {
            horizontal = Mathf.Lerp(horizontal, -1f, Time.deltaTime * parameterChangeSpeed);
        }
        
        // ���� �̵���
        if (playerInput.moveVertical > 0) // W
        {
            // W + LShift (�������ֽ�)
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

    // �� �Է��� �����ϰ�, �÷��̾��� �̵��ӵ��� �����ϴ� �޼���
    private void PlayerMoveSpeed()
    {
        if (!groundCheck.isGrounded)
        {
            return;
        }

        // ���ִ� ����
        if (isStanding)
        {
            // �� ������ moveSpeed ����
            if (playerInput.moveHorizontal != 0f || playerInput.moveVertical != 0f)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, defaultMoveSpeed, Time.deltaTime * 5f); // �⺻ �ӵ��� ����

                // �������� �̵��ϴ� ��, ������ ���°� �ƴҶ�, �������� Ű �Է��� �����Ǹ�
                if (playerInput.moveVerticalFloat >= 1f && !thirdPersonShooterController.isAiming && playerInput.sprint)
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, sprintMoveSpeed, Time.deltaTime * 3f); // �޸��� �ӵ��� ����
                    isSprinting = true;
                }
                else
                {
                    moveSpeed = Mathf.Lerp(moveSpeed, defaultMoveSpeed, Time.deltaTime * 3f); // �⺻ �ӵ��� ����
                    isSprinting = false;
                }
            }
            // �� ������ ���� �ʴ´ٸ�
            else if (playerInput.moveHorizontal == 0f || playerInput.moveVertical == 0f)
            {
                moveSpeed = Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * 5f); // 0���� ����
                isSprinting = false;
            }
            else
            {
                moveSpeed = Mathf.Lerp(moveSpeed, 0f, Time.deltaTime * 5f); // 0���� ����
            }
        }

        // ���� ����
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

    // Rigidbody�� �̿��� �÷��̾��� ��ġ�� �����Ű�� �޼���
    private void PlayerMovePosition()
    {
        if (!isJumping)
        {
            // ���� �̵� ����, ���� �̵����� ���� -> moveDirection = ī�޶� ���� ���� ���� ���� + ī�޶� ���� ���� ���� ����
            Vector3 rightDirection = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;
            Vector3 forwardDirection = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
            moveDirection = playerInput.moveHorizontal * rightDirection + playerInput.moveVertical * forwardDirection;
        }

        // Rigidbody�� �̿��� ���ӿ�����Ʈ ��ġ ����, FixedUpdate���� ���� �� Time.deltaTime�� Time.fixedDeltaTime���� �ڵ� ��ȯ
        // �밢�� �̵��� �� ���� �̵��ϴ� ���� �����ϱ� ���� ClampMagnitude() �޼���� ������ ���밪 ����
        //playerRigidbody.MovePosition(playerRigidbody.position + Vector3.ClampMagnitude(moveDirection, 1f) * moveSpeed * Time.deltaTime);
        playerRigidbody.velocity = Vector3.ClampMagnitude(moveDirection, 1f) * moveSpeed;
        playerAnimator.SetFloat("MoveSpeed", moveSpeed);
    }

    // ����������� �÷��̾ ȸ����Ű�� �޼���
    private void PlayerRotate()
    {
        if (thirdPersonShooterController.isAiming)
        {
            return;
        }

        // �� �Է� ����
        Vector2 moveInput = new Vector2(playerInput.moveHorizontal, playerInput.moveVertical);
        bool isMove = moveInput.magnitude != 0;

/*        if (!isJumping && isMove) // ���� ���°� �ƴϰ�, �� ������ �� �Է��� �����Ǹ� �Էµ� �������� �÷��̾� ȸ��
        {
            // ȸ�� ���� ���
            Vector3 lookRight = new Vector3(Camera.main.transform.right.x, 0f, Camera.main.transform.right.z).normalized;
            Vector3 lookForward = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z).normalized;
            // rotateDirection = lookRight * moveInput.x + lookForward * moveInput.y;
            rotateDirection = playerInput.moveHorizontal * lookRight + playerInput.moveVertical * lookForward;

            Quaternion rotation = Quaternion.LookRotation(rotateDirection);


            // ���� �ٶ󺸴� ������ ��ȣ != ���ư� ������ ��ȣ, �̸� ȸ���� ���� �����༭ �� �ݴ� �������� �̵��Ҷ� ���ڿ������� ���� ����
            if (Mathf.Sign(transform.forward.x) != Mathf.Sign(moveDirection.x) || Mathf.Sign(transform.forward.z) != Mathf.Sign(moveDirection.z))
            {
                // transform.Rotate(0f, 1f, 0f);
                // transform.forward = Vector3.Lerp(transform.forward, rotateDirection, rotateSpeed * Time.deltaTime * 1.5f); // ȸ�� ����
                // playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * rotateSpeed);
            }

            //transform.forward = Vector3.Lerp(transform.forward, rotateDirection, rotateSpeed * Time.deltaTime);
            playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * rotateSpeed);
        }
*/

        if (!isJumping && isMove) // ���� ���°� �ƴϰ�, �� ������ �� �Է��� �����Ǹ� �Էµ� �������� �÷��̾� ȸ��
        {
            Vector3 aimDirection = Vector3.zero;
            aimDirection.x = Camera.main.transform.forward.x;
            aimDirection.z = Camera.main.transform.forward.z;
            // transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 17f);

            Quaternion rotation = Quaternion.LookRotation(aimDirection);
            // playerRigidbody.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * rotateSpeed); // ������� �ִϸ��̼� ���� ���� ���� �ּ�ó���ϱ�
            transform.rotation = Quaternion.Slerp(playerRigidbody.rotation, rotation, Time.deltaTime * rotateSpeed); // ���� ��� �ִϸ��̼� ���� ���� ���� ����ϱ�
        }
    }

    // ���� �ִϸ��̼� Ŭ������ ����Ǵ� �޼���
    private void PlayerRotateJump()
    {
        if(moveDirection != Vector3.zero)
        {
            transform.forward = moveDirection;
        }
    }

    // �ɱ� �޼���
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

    // ���帮�� �޼���
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

    // ���� �޼���
    private void PlayerJump()
    {
        // �ɱ�, ���帮��, ���� ���¶�� ���� �Ұ���
        if (isCrouching || isProning || thirdPersonShooterController.isAiming)
        {
            return;
        }
        
        // �� ������ && ���� Ű�� ������ �� && ���� ��Ÿ���� ��������
        if (groundCheck.isGrounded && playerInput.jump && (Time.time >= lastJumpTime + jumpCoolDown) )
        {
            lastJumpTime = Time.time; // ������ ���� �ð� ����
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

    // Land �ִϸ��̼� Ŭ������ ����Ǵ� �޼���
    private void Land()
    {
        isJumping = false;
        groundCheck.isGrounded = true;
    }

    private void Slip()
    {
        // ���� �� �Է� ����, ���� �ӵ� �̻� �޸��� ��, WŰ�� �ôٸ� �����̵� �ִϸ��̼� Ʈ���� �۵�
        if ((Mathf.Abs(horizontal) < 0.1f) && (vertical > 1.4f) &&  Input.GetKeyUp(KeyCode.W))
        {
            // ��¦ �̲�������, �� ���� ���� ���
            playerAnimator.SetTrigger("Slip");
            // Debug.Log("Slip");
        }
    }

    private void Slide()
    {
        if ((Mathf.Abs(horizontal) < 0.1f) && (vertical > 1.4f) && Input.GetKeyDown(KeyCode.C))
        {
            // �������� �����̵�
            // playerAnimator.SetTrigger("Slide");
            // Debug.Log("Slip");
        }

    }
}
