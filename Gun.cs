using System.Collections;
using UnityEngine;

// ���� �����Ѵ�
// �ѱ� �������κ��� ����� �߾� ��ǥ�� Ray �߻�, hit ��ǥ�� ��ü�� �ٶ󺸵��� ����
public class Gun : MonoBehaviour, IItem
{
    private GunData.GunType gunType; // �ѱ� ����
    public enum Type { M4, AK47 } // Gun ��ũ��Ʈ�� ����� �ѱ��� ���� Ÿ��
    public Type gun;
    public int index; // �ѱ� �ε���
    public Vector3 hitPosition;  // �Ѿ��� ���� ����, bulletProjectile.cs �� �Ѱ���
    public Vector3 hitNormal;  // �Ѿ��� ���� ����, bulletProjectile.cs �� �Ѱ���

    // �Ѿ� ����ü
    [SerializeField] private Transform bulletProjectile;


    // ���� ���¸� ǥ���ϴµ� ����� Ÿ�� ����
    public enum State
    {
        ReadyToFire, // �߻� �غ��
        Shooting, // �߻���
        MagazineEmpty, // źâ�� ��
        Reloading, // ������ ��
        ReloadingDone // ������ �Ϸ�
    }

    public State state { get; set; } // ���� ���� ����


    public Transform firePositionTransform; // �Ѿ��� �߻�� ��ġ

    public Light muzzleFlash; // �ѱ� ȭ�� ����
    public ParticleSystem muzzleFlashParticleSystem; // �ѱ� ȭ�� ��ƼŬ �ý���
    public ParticleSystem shellEjectParticleSystem; // ź�� ���� ��ƼŬ �ý���

    [Header("AudioSource Channel")]
    public AudioSource gunAudioPlayer1; // �� �Ҹ� ����� ä��1 ����1, �Ѽ� ���
    public AudioSource gunAudioPlayer2; // �� �Ҹ� ����� ä��2 ����1, ���� �︲ ���
    public AudioSource gunAudioPlayer3; // �� �Ҹ� ����� ä��3 ź�� �Ҹ� �� ���� �Ҹ���

    public GunData gunData; // ���� ���� ������, ������, ��ź�� ��, ��ũ���ͺ� ������Ʈ

    private float fireDistance = Mathf.Infinity; // �����Ÿ�

    public int ammoRemain = 100; // ���� ��ü ź��
    public int magAmmo; // ���� źâ�� �����ִ� ź��

    private float lastFireTime; // ���� ���������� �߻��� ����


    public ThirdPersonShooterController thirdPersonShooterController;
    public Transform leftHandIKTransform; // OnEnable()���� ThirdPersonShooterController.cs�� �Ѱ���
    public Transform rightHandIKTransform;

    private void Start()
    {
    }

    private void OnEnable()
    {
        // ��ü ���� ź�� ���� �ʱ�ȭ
        //ammoRemain = gunData.startAmmoRemain;
        // ���� źâ�� ����ä���
        //magAmmo = gunData.magCapacity;
        // ���� ���� ���¸� ���� �� �غ� �� ���·� ����
        state = State.ReadyToFire;
        // ���������� ���� �� ������ �ʱ�ȭ
        lastFireTime = 0;

        // ���� ����� ��� IK ��ġ ������ ���� �Ѱ���
        thirdPersonShooterController.GetGunIKPosition(leftHandIKTransform, rightHandIKTransform);
/*        thirdPersonShooterController.leftHandIKTransform = this.leftHandIKTransform;
        thirdPersonShooterController.rightHandIKTransform = this.rightHandIKTransform;
*/    }

    private void Update()
    {
        // �ѱ� ����Ǹ� ����� �ѱ��� ������Ʈ �ʱ�ȭ
        // GunData���� �ѱ� ���� ��������, �ѱ� ������ ���� ������ �Ҹ� �پ�ȭ
        //gunType = gunData.gunType;

        /*        gunAudioPlayer1 = GameObject.Find("GunAudioSFX1").GetComponent<AudioSource>();
                gunAudioPlayer2 = GameObject.Find("GunAudioSFX2").GetComponent<AudioSource>();
                gunAudioPlayer3 = GameObject.Find("GunAudioSFX3").GetComponent<AudioSource>();*/
    }

    // �߻� �õ�
    public void Fire()
    {
        // ���� ���°� �߻� ������ ����
        // && ������ �� �߻� �������� timeBetFire �̻��� �ð��� ����
        if (state == State.ReadyToFire && Time.time >= lastFireTime + gunData.timeBetFire)
        {
            // ������ �� �߻� ������ ����
            lastFireTime = Time.time;
            // ���� �߻� ó�� ����
            Shot();
        }
    }

