using UnityEngine;

// 클래스 선언 전에 CreateAssetMenu() Atrribute 설정
// 해당 Attribute는 스크립터블 오브젝트를 상속받는 클래스를 스크립터블 오브젝트 애셋으로 만들어줌
// Attribute의 파라미터
// CreateAssetMenu(fileName, menuName, order)
// fileName : 저장될 스크립터블 오브젝트 애셋의 이름
// menuName : 스크립터블 오브젝트를 생성하는 메뉴의 이름 (유니티 상단 메뉴바)
// order : 메뉴에 몇번째 표시될 것인가
[CreateAssetMenu(menuName = "Scriptable/GunData", fileName = "Gun Data", order = int.MaxValue)]

// 클래스에 ScriptableObject 상속해주기
public class GunData : ScriptableObject
{
    public enum GunType { Pistol, Shotgun, SMG, AR, DMR }
    public GunType gunType;
    public AudioClip shotClip; // 발사 소리
    public AudioClip shotTailClip; // 공간 울림 소리
    public AudioClip reloadClip; // 재장전 소리
    public AudioClip shellDropClip; // 탄피 소리

    public float damage = 25; // 공격력

    public int startAmmoRemain = 100; // 처음에 주어질 전체 탄약
    public int magCapacity = 25; // 탄창 용량

    public float timeBetFire = 0.12f; // 총알 발사 간격
    public float reloadTime = 1.8f; // 재장전 소요 시간

    public float recoilStrength = 2f; // 반동 세기
    public float bulletStrength = 20f;
}