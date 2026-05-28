using System.Collections;
using UnityEngine;
using UnityEngine.AI; // AI, 내비게이션 시스템 관련 코드 가져오기

// 좀비 AI 구현
public class Zombie : LivingEntity
{
    public LayerMask whatIsTarget; // 추적 대상 레이어

    private LivingEntity targetEntity; // 추적 대상
    private NavMeshAgent navMeshAgent; // 경로 계산 AI 에이전트

    public ParticleSystem hitEffect; // 피격 시 재생할 파티클 효과
    public AudioClip deathSound; // 사망 시 재생할 소리
    public AudioClip hitSound; // 피격 시 재생할 소리

    private Animator zombieAnimator; // 애니메이터 컴포넌트
    private AudioSource zombieAudioPlayer; // 오디오 소스 컴포넌트
    private Renderer zombieRenderer; // 렌더러 컴포넌트

    public float damage = 20f; // 공격력
    public float timeBetAttack = 0.5f; // 공격 간격
    private float lastAttackTime; // 마지막 공격 시점

    private ZombieData zombieData; // 좀비 데이터 참조
    public Transform firePoint; // 투사체 발사 위치

    // 추적할 대상이 존재하는지 알려주는 프로퍼티
    private bool hasTarget
    {
        get
        {
            // 추적할 대상이 존재하고, 대상이 사망하지 않았다면 true
            if (targetEntity != null && !targetEntity.dead)
            {
                return true;
            }

            // 그렇지 않다면 false
            return false;
        }
    }

    private void Awake()
    {
        // 초기화
        navMeshAgent = GetComponent<NavMeshAgent>();
        zombieAnimator = GetComponent<Animator>();
        zombieAudioPlayer = GetComponent<AudioSource>();

        //렌더러 컴포넌트는 자식 게임 오브젝트에게 있으므로
        //GetComponentInChildren() 메서드를 사용
        zombieRenderer = GetComponentInChildren<Renderer>();
    }

    // 좀비 AI의 초기 스펙을 결정하는 셋업 메서드
    public void Setup(ZombieData zombieData)
    {
        this.zombieData = zombieData;
        startingHealth = zombieData.health;
        health = zombieData.health;

        damage = zombieData.damage;
        //네비매시 에이전트의 이동 속도를 좀비 데이터에서 가져온 값으로 설정
        navMeshAgent.speed = zombieData.speed;
        //렌더러가 사용 중인 머테리얼의 컬러를 변경, 외형 색이 변함
        zombieRenderer.material.color = zombieData.skinColor;
    }

    private void Start()
    {
        // 게임 오브젝트 활성화와 동시에 AI의 추적 루틴 시작
        StartCoroutine(UpdatePath());
    }

    private void Update()
    {
        // 추적 대상의 존재 여부에 따라 다른 애니메이션 재생
        zombieAnimator.SetBool("HasTarget", hasTarget);

        // 원거리 좀비이고 추적 대상이 있다면 사격 처리
        if (!dead && hasTarget && zombieData != null && zombieData.isRanged)
        {
            float distance = Vector3.Distance(transform.position, targetEntity.transform.position);

            // 사정거리 안에 들어오면 플레이어를 바라보고 사격
            if (distance <= zombieData.attackRange)
            {
                // 플레이어를 향해 회전 (Y축만 회전하여 바닥에 고정되게 함)
                Vector3 lookDir = targetEntity.transform.position - transform.position;
                lookDir.y = 0;
                transform.rotation = Quaternion.LookRotation(lookDir);

                // 공격 간격 확인 후 발사
                if (Time.time >= lastAttackTime + timeBetAttack)
                {
                    lastAttackTime = Time.time;
                    Shoot();
                }
            }
        }
    }

    // 원거리 공격 실행
    private void Shoot()
    {
        if (zombieData.projectilePrefab != null && firePoint != null)
        {
            // 투사체 생성 및 방향 설정
            GameObject projectile = Instantiate(zombieData.projectilePrefab, firePoint.position, firePoint.rotation);
            ZombieProjectile projectileScript = projectile.GetComponent<ZombieProjectile>();
            
            if (projectileScript != null)
            {
                projectileScript.damage = damage; // 좀비의 데미지를 투사체에 전달
            }
        }
    }

