using UnityEngine;

// 플레이어 캐릭터를 사용자 입력에 따라 움직이는 스크립트
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; // 앞뒤 움직임의 속도
    private float originalMoveSpeed; // 원래 이동 속도 저장

    private PlayerInput playerInput; // 플레이어 입력을 알려주는 컴포넌트
    private Rigidbody playerRigidbody; // 플레이어 캐릭터의 리지드바디
    private Animator playerAnimator; // 플레이어 캐릭터의 애니메이터

    private void Start()
    {
        // 사용할 컴포넌트들의 참조를 가져오기
        playerInput = GetComponent<PlayerInput>();
        playerRigidbody = GetComponent<Rigidbody>();
        playerAnimator = GetComponent<Animator>();

        originalMoveSpeed = moveSpeed;

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
        if (combo >= 5)
        {
            moveSpeed = originalMoveSpeed * 1.5f; // 5콤보 이상이면 속도 1.5배
        }
    }

    // 버프 초기화
    private void ResetBuff()
    {
        moveSpeed = originalMoveSpeed;
    }

    // FixedUpdate는 물리 갱신 주기에 맞춰 실행됨 0.02초마다 한 번씩 실행됨
    private void FixedUpdate()
    {
        // 물리 갱신 주기마다 움직임, 회전, 애니메이션 처리 실행
        Rotate();
        Move();

        // 입력값에 따라 애니메이터의 Move 파리미터값 변경
        playerAnimator.SetFloat("Move", playerInput.Move);
    }

    // 입력값에 따라 캐릭터를 앞뒤로 움직임
    private void Move()
    {
        //상대적으로 이동할 거리 계산
        Vector3 moveDistance = playerInput.Move * transform.forward * moveSpeed * Time.deltaTime;

        //리지드바디를 이용해 게임 오브젝트 위치 변경
        playerRigidbody.MovePosition(playerRigidbody.position + moveDistance);
    }

    // 마우스 포인터 방향을 바라보도록 캐릭터 회전
    private void Rotate()
    {
        // 마우스 커서 위치를 월드 좌표로 변환하기 위해 메인 카메라에서 레이를 쏨
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // 캐릭터의 발밑을 기준으로 하는 수평 평면 생성
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        float rayDistance;

        // 레이가 평면과 교차하는지 확인
        if (groundPlane.Raycast(ray, out rayDistance))
        {
            // 교차 지점을 가져옴
            Vector3 lookPoint = ray.GetPoint(rayDistance);

            // 캐릭터가 바라볼 방향 벡터 계산
            Vector3 lookDirection = lookPoint - transform.position;
            lookDirection.y = 0; // y축 회전만 사용

            // 방향이 유효할 때만 회전 적용
            if (lookDirection != Vector3.zero)
            {
                Quaternion rotation = Quaternion.LookRotation(lookDirection);
                playerRigidbody.MoveRotation(rotation);
            }
        }
    }
}