using UnityEngine;
using Photon.Pun;

// �÷��̾� ĳ������ ����ü�μ��� ������ ���
public class PlayerHealth : HealthSystem
{
    public AudioClip deathClip; // ��� �Ҹ�
    public AudioClip hitClip; // �ǰ� �Ҹ�

    private AudioSource m_playerAudioPlayer; // �÷��̾� �Ҹ� �����
    private Animator m_playerAnimator; // �÷��̾��� �ִϸ�����

    private MFBLocomotion m_playerLocomotion; // �÷��̾� ������ ������Ʈ

    private void Awake()
    {
        // ����� ������Ʈ�� ��������
        m_playerAnimator = GetComponent<Animator>();
        m_playerAudioPlayer = GetComponent<AudioSource>();

        m_playerLocomotion = GetComponent<MFBLocomotion>();
    }

    protected override void OnEnable()
    {
        // LivingEntity�� OnEnable() ���� (���� �ʱ�ȭ)
        base.OnEnable();

        // �÷��̾� ������ �޴� ������Ʈ�� Ȱ��ȭ
        m_playerLocomotion.enabled = true;
    }

    // ü�� ȸ��
    [PunRPC]
    public override void RestoreHealth(float newHealth)
    {
        // LivingEntity�� RestoreHealth() ���� (ü�� ����)
        base.RestoreHealth(newHealth);
    }


    /// <summary>
    /// OnDamage() �޼��� �������̵�, ��� ȿ���� �߰�
    /// </summary>
    /// <param name="damage"></param>
    /// <param name="hitPoint"></param>
    /// <param name="hitDirection"></param>
    [PunRPC]
    public override void OnDamage(float damage)
    {
        base.OnDamage(damage); // LivingEntity�� OnDamage() ����(������ ����)

        if (!dead)
        {
            // ������� ���� ��쿡�� ȿ������ ���
            m_playerAudioPlayer.PlayOneShot(hitClip);
        }
    }

    public override void Die()
    {
        // LivingEntity�� Die() ����(��� ����)
        base.Die();

        // ����� ���
        m_playerAudioPlayer.PlayOneShot(deathClip);

        // �ִϸ������� Die Ʈ���Ÿ� �ߵ����� ��� �ִϸ��̼� ���
        m_playerAnimator.SetTrigger("Die");

        // �÷��̾� ������ �޴� ������Ʈ ��Ȱ��ȭ
        m_playerLocomotion.enabled = false;

        // 5�� �ڿ� ������
        Invoke("Respawn", 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        // �����۰� �浹�� ��� �ش� �������� ����ϴ� ó��
        // ������� ���� ��쿡�� ������ ��밡��
        if (!dead)
        {
            // �浹�� �������� ���� Item ������Ʈ�� �������� �õ�
            IItem item = other.GetComponent<IItem>();

            // �浹�� �������κ��� Item ������Ʈ�� �������µ� �����ߴٸ�
            if (item != null)
            {
                // ȣ��Ʈ�� ������ ���� ��� ����
                // ȣ��Ʈ������ �������� ��� ��, ���� �������� ȿ���� ��� Ŭ���̾�Ʈ�鿡�� ����ȭ��Ŵ
                if (PhotonNetwork.IsMasterClient)
                {
                    // Use �޼��带 �����Ͽ� ������ ���
                    item.Use(gameObject);
                }
            }
        }
    }

    // ��Ȱ ó��
    public void Respawn()
    {
        // ���� �÷��̾ ���� ��ġ�� ���� ����
        if (photonView.IsMine)
        {
            // �������� �ݰ� 5���� ������ ������ ��ġ ����
            Vector3 randomSpawnPos = Random.insideUnitSphere * 5f;
            // ���� ��ġ�� y���� 0���� ����
            randomSpawnPos.y = 0f;

            // ������ ���� ��ġ�� �̵�
            transform.position = randomSpawnPos;
        }

        // ������Ʈ���� �����ϱ� ���� ���� ������Ʈ�� ��� ���ٰ� �ٽ� �ѱ�
        // ������Ʈ���� OnDisable(), OnEnable() �޼��尡 �����
        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
}