    // 주기적으로 추적할 대상의 위치를 찾아 경로 갱신
    private IEnumerator UpdatePath()
    {
        // 살아 있는 동안 무한 루프
        while (!dead)
        {
            if (hasTarget)
            {
                // 원거리 좀비이고 사거리 안에 있다면 이동 중지
                if (zombieData != null && zombieData.isRanged &&
                    Vector3.Distance(transform.position, targetEntity.transform.position) <= zombieData.attackRange)
                {
                    navMeshAgent.isStopped = true;
                }
                else
                {
                    //추적 대상 존제 : 경로를 갱신하고 AI 이동을 계속 진행
                    navMeshAgent.isStopped = false;
                    navMeshAgent.SetDestination(targetEntity.transform.position);
                }
            }
            else
            {
                //추적 대상 존재하지 않음 : AI 이동 멈춤
                navMeshAgent.isStopped = true;

                //20 유닛 반지름을 가진 가상의 구를 그렸을 때 구와 겹치는 모든 콜라이더를 가져옴
                //단, whatIsTarget 레이어에 해당하는 콜라이더만 가져오도록 필터링
                Collider[] colliders = Physics.OverlapSphere(transform.position, 20f, whatIsTarget);

                //모든 콜라이더를 순회하면서 살아 있는 LivingEntity 컴포넌트를 가진 게임 오브젝트가 있는지 확인
                for (int i = 0; i < colliders.Length; i++)
                {
                    // 겹친 콜라이더의 게임 오브젝트에서 LivingEntity 컴포넌트를 가져옴
                    LivingEntity livingEntity = colliders[i].GetComponent<LivingEntity>();

                    // LivingEntity 컴포넌트가 존재하고, 사망하지 않았다면 추적 대상으로 설정
                    if (livingEntity != null && !livingEntity.dead)
                    {
                        //추적 대상을 해당 LivingEntity로 설정
                        targetEntity = livingEntity;
                        break; // 추적 대상이 하나라도 발견되면 루프 종료
                    }
                }
            }

            // 0.25초 주기로 처리 반복
            yield return new WaitForSeconds(0.25f);
        }
    }

    // 데미지를 입었을 때 실행할 처리
    public override void OnDamage(float damage, Vector3 hitPoint, Vector3 hitNormal)
    {
        //아직 사망하지 않은 경우에만 피격 효과 재생
        if (!dead)
        {
            //공격 받은 지점과 방향으로 파티클 효과를 재생
            hitEffect.transform.position = hitPoint;
            hitEffect.transform.rotation = Quaternion.LookRotation(hitNormal);
            hitEffect.Play();

            //피격 효과음 재생
            zombieAudioPlayer.PlayOneShot(hitSound);
        }

        // LivingEntity의 OnDamage()를 실행하여 데미지 적용
        base.OnDamage(damage, hitPoint, hitNormal);
    }

    // 사망 처리
    public override void Die()
    {
        // LivingEntity의 Die()를 실행하여 기본 사망 처리 실행
        base.Die();

        // 콤보 추가
        if (GameManager.instance != null)
        {
            GameManager.instance.AddCombo();
        }

        //다른 AI를 방해하지 않도록 자신의 모든 콜라이더를 비활성화
        Collider[] zombieColliders = GetComponents<Collider>();
        for (int i = 0; i < zombieColliders.Length; i++)
        {
            zombieColliders[i].enabled = false;
        }

        //AI 추적을 중지하고 네비매쉬 컴포넌트를 비활성화
        navMeshAgent.isStopped = true;
        navMeshAgent.enabled = false;

        //사망 애니메이션 재생
        zombieAnimator.SetTrigger("Die");

        //사망 효과음 재생
        zombieAudioPlayer.PlayOneShot(deathSound);
    }

    private void OnTriggerStay(Collider other)
    {
        //자신이 사망하지 않았으며,
        //최근 공격 시점에서 timeBetAttack 이상 시간이 지났다면 공격 실행 가능
        if (!dead && Time.time >= lastAttackTime + timeBetAttack)
        {
            //상대방으로부터 LivingEntity 컴포넌트를 가져옴
            LivingEntity attackTarget = other.GetComponent<LivingEntity>();


            //상대방의 LivingEntity가 자신의 추적 대상이라면 공격 실행
            if (attackTarget != null && attackTarget == targetEntity)
            {
                //최근 공격 시점 갱신
                lastAttackTime = Time.time;

                //상대방의 피격 위치와 피격 방향을 근삿값으로 계산
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Vector3 hitNormal = transform.position - other.transform.position;

                //공격 실행
                attackTarget.OnDamage(damage, hitPoint, hitNormal);
            }

        }
    }
}