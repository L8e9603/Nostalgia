using System;
using UnityEngine;
using Photon.Pun;

// ����ü�μ� ������ ���� ������Ʈ���� ���� ���븦 ����
// ü��, ������ �޾Ƶ��̱�, ��� ���, ��� �̺�Ʈ�� ����
public class HealthSystem : MonoBehaviourPun, IDamageable
{
    public float startingHealth = 100f; // ���� ü��
    public float health { get; protected set; } // ���� ü��
    public float stamina { get; protected set; } // ���� ü��
    public float hunger { get; protected set; } // ���� ü��
    public float thirsty { get; protected set; } // ���� ü��
    public bool dead { get; protected set; } // ��� ����, protected�� ��� �ڽ� Ŭ���������� ���� ����
    public event Action onDeath; // ����� �ߵ��� �̺�Ʈ, Action Ÿ�� : ������� ���� �޼ҵ带 ����ų �� �ִ� ��������Ʈ(�븮��), �Ű����� ���� �޼ҵ带 �ڽſ��� ����صΰ� ����� ȣ�� ������ Ÿ��

    // ȣ��Ʈ->��� Ŭ���̾�Ʈ �������� ü�°� ��� ���¸� ����ȭ �ϴ� �޼���
    [PunRPC]
    public void ApplyUpdatedHealth(float newHealth, bool newDead)
    {
        health = newHealth;
        dead = newDead;
    }


    // ����ü�� Ȱ��ȭ�ɶ� ���¸� ����
    protected virtual void OnEnable()
    {
        // ������� ���� ���·� ����
        dead = false;
        // ü���� ���� ü������ �ʱ�ȭ
        health = startingHealth;
    }

    // ������ ó��
    // ȣ��Ʈ���� ���� �ܵ� ����ǰ�, ȣ��Ʈ�� ���� �ٸ� Ŭ���̾�Ʈ�鿡�� �ϰ� �����
    [PunRPC]
    public virtual void OnDamage(float damage)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // ��������ŭ ü�� ����
            health -= damage;

            // ȣ��Ʈ���� Ŭ���̾�Ʈ�� ����ȭ
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, health, dead);

            // �ٸ� Ŭ���̾�Ʈ�鵵 OnDamage�� �����ϵ��� ��
            photonView.RPC("OnDamage", RpcTarget.Others, damage);
        }

        // ü���� 0 ���� && ���� ���� �ʾҴٸ� ��� ó�� ����
        if (health <= 0 && !dead)
        {
            Die();
        }
    }

    // ü���� ȸ���ϴ� ���
    [PunRPC]
    public virtual void RestoreHealth(float newHealth)
    {
        if (dead)
        {
            // �̹� ����� ��� ü���� ȸ���� �� ����
            return;
        }

        // ȣ��Ʈ�� ü���� ���� ���� ����
        if (PhotonNetwork.IsMasterClient)
        {
            // ü�� �߰�
            health += newHealth;
            // �������� Ŭ���̾�Ʈ�� ����ȭ
            photonView.RPC("ApplyUpdatedHealth", RpcTarget.Others, health, dead);

            // �ٸ� Ŭ���̾�Ʈ�鵵 RestoreHealth�� �����ϵ��� ��
            photonView.RPC("RestoreHealth", RpcTarget.Others, newHealth);
        }
    }

    // ��� ó��
    public virtual void Die()
    {
        Debug.Log("Die()");
        // onDeath �̺�Ʈ�� ��ϵ� �޼��尡 �ִٸ� ����
        if (onDeath != null)
        {
            onDeath();
        }

        // ��� ���¸� ������ ����
        dead = true;
    }
}