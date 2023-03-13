using UnityEngine;

// Ŭ���� ���� ���� CreateAssetMenu() Atrribute ����
// �ش� Attribute�� ��ũ���ͺ� ������Ʈ�� ��ӹ޴� Ŭ������ ��ũ���ͺ� ������Ʈ �ּ����� �������
// Attribute�� �Ķ����
// CreateAssetMenu(fileName, menuName, order)
// fileName : ����� ��ũ���ͺ� ������Ʈ �ּ��� �̸�
// menuName : ��ũ���ͺ� ������Ʈ�� �����ϴ� �޴��� �̸� (����Ƽ ��� �޴���)
// order : �޴��� ���° ǥ�õ� ���ΰ�
[CreateAssetMenu(menuName = "Scriptable/GunData", fileName = "Gun Data", order = int.MaxValue)]

// Ŭ������ ScriptableObject ������ֱ�
public class GunData : ScriptableObject
{
    public enum GunType { Pistol, Shotgun, SMG, AR, DMR }
    public GunType gunType;
    public AudioClip shotClip; // �߻� �Ҹ�
    public AudioClip shotTailClip; // ���� �︲ �Ҹ�
    public AudioClip reloadClip; // ������ �Ҹ�
    public AudioClip shellDropClip; // ź�� �Ҹ�

    public float damage = 25; // ���ݷ�

    public int startAmmoRemain = 100; // ó���� �־��� ��ü ź��
    public int magCapacity = 25; // źâ �뷮

    public float timeBetFire = 0.12f; // �Ѿ� �߻� ����
    public float reloadTime = 1.8f; // ������ �ҿ� �ð�

    public float recoilStrength = 2f; // �ݵ� ����
    public float bulletStrength = 20f;
}