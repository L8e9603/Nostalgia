using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class FullBodyInputManager : MonoBehaviourPun
{
    #region SERIALIZE FIELDS
    #endregion

    #region FIELDS
    [Header("Player Input List")]
    public string moveHorizontalName = "Horizontal";
    public string moveVerticalName = "Vertical";
    public string moveHorizontalFloatName = "Horizontal";
    public string moveVerticalFloatName = "Vertical";
    public string sprintButtonName = "Sprint";
    public string crouchButtonName = "Crouch";
    public string proneButtonName = "Prone";
    public string jumpButtonName = "Jump";
    public string interactButtonName = "Interact";
    public string aimButtonName = "Aim";
    public string fireButtonName = "Fire1";
    public string reloadButtonName = "Reload";
    public string punchButtonName = "Fire1";
    public string weapon1ButtonName = "Weapon1";
    public string weapon2ButtonName = "Weapon2";
    public string weapon3ButtonName = "Weapon3";
    public string weapon4ButtonName = "Weapon4";
    public string weapon5ButtonName = "Weapon5";

    private FullBodyCameraSetUp fullBodyCameraSetUp;

    [Header("Camera Axis Input")]
    private string m_verticalLookAxisName = "Mouse Y";
    private string m_horizontalLookAxisName = "Mouse X";
    [SerializeField]
    private float m_xAxisSensitivity = 0.2f;
    [SerializeField]
    private float m_yAxisSensitivity = 0.2f;

    #endregion
    #region PROPERTIES
    // 값 할당은 내부에서만 가능
    public float moveHorizontal { get; private set; }
    public float moveVertical { get; private set; }
    public float moveHorizontalFloat { get; private set; }
    public float moveVerticalFloat { get; private set; }
    public bool sprint { get; private set; }
    public bool crouch { get; private set; }
    public bool prone { get; private set; }
    public bool jump { get; private set; }
    public bool interact { get; private set; }
    public bool aim { get; private set; }
    public bool fire { get; private set; }
    public bool reload { get; private set; }
    public bool punch { get; private set; }
    public bool weapon1 { get; private set; }
    public bool weapon2 { get; private set; }
    public bool weapon3 { get; private set; }
    public bool weapon4 { get; private set; }
    public bool weapon5 { get; private set; }
    
    public float XLookAxis { get; private set; }
    public float YLookAxis { get; private set; }
    #endregion

    #region UNITY
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        fullBodyCameraSetUp = GetComponent<FullBodyCameraSetUp>();
    }

    void Update()
    {
        // 로컬 플레이어가 아닌 경우 입력을 받지 않음
        if (!photonView.IsMine)
        {
            return;
        }

        XLookAxis = Input.GetAxis(m_horizontalLookAxisName) * m_xAxisSensitivity;
        YLookAxis = Input.GetAxis(m_verticalLookAxisName) * m_yAxisSensitivity;

        moveHorizontal = Input.GetAxisRaw(moveHorizontalName);
        moveVertical = Input.GetAxisRaw(moveVerticalName);

        // 애니메이션 파라미터로 쓰기위해 따로 감지
        moveHorizontalFloat = Input.GetAxis(moveHorizontalFloatName);
        moveVerticalFloat = Input.GetAxis(moveVerticalFloatName);

        // 조준 상태가 아닐때만 전력질주 입력 가능으로 수정
        sprint = Input.GetButton(sprintButtonName);
        crouch = Input.GetButtonDown(crouchButtonName);
        prone = Input.GetButtonDown(proneButtonName);
        jump = Input.GetButtonDown(jumpButtonName);
        interact = Input.GetButtonDown(interactButtonName);
        aim = Input.GetButton(aimButtonName);
        fire = Input.GetButton(fireButtonName);
        reload = Input.GetButtonDown(reloadButtonName);
        punch = Input.GetButtonDown(punchButtonName);

        weapon1 = Input.GetButtonDown(weapon1ButtonName);
        weapon2 = Input.GetButtonDown(weapon2ButtonName);
        weapon3 = Input.GetButtonDown(weapon3ButtonName);
        weapon4 = Input.GetButtonDown(weapon4ButtonName);
        weapon5 = Input.GetButtonDown(weapon5ButtonName);
    }
    #endregion
}
