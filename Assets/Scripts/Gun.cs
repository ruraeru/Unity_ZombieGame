using System.Collections;
using UnityEngine;

// 총을 구현
[RequireComponent(typeof(AudioSource))]
public class Gun : MonoBehaviour
{
    // 총의 상태를 표현하는 데 사용할 타입을 선언
    public enum State
    {
        Ready, // 발사 준비됨
        Empty, // 탄알집이 빔
        Reloading // 재장전 중
    }

    public State state { get; private set; } // 현재 총의 상태

    public Transform fireTransform; // 탄알이 발사될 위치

    public ParticleSystem muzzleFlashEffect; // 총구 화염 효과
    public ParticleSystem shellEjectEffect; // 탄피 배출 효과

    private LineRenderer bulletLineRenderer; // 탄알 궤적을 그리기 위한 렌더러

    private AudioSource gunAudioPlayer; // 총 소리 재생기

    public GunData gunData; // 총의 현재 데이터

    public LayerMask targetMask = -1; // 타격할 레이어들

    private float fireDistance = 50f; // 사정거리

    public int ammoRemain = 100; // 남은 전체 탄알
    public int magAmmo; // 현재 탄알집에 남아 있는 탄알

    private float lastFireTime; // 총을 마지막으로 발사한 시점

    public float fireRateMultiplier = 1f; // 발사 속도 배율 (콤보 버프용)
    public float damageMultiplier = 1f; // 데미지 배율 (아이템 버프용)
    public float comboDamageMultiplier = 1f; // 콤보 데미지 배율 (추가)

    private void Awake()
    {
        // 사용할 컴포넌트의 참조 가져오기
        gunAudioPlayer = GetComponent<AudioSource>();

        // 만약 AudioSource가 없다면 추가 (런타임에 인스턴스화된 경우 대비)
        if (gunAudioPlayer == null)
        {
            gunAudioPlayer = gameObject.AddComponent<AudioSource>();
        }

        bulletLineRenderer = GetComponent<LineRenderer>();

        // 라인 렌더러가 있는 경우에만 설정
        if (bulletLineRenderer != null)
        {
            //사용할 점을 두개로 변경
            bulletLineRenderer.positionCount = 2; //라인렌더러를 그릴 시작점과 끝점 2개로 설정
            //라인 렌더러를 비활성화
            bulletLineRenderer.enabled = false;
        }
    }

    private void OnEnable()
    {
        //전체 예비 탄알 양을 초기화
        ammoRemain = gunData.startAmmoRemain;

        //현재 탄창을 가득 채우기
        magAmmo = gunData.magCapacity;

        // 배율 초기화 (콤보 및 아이템 버프 초기화)
        fireRateMultiplier = 1f;
        damageMultiplier = 1f;

        //총의 현재 상태를 총을 쏠 준비가 된 상태로 변경
        state = State.Ready;
        //마지막으로 총을 쏜 시점을 초기화
        lastFireTime = 0f;
    }

    // 발사 시도
    public void Fire()
    {
        //현재 상태가 발사 가능한 상태
        // && 마지막 총 발사 시점에서 (gunData.timeBetFire * multiplier) 이상 시간이 지남
        if (state == State.Ready && Time.time >= lastFireTime + (gunData.timeBetFire * fireRateMultiplier))
        {
            lastFireTime = Time.time; // 마지막 총 발사 시점 갱신
            //총 발사 처리 실행
            Shot();
        }
    }

