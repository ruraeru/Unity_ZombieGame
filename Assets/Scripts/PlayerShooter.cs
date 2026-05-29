using UnityEngine;

// 주어진 Gun 오브젝트를 쏘거나 재장전
// 알맞은 애니메이션을 재생하고 IK를 사용해 캐릭터 양손이 총에 위치하도록 조정
public class PlayerShooter : MonoBehaviour
{
    public Gun gun; // 사용할 총
    public Transform gunPivot; // 총 배치의 기준점
    public Transform leftHandMount; // 총의 왼쪽 손잡이, 왼손이 위치할 지점
    public Transform rightHandMount; // 총의 오른쪽 손잡이, 오른손이 위치할 지점

    private PlayerInput playerInput; // 플레이어의 입력
    private Animator playerAnimator; // 애니메이터 컴포넌트

    private void Start()
    {
        // 사용할 컴포넌트들을 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerAnimator = GetComponent<Animator>();

        // 콤보 이벤트 구독
        if (GameManager.instance != null)
        {
            GameManager.instance.onComboChanged += HandleComboBuff;
            GameManager.instance.onComboReset += ResetBuff;
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (GameManager.instance != null)
        {
            GameManager.instance.onComboChanged -= HandleComboBuff;
            GameManager.instance.onComboReset -= ResetBuff;
        }
    }

    // 콤보에 따른 버프 처리
    private void HandleComboBuff(int combo)
    {
        if (gun != null)
        {
            if (combo >= 15) gun.comboDamageMultiplier = 2.0f; // 15콤보 이상 공격력 2배
            if (combo >= 10) gun.fireRateMultiplier = 0.5f; // 10콤보 이상 연사 속도 2배
        }
    }

    // 버프 초기화
    private void ResetBuff()
    {
        if (gun != null)
        {
            gun.fireRateMultiplier = 1.0f;
            gun.comboDamageMultiplier = 1.0f;
        }
    }

    private void OnEnable()
    {
        // 슈터가 활성화될 때 총도 함께 활성화
        gun.gameObject.SetActive(true);
    }

    private void OnDisable()
    {
        // 슈터가 비활성화될 때 총도 함께 비활성화
        gun.gameObject.SetActive(false);
    }

    private void Update()
    {
        // 입력을 감지하고 총 발사하거나 재장전
        if (playerInput.Fire)
        {
            //발사 입력 감지 시 총 발사
            gun.Fire();
        }
        else if (playerInput.Reload)
        {
            if (gun.Reload())
            {
                //재장전 성공 시 애니메이터의 Reload 트리거 파라미터를 활성화
                playerAnimator.SetTrigger("Reload");
            }
        }
        UpdateUI(); // 탄약 UI 갱신
    }

    // 탄약 UI 갱신
    private void UpdateUI()
    {
        if (gun != null && UIManager.instance != null)
        {
            // UI 매니저의 탄약 텍스트에 탄창의 탄약과 남은 전체 탄약을 표시
            UIManager.instance.UpdateAmmoText(gun.magAmmo, gun.ammoRemain);

            // 현재 총의 데미지 정보를 UI에 표시
            if (gun.gunData != null)
            {
                UIManager.instance.UpdateDamageText(gun.gunData.damage * gun.damageMultiplier);
            }
        }
    }

    // 애니메이터의 IK 갱신
    private void OnAnimatorIK(int layerIndex)
    {
        //총의 기준점 gunPivot을 3D 모델의 오른쪽 팔꿈치 위치로 이동
        gunPivot.position = playerAnimator.GetIKHintPosition(AvatarIKHint.RightElbow);

        //IK를 사용하여 왼손의 위치와 회전을 총의 왼쪽 손잡이에 맞춤
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
        playerAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandMount.rotation);

        //IK를 사용하여 오른손의 위치와 회전을 총의 오른쪽 손잡이에 맞춤
        playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
        playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);
        playerAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandMount.position);
        playerAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandMount.rotation);
    }
}