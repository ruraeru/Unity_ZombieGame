using UnityEngine;

// 인트로 씬에서 카메라를 특정 지점 주변으로 회전시키는 스크립트
public class IntroCameraRotate : MonoBehaviour
{
    public Transform target; // 회전의 중심이 될 지점 (맵의 중앙 등)
    public float rotateSpeed = 20f; // 회전 속도

    // 초기 설정값 (사용자 요청 수치 반영)
    public float distance = 15f;
    public float height = 11f;
    public float fieldOfView = 50f;

    private float currentAngle = 0f;

    private void Start()
    {
        // 카메라 FOV 설정
        Camera cam = GetComponent<Camera>();
        if (cam != null)
        {
            cam.fieldOfView = fieldOfView;
        }

        if (target != null)
        {
            // 현재 카메라 위치를 기반으로 거리와 높이, 시작 각도를 자동으로 계산하여 유지
            Vector3 offset = transform.position - target.position;
            height = offset.y;
            distance = new Vector2(offset.x, offset.z).magnitude;
            currentAngle = Mathf.Atan2(offset.z, offset.x) * Mathf.Rad2Deg;
        }
    }



    private void Update()
    {

        if (target == null)
        {
            // 중심점이 설정되지 않았다면 원점(0,0,0)을 기준으로 회전
            RotateAround(Vector3.zero);
            return;
        }

        RotateAround(target.position);
    }

    private void RotateAround(Vector3 center)
    {
        // 시간에 따라 각도 증가
        currentAngle += rotateSpeed * Time.deltaTime;

        // 삼각함수를 이용해 원형 위치 계산
        float x = Mathf.Cos(currentAngle * Mathf.Deg2Rad) * distance;
        float z = Mathf.Sin(currentAngle * Mathf.Deg2Rad) * distance;

        // 카메라 위치 갱신 (상대적 오프셋 적용)
        transform.position = center + new Vector3(x, height, z);

        // 항상 중심점을 바라보도록 회전 설정 (프레임 유지)
        transform.LookAt(center);
    }
}