    // ���� �߻� ó��, �Ѿ� ���� ������, �ݵ����� ���� ī�޶� ����ŷ ���� ����
    private void Shot()
    {
        // ����ĳ��Ʈ�� ���� �浹 ������ �����ϴ� �����̳�
        RaycastHit hit;

        // ����ĳ��Ʈ(��������, ����, �浹 ���� �����̳�, �����Ÿ�)
        // �ѱ����� ���� 
        // Bullet ���̾� ����, �� RPM �ѱ�� ���� �ٷ� ���� �Ѿ� �������� �ݶ��̴��� �浹�� ��ġ�� hitPosition�� �������� �ʰ� �ϱ� ���� ���̾� ����ũ
        int layerMask = (-1) - (1 << LayerMask.NameToLayer("Bullet"));
        if (Physics.Raycast(firePositionTransform.position, firePositionTransform.forward, out hit, Mathf.Infinity, layerMask))
        {
            // ���̰� � ��ü�� �浹�� ���
            // ���̰� �浹�� ��ġ ����
            hitPosition = hit.point;
            hitNormal = hit.normal;

            // �Ѿ��� ���ư� ����
            Vector3 bulletDir = (hitPosition - firePositionTransform.position).normalized;
            // ����ü �߻�
            Instantiate(bulletProjectile.transform, firePositionTransform.position, Quaternion.LookRotation(bulletDir, Vector3.up));

            /*            // �浹�� �������κ��� IDamageable ������Ʈ�� �������� �õ�
                        IDamageable target =
                            hit.collider.GetComponent<IDamageable>();

                        // �������� ���� IDamageable ������Ʈ�� �������µ� �����ߴٸ�
                        if (target != null)
                        {
                            // ������ OnDamage �Լ��� ������Ѽ� ���濡�� ������ �ֱ�
                            target.OnDamage(gunData.damage, hit.point, hit.normal);
                        }
            */
        }
        else
        {
            // ���̰� �ٸ� ��ü�� �浹���� �ʾҴٸ�
            // �Ѿ��� �ִ� �����Ÿ����� ���ư������� ��ġ�� �浹 ��ġ�� ���
            hitPosition = firePositionTransform.position +
                          firePositionTransform.forward * fireDistance;
        }

        // �߻� ����Ʈ ��� ����
        StartCoroutine(ShotEffect(hitPosition));

        // ���� źȯ�� ���� -1
        magAmmo--;
        if (magAmmo <= 0)
        {
            // źâ�� ���� ź���� ���ٸ�, ���� ���� ���¸� Empty���� ����
            state = State.MagazineEmpty;
        }

        // ī�޶� ����ŷ
        //CinemachineShake.Instance.ShakeCamera(gunData.recoilStrength, .1f); // �ѱ� �ݵ� ���� ��ŭ ī�޶� ����
    }

    // �߻� ����Ʈ�� �Ҹ��� ����ϰ� �Ѿ� ������ �׸���
    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        // �ѱ� ȭ�� ȿ�� ���
        muzzleFlashParticleSystem.Play();
        // ź�� ���� ȿ�� ���
        shellEjectParticleSystem.playbackSpeed = 4f;
        shellEjectParticleSystem.Play();

        // �Ѱ� �Ҹ� ���
        gunAudioPlayer1.PlayOneShot(gunData.shotClip); // �Ѽ�
        gunAudioPlayer2.clip = gunData.shotTailClip; // ���� �︲ Ŭ�� ����, ����Ÿ�� �� �÷��̿����� ����
        gunAudioPlayer2.Play(); // ���� �︲ ���
        Invoke("ShellDropSFX", 1f); // ź�ǰ� ���� ������ �ð��� �Ǹ� ź�ǼҸ� ���, ä��3���� �����

        /*        // ���� �������� �ѱ��� ��ġ
                bulletLineRenderer.SetPosition(0, fireTransform.position);
                // ���� ������ �Է����� ���� �浹 ��ġ
                bulletLineRenderer.SetPosition(1, hitPosition);
                // ���� �������� Ȱ��ȭ�Ͽ� �Ѿ� ������ �׸���
                bulletLineRenderer.enabled = true;

                // 0.03�� ���� ��� ó���� ���
                yield return new WaitForSeconds(0.03f);

                // ���� �������� ��Ȱ��ȭ�Ͽ� �Ѿ� ������ �����
                bulletLineRenderer.enabled = false;*/

        // 0.03�� ���� ��� ó���� ���
        yield return new WaitForSeconds(0.03f);
    }

    // ������ �õ�
    public bool Reload()
    {
        if (state == State.Reloading ||
            ammoRemain <= 0 || magAmmo >= gunData.magCapacity)
        {
            // �̹� ������ ���̰ų�, ���� �Ѿ��� ���ų�
            // źâ�� �Ѿ��� �̹� ������ ��� ������ �Ҽ� ����
            return false;
        }

        // ������ ó�� ����
        StartCoroutine(ReloadRoutine());
        return true;
    }

    // ���� ������ ó���� ����
    private IEnumerator ReloadRoutine()
    {
        {
            // ���� ���¸� ������ �� ���·� ��ȯ
            state = State.Reloading;

            // ������ �Ҹ� ���
            gunAudioPlayer1.PlayOneShot(gunData.reloadClip);

            // ������ �ҿ� �ð� ��ŭ ó���� ����
            yield return new WaitForSeconds(gunData.reloadTime);

            // źâ�� ä�� ź���� ����Ѵ�
            int ammoToFill = gunData.magCapacity - magAmmo;


            // źâ�� ä������ ź���� ���� ź�ຸ�� ���ٸ�,
            // ä������ ź�� ���� ���� ź�� ���� ���� ���δ�
            if (ammoRemain < ammoToFill)
            {
                ammoToFill = ammoRemain;
            }

            // źâ�� ä���
            magAmmo += ammoToFill;
            // ���� ź�࿡��, źâ�� ä�ŭ ź���� �A��
            ammoRemain -= ammoToFill;

            // ���� ���� ���¸� �߻� �غ�� ���·� ����
            state = State.ReloadingDone;
        }
    }

    public void ShellDropSFX()
    {
        gunAudioPlayer3.PlayOneShot(gunData.shellDropClip); // ź�� ȿ����
    }

    // �������̽�
    public void Use(GameObject target)
    {

    }
}