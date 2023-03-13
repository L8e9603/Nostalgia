using System.Collections;
using UnityEngine;

// 총을 구현한다
// 총구 방향으로부터 모니터 중앙 좌표에 Ray 발사, hit 좌표를 상체가 바라보도록 조정
public class Gun : MonoBehaviour, IItem
{
    private GunData.GunType gunType; // 총기 유형
    public enum Type { M4, AK47 } // Gun 스크립트가 적용된 총기의 무기 타입
    public Type gun;
    public int index; // 총기 인덱스
    public Vector3 hitPosition;  // 총알이 맞은 지점, bulletProjectile.cs 로 넘겨줌
    public Vector3 hitNormal;  // 총알이 맞은 방향, bulletProjectile.cs 로 넘겨줌

    // 총알 투사체
    [SerializeField] private Transform bulletProjectile;


    // 총의 상태를 표현하는데 사용할 타입 선언
    public enum State
    {
        ReadyToFire, // 발사 준비됨
        Shooting, // 발사중
        MagazineEmpty, // 탄창이 빔
        Reloading, // 재장전 중
        ReloadingDone // 재장전 완료
    }

    public State state { get; set; } // 현재 총의 상태


    public Transform firePositionTransform; // 총알이 발사될 위치

    public Light muzzleFlash; // 총구 화염 광원
    public ParticleSystem muzzleFlashParticleSystem; // 총구 화염 파티클 시스템
    public ParticleSystem shellEjectParticleSystem; // 탄피 배출 파티클 시스템

    [Header("AudioSource Channel")]
    public AudioSource gunAudioPlayer1; // 총 소리 재생기 채널1 볼륨1, 총성 재생
    public AudioSource gunAudioPlayer2; // 총 소리 재생기 채널2 볼륨1, 공간 울림 재생
    public AudioSource gunAudioPlayer3; // 총 소리 재생기 채널3 탄피 소리 등 작은 소리용

    public GunData gunData; // 총의 현재 데이터, 데미지, 장탄수 등, 스크립터블 오브젝트

    private float fireDistance = Mathf.Infinity; // 사정거리

    public int ammoRemain = 100; // 남은 전체 탄약
    public int magAmmo; // 현재 탄창에 남아있는 탄약

    private float lastFireTime; // 총을 마지막으로 발사한 시점


    public ThirdPersonShooterController thirdPersonShooterController;
    public Transform leftHandIKTransform; // OnEnable()에서 ThirdPersonShooterController.cs로 넘겨줌
    public Transform rightHandIKTransform;

    private void Start()
    {
    }

    private void OnEnable()
    {
        // 전체 예비 탄약 양을 초기화
        //ammoRemain = gunData.startAmmoRemain;
        // 현재 탄창을 가득채우기
        //magAmmo = gunData.magCapacity;
        // 총의 현재 상태를 총을 쏠 준비가 된 상태로 변경
        state = State.ReadyToFire;
        // 마지막으로 총을 쏜 시점을 초기화
        lastFireTime = 0;

        // 무기 변경시 양손 IK 위치 변경을 위해 넘겨줌
        thirdPersonShooterController.GetGunIKPosition(leftHandIKTransform, rightHandIKTransform);
/*        thirdPersonShooterController.leftHandIKTransform = this.leftHandIKTransform;
        thirdPersonShooterController.rightHandIKTransform = this.rightHandIKTransform;
*/    }

    private void Update()
    {
        // 총기 변경되면 변경된 총기의 컴포넌트 초기화
        // GunData에서 총기 유형 가져오기, 총기 유형에 따른 재장전 소리 다양화
        //gunType = gunData.gunType;

        /*        gunAudioPlayer1 = GameObject.Find("GunAudioSFX1").GetComponent<AudioSource>();
                gunAudioPlayer2 = GameObject.Find("GunAudioSFX2").GetComponent<AudioSource>();
                gunAudioPlayer3 = GameObject.Find("GunAudioSFX3").GetComponent<AudioSource>();*/
    }

    // 발사 시도
    public void Fire()
    {
        // 현재 상태가 발사 가능한 상태
        // && 마지막 총 발사 시점에서 timeBetFire 이상의 시간이 지남
        if (state == State.ReadyToFire && Time.time >= lastFireTime + gunData.timeBetFire)
        {
            // 마지막 총 발사 시점을 갱신
            lastFireTime = Time.time;
            // 실제 발사 처리 실행
            Shot();
        }
    }

