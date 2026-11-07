using UnityEngine;
using UnityEngine.InputSystem;

// BaseController를 상속받아 플레이어 조작을 담당하는 클래스
public class PlayerController : BaseController
{
    private Camera mainCamera;       // 마우스 화면 좌표를 게임 월드 좌표로 변환하기 위한 메인 카메라
    private Vector2 pointerPosition; // 마우스 커서의 현재 화면(Screen) 위치
    private PlayerInput input;       // 유니티 Input System의 PlayerInput 컴포넌트

    // 1. 초기화: 부모(BaseController)의 Awake를 먼저 실행한 뒤 필요한 컴포넌트 참조
    protected override void Awake()
    {
        base.Awake(); // 부모 클래스에서 Rigidbody2D를 가져오는 로직 실행
        mainCamera = Camera.main;
        input = GetComponent<PlayerInput>();
    }

    // 2. 이동 입력 이벤트: PlayerInput의 Move 액션 발생 시 호출됨
    public void OnMove(InputAction.CallbackContext context)
    {
        // 입력받은 WASD/방향키의 Vector2 값을 읽어와 길이를 1로 정규화(normalized)하여 저장
        // (대각선 이동 시 속도가 루트 2배 빨라지는 현상 방지)
        movementDirection = context.ReadValue<Vector2>().normalized;
    }

    // 3. 시선 입력 이벤트: PlayerInput의 Look 액션 발생 시 호출됨
    public void OnLook(InputAction.CallbackContext context)
    {
        // 마우스 커서의 현재 화면 픽셀 좌표(Screen Space)를 읽어와 저장
        pointerPosition = context.ReadValue<Vector2>();
    }

    // 4. 매 프레임 행동 처리: 부모 클래스의 Update()에서 매 프레임 자동으로 호출됨
    protected override void HandleAction()
    {
        // 모니터 화면 기준인 마우스 좌표(pointerPosition)를 실제 게임 공간(World) 좌표로 변환
        Vector2 world = mainCamera.ScreenToWorldPoint(pointerPosition);

        // 타겟(마우스 월드 좌표) 위치에서 내 캐릭터 위치를 빼서 마우스를 향하는 방향 벡터 계산
        Vector2 direction = world - (Vector2)transform.position;

        // 마우스 커서가 캐릭터와 너무 가까우면(거리 0.3 미만) 시선 처리를 멈춤 (캐릭터가 제자리에서 덜덜 떨리는 현상 방지)
        if (direction.magnitude < .3f) lookDirection = Vector2.zero;
        // 충분히 떨어져 있다면 방향 벡터의 길이를 1로 정규화하여 바라보는 방향(lookDirection)에 적용
        else lookDirection = direction.normalized;
    }
}