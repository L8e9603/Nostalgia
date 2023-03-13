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
        Invoke("DestroySelf", 5f); // 5초 뒤 자동 파괴
    }

    private void Update()
    {
    }

    private void OnTriggerEnter(Collider other)
    {
        Instantiate(m_dustParticle, hitPosition, Quaternion.LookRotation(hitNormal));

        // 데미지 처리
        hitObject = other.GetComponent<IDamageable>(); // 충돌한 상대방으로부터 IDamageable 컴포넌트를 가져오기 시도

        if (hitObject != null) // 상대방으로 부터 IDamageable 컴포넌트를 가져오는데 성공했다면
        {
            //hitObject.OnDamage(bulletDamage, hitPosition, hitNormal); // 상대방의 OnDamage 함수를 실행시켜서 상대방에게 데미지 주기
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
        Instantiate(m_bloodParticle, hitPosition + hitNormal * 0.05f, Quaternion.LookRotation(hitNormal)); // hitNormal 방향으로 조금 띄워 파티클이 파묻히는 현상 방지
    }

    IEnumerator BulletProcess(GameObject hitObject, Vector3 hitPosition, Vector3 hitNormal, Rigidbody hitObjectRigidbody, IDamageable idamageable)
    {
        // 데미지 실행
        if (idamageable != null)
        {
            idamageable.OnDamage(bulletDamage);
        }

        // 피탄효과 재생
        BulletHitSFX(hitPosition, hitNormal);

        // 총알 오브젝트 파괴
        DestroySelf();

        yield return null;
    }
}