    // 실제 발사 처리, 총알 궤적 렌더링, 반동으로 인한 카메라 쉐이킹 연출 진행
    private void Shot()
    {
        // 레이캐스트에 의한 충돌 정보를 저장하는 컨테이너
        RaycastHit hit;

        // 레이캐스트(시작지점, 방향, 충돌 정보 컨테이너, 사정거리)
        // 총구에서 레이 
        // Bullet 레이어 제외, 고 RPM 총기류 사용시 바로 앞쪽 총알 프리팹의 콜라이더에 충돌한 위치를 hitPosition로 저장하지 않게 하기 위한 레이어 마스크
        int layerMask = (-1) - (1 << LayerMask.NameToLayer("Bullet"));
        if (Physics.Raycast(firePositionTransform.position, firePositionTransform.forward, out hit, Mathf.Infinity, layerMask))
        {
            // 레이가 어떤 물체와 충돌한 경우
            // 레이가 충돌한 위치 저장
            hitPosition = hit.point;
            hitNormal = hit.normal;

            // 총알이 날아갈 방향
            Vector3 bulletDir = (hitPosition - firePositionTransform.position).normalized;
            // 투사체 발사
            Instantiate(bulletProjectile.transform, firePositionTransform.position, Quaternion.LookRotation(bulletDir, Vector3.up));

            /*            // 충돌한 상대방으로부터 IDamageable 오브젝트를 가져오기 시도
                        IDamageable target =
                            hit.collider.GetComponent<IDamageable>();

                        // 상대방으로 부터 IDamageable 오브젝트를 가져오는데 성공했다면
                        if (target != null)
                        {
                            // 상대방의 OnDamage 함수를 실행시켜서 상대방에게 데미지 주기
                            target.OnDamage(gunData.damage, hit.point, hit.normal);
                        }
            */
        }
        else
        {
            // 레이가 다른 물체와 충돌하지 않았다면
            // 총알이 최대 사정거리까지 날아갔을때의 위치를 충돌 위치로 사용
            hitPosition = firePositionTransform.position +
                          firePositionTransform.forward * fireDistance;
        }

        // 발사 이펙트 재생 시작
        StartCoroutine(ShotEffect(hitPosition));

        // 남은 탄환의 수를 -1
        magAmmo--;
        if (magAmmo <= 0)
        {
            // 탄창에 남은 탄약이 없다면, 총의 현재 상태를 Empty으로 갱신
            state = State.MagazineEmpty;
        }

        // 카메라 쉐이킹
        //CinemachineShake.Instance.ShakeCamera(gunData.recoilStrength, .1f); // 총기 반동 세기 만큼 카메라 흔들기
    }

    // 발사 이펙트와 소리를 재생하고 총알 궤적을 그린다
    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        // 총구 화염 효과 재생
        muzzleFlashParticleSystem.Play();
        // 탄피 배출 효과 재생
        shellEjectParticleSystem.playbackSpeed = 4f;
        shellEjectParticleSystem.Play();

        // 총격 소리 재생
        gunAudioPlayer1.PlayOneShot(gunData.shotClip); // 총성
        gunAudioPlayer2.clip = gunData.shotTailClip; // 공간 울림 클립 삽입, 러닝타임 길어서 플레이원샷은 끊김
        gunAudioPlayer2.Play(); // 공간 울림 재생
        Invoke("ShellDropSFX", 1f); // 탄피가 땅에 떨어질 시간이 되면 탄피소리 재생, 채널3에서 재생됨

        /*        // 선의 시작점은 총구의 위치
                bulletLineRenderer.SetPosition(0, fireTransform.position);
                // 선의 끝점은 입력으로 들어온 충돌 위치
                bulletLineRenderer.SetPosition(1, hitPosition);
                // 라인 렌더러를 활성화하여 총알 궤적을 그린다
                bulletLineRenderer.enabled = true;

                // 0.03초 동안 잠시 처리를 대기
                yield return new WaitForSeconds(0.03f);

                // 라인 렌더러를 비활성화하여 총알 궤적을 지운다
                bulletLineRenderer.enabled = false;*/

        // 0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);
    }

    // 재장전 시도
    public bool Reload()
    {
        if (state == State.Reloading ||
            ammoRemain <= 0 || magAmmo >= gunData.magCapacity)
        {
            // 이미 재장전 중이거나, 남은 총알이 없거나
            // 탄창에 총알이 이미 가득한 경우 재장전 할수 없다
            return false;
        }

        // 재장전 처리 시작
        StartCoroutine(ReloadRoutine());
        return true;
    }

    // 실제 재장전 처리를 진행
    private IEnumerator ReloadRoutine()
    {
        {
            // 현재 상태를 재장전 중 상태로 전환
            state = State.Reloading;

            // 재장전 소리 재생
            gunAudioPlayer1.PlayOneShot(gunData.reloadClip);

            // 재장전 소요 시간 만큼 처리를 쉬기
            yield return new WaitForSeconds(gunData.reloadTime);

            // 탄창에 채울 탄약을 계산한다
            int ammoToFill = gunData.magCapacity - magAmmo;


            // 탄창에 채워야할 탄약이 남은 탄약보다 많다면,
            // 채워야할 탄약 수를 남은 탄약 수에 맞춰 줄인다
            if (ammoRemain < ammoToFill)
            {
                ammoToFill = ammoRemain;
            }

            // 탄창을 채운다
            magAmmo += ammoToFill;
            // 남은 탄약에서, 탄창에 채운만큼 탄약을 뺸다
            ammoRemain -= ammoToFill;

            // 총의 현재 상태를 발사 준비된 상태로 변경
            state = State.ReloadingDone;
        }
    }

    public void ShellDropSFX()
    {
        gunAudioPlayer3.PlayOneShot(gunData.shellDropClip); // 탄피 효과음
    }

    // 인터페이스
    public void Use(GameObject target)
    {

    }
}