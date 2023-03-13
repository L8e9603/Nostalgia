using System.Collections;
using UnityEngine;
using Photon.Pun;

public class BulletProjectile : MonoBehaviourPun
{
    [SerializeField] private ParticleSystem m_bloodParticle;
    [SerializeField] private ParticleSystem m_dustParticle;

    private Rigidbody bulletRigidbody;
    public IDamageable hitObject;
    public Vector3 hitPosition;  
    public Vector3 hitNormal;  

    public float bulletDamage;
    public float bulletSpeed;

    private void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        DetectHitPosition();
        float speed = 100f;
        bulletRigidbody.velocity = transform.forward * speed;
        Invoke("DestroySelf", 5f); // 5�� �� �ڵ� �ı�
    }

    private void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        Instantiate(m_dustParticle, hitPosition, Quaternion.LookRotation(hitNormal));

        // ������ ó��
        hitObject = other.GetComponent<IDamageable>(); // �浹�� �������κ��� IDamageable ������Ʈ�� �������� �õ�

        if (hitObject != null) // �������� ���� IDamageable ������Ʈ�� �������µ� �����ߴٸ�
        {
            //hitObject.OnDamage(bulletDamage, hitPosition, hitNormal); // ������ OnDamage �Լ��� ������Ѽ� ���濡�� ������ �ֱ�
            photonView.RPC("DamageProcessOnServer", RpcTarget.MasterClient);
            Instantiate(m_bloodParticle, hitPosition, Quaternion.LookRotation(hitNormal));
        }
    }

    [PunRPC]
    private void DamageProcessOnServer()
    {
        Debug.Log("HitProcessOnServer()");
        hitObject.OnDamage(bulletDamage);
    }

    private void DetectHitPosition()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
        {
            hitPosition = hit.point;
            hitNormal = hit.normal;
        }
    }
    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void BulletHitSFX(Vector3 hitPosition, Vector3 hitNormal)
    {
        Instantiate(m_bloodParticle, hitPosition + hitNormal * 0.05f, Quaternion.LookRotation(hitNormal)); // hitNormal �������� ���� ��� ��ƼŬ�� �Ĺ����� ���� ����
    }

    IEnumerator BulletProcess(GameObject hitObject, Vector3 hitPosition, Vector3 hitNormal, Rigidbody hitObjectRigidbody, IDamageable idamageable)
    {
        // ������ ����
        if (idamageable != null)
        {
            idamageable.OnDamage(bulletDamage);
        }

        // ��źȿ�� ���
        BulletHitSFX(hitPosition, hitNormal);

        // �Ѿ� ������Ʈ �ı�
        DestroySelf();

        yield return null;
    }
}