    // 실제 발사 처리
    private void Shot()
    {
        //레이캐스트에 의한 충돌 정보를 저장하는 컨테이너
        RaycastHit hit;
        //탄알이 맞은 곳을 저장할 변수
        Vector3 hitPosition = Vector3.zero;

        //레이캐스트(시작 지점, 방향, 충돌 정보 컨테이너, 사정거리, 타격 레이어 마스크)
        if (Physics.Raycast(fireTransform.position, fireTransform.forward, out hit, fireDistance, targetMask)) //out 키워드로 매개변수 hit의 주소를 넘겨주어 레이캐스트가 충돌 정보를 hit에 저장할 수 있도록 함
        {
            //레이가 어떤 물체와 충돌한 경우

            //충돌한 상대방으로부터 IDamageable 인터페이스를 가져오기 시도
            //다형성 : 충돌한 상대방이 IDamageable 인터페이스를 구현하고 있다면, 그 인터페이스를 가져올 수 있음
            IDamageable target = hit.collider.GetComponent<IDamageable>();

            //상대방으로부터 IDamageable 인터페이스를 가져오는데 성공했다면
            if (target != null)
            {
                //상대방의 OnDamage 함수를 호출하여 데미지 전달 (기본 데미지 * 배율)
                target.OnDamage(gunData.damage * damageMultiplier, hit.point, hit.normal);
            }

            //충돌 위치를 저장
            hitPosition = hit.point; //라인렌더러를 그릴 끝점을 저장. 좀비 위치
        }
        else
        {
            //레이가 어떤 물체와도 충돌하지 않은 경우, 사정거리 끝 지점으로 충돌 위치 설정 (사거리 : 50)
            hitPosition = fireTransform.position + fireTransform.forward * fireDistance;
        }

        //발사 이펙트 재생과 탄알 궤적을 그리는 코루틴 실행
        StartCoroutine(ShotEffect(hitPosition));

        //남은 탄알 수에서 1 감소
        magAmmo--;
        //총알이 다 떨어졌다면 상태를 Empty로 변경
        if (magAmmo <= 0)
        {
            state = State.Empty;
        }
    }

    // 발사 이펙트와 소리를 재생하고 탄알 궤적을 그림
    private IEnumerator ShotEffect(Vector3 hitPosition)
    {
        //총구 화염 효과 재생
        if (muzzleFlashEffect != null) muzzleFlashEffect.Play();
        //탄피 배출 효과 재생
        if (shellEjectEffect != null) shellEjectEffect.Play();

        //총 발사 소리 재생
        if (gunAudioPlayer != null && gunData != null && gunData.shotClip != null)
        {
            gunAudioPlayer.PlayOneShot(gunData.shotClip);
        }
        else
        {
            // 소리가 나지 않는 이유를 알 수 있도록 경고 로그 출력
            if (gunData == null) Debug.LogWarning(gameObject.name + "의 GunData가 할당되지 않았습니다.");
            else if (gunData.shotClip == null) Debug.LogWarning(gunData.name + "에 Shot Clip이 할당되지 않았습니다.");
        }

        // 라인 렌더러가 있는 경우에만 궤적을 그림
        if (bulletLineRenderer != null)
        {
            //선의 시작점은 총구의 위치
            bulletLineRenderer.SetPosition(0, fireTransform.position);
            //선의 끝점은 입력으로 들어온 충돌 위치
            bulletLineRenderer.SetPosition(1, hitPosition);
            // 라인 렌더러를 활성화하여 탄알 궤적을 그림
            bulletLineRenderer.enabled = true;
        }

        // 0.03초 동안 잠시 처리를 대기
        yield return new WaitForSeconds(0.03f);

        // 라인 렌더러를 비활성화하여 탄알 궤적을 지움
        if (bulletLineRenderer != null)
        {
            bulletLineRenderer.enabled = false;
        }
    }

    // 재장전 시도
    public bool Reload()
    {
        if (state == State.Reloading || ammoRemain <= 0 || magAmmo >= gunData.magCapacity)
        {
            //이미 재장전 중이거나, 남은 탄알이 없거나, 탄창에 이미 탄알이 가득한 경우 재장전 불가능
            return false;
        }
        //재장전 처리 실행
        StartCoroutine(ReloadRoutine());
        return true;
    }

    // 실제 재장전 처리를 진행
    private IEnumerator ReloadRoutine()
    {
        // 현재 상태를 재장전 중 상태로 전환
        state = State.Reloading;

        // 재장전 소리 재생
        if (gunAudioPlayer != null && gunData != null && gunData.reloadClip != null)
        {
            gunAudioPlayer.PlayOneShot(gunData.reloadClip);
        }

        // 재장전 소요 시간 만큼 처리 쉬기
        yield return new WaitForSeconds(gunData.reloadTime);

        //탄창에 채울 탄알을 계산
        int ammoToFill = gunData.magCapacity - magAmmo;

        //탄창에 채워야 할 탄알이 남은 탄알보다 많다면
        //채워야 할 탄알 수를 남은 탄알 수에 맞춰 줄임
        if (ammoRemain < ammoToFill)
        {
            ammoToFill = ammoRemain;
        }

        //탄창에 탄알 채우기
        magAmmo += ammoToFill;
        //남은 탄알 수에서 채운 탄알 수만큼 빼기
        ammoRemain -= ammoToFill;

        // 총의 현재 상태를 발사 준비된 상태로 변경
        state = State.Ready;
    }
}