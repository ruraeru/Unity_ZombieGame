using UnityEngine;

// 플레이어 캐릭터를 조작하기 위한 사용자 입력을 감지
// 감지된 입력값을 다른 컴포넌트들이 사용할 수 있도록 제공
public class PlayerInput : MonoBehaviour
{
    public string moveAxisName = "Vertical"; // 앞뒤 움직임을 위한 입력축 이름
    public string fireButtonName = "Fire1"; // 발사를 위한 입력 버튼 이름
    public string reloadButtonName = "Reload"; // 재장전을 위한 입력 버튼 이름
    public string sprintButtonName = "Sprint"; // 달리기를 위한 입력 버튼 이름
    public string jumpButtonName = "Jump"; // 점프를 위한 입력 버튼 이름

    // 값 할당은 내부에서만 가능
    public float Move { get; private set; } // 감지된 움직임 입력값
    public bool Fire { get; private set; } // 감지된 발사 입력값
    public bool Reload { get; private set; } // 감지된 재장전 입력값
    public bool Sprint { get; private set; } // 감지된 달리기 입력값
    public bool Jump { get; private set; } // 감지된 점프 입력값

    // 매프레임 사용자 입력을 감지
    private void Update()
    {
        // 게임오버 상태에서는 사용자 입력을 감지하지 않는다
        if (GameManager.instance != null && GameManager.instance.isGameover)
        {
            Move = 0;
            Fire = false;
            Reload = false;
            Sprint = false;
            Jump = false;
            return;
        }

        // move에 관한 입력 감지
        Move = Input.GetAxis(moveAxisName);
        // fire에 관한 입력 감지
        Fire = Input.GetButton(fireButtonName);
        // reload에 관한 입력 감지
        Reload = Input.GetButtonDown(reloadButtonName);
        // jump에 관한 입력 감지
        Jump = Input.GetButtonDown(jumpButtonName);
        // sprint에 관한 입력 감지 (Shift 키)
        Sprint = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
    }
}