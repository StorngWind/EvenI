using UnityEngine;

public class BaseController : MonoBehaviour
{
    // 물리를 담당하는 컴포넌트 (자식 클래스에서도 접근 가능하도록 protected 설정)
    protected Rigidbody2D _rigidbody2D;

    // 인스펙터 창에서 직접 드래그해서 연결할 캐릭터 스프라이트와 무기 회전축
    [SerializeField] private SpriteRenderer characterRenderer;
    [SerializeField] private Transform weaponPivot;

    // 이동 방향과 바라보는 방향 (자식 클래스에서 값을 계산해 넣어줌)
    protected Vector2 movementDirection = Vector2.zero;
    public Vector2 MovementDirection { get { return movementDirection; } } // 외부 읽기 전용 프로퍼티

    protected Vector2 lookDirection = Vector2.zero;
    public Vector2 LookDirection { get { return lookDirection; } }         // 외부 읽기 전용 프로퍼티

    // 피격 시 밀려나는(넉백) 힘과 지속 시간
    private Vector2 knockback = Vector2.zero;
    private float knockbackDuration = 0.0f;

    // 1. 초기화: 게임 오브젝트가 생성될 때 Rigidbody2D 컴포넌트를 찾아 연결
    protected virtual void Awake()
    {
        _rigidbody2D = GetComponent<Rigidbody2D>();
    }

    protected virtual void Start()
    {

    }

    // 2. 매 프레임 호출: 입력/AI 판단 처리 및 캐릭터/무기 회전
    protected virtual void Update()
    {
        HandleAction();        // 자식 클래스에서 구체적인 행동(입력 등)을 구현할 함수
        Rotate(lookDirection); // 바라보는 방향에 맞춰 캐릭터 및 무기 회전
    }

    // 3. 물리 프레임마다 호출: 실제 이동(Rigidbody2D) 및 넉백 타이머 감소
    protected virtual void FixedUpdate()
    {
        MoveMent(movementDirection);

        // 넉백 지속 시간이 남아있다면 물리 시간만큼 차감
        if (knockbackDuration > 0.0f)
        {
            knockbackDuration -= Time.fixedDeltaTime;
        }
    }

    // 자식 클래스(PlayerController 등)에서 override(재정의)하여 사용할 빈 함수
    protected virtual void HandleAction()
    {

    }

    // 실제 캐릭터를 이동시키는 함수
    private void MoveMent(Vector2 direction)
    {
        // 기본 이동 속도를 5로 설정
        direction = direction * 5;

        // 넉백 중이라면 원래 이동 속도를 20%로 줄이고 넉백 힘을 더함
        if (knockbackDuration > 0.0f)
        {
            direction *= 0.2f;
            direction += knockback;
        }

        // 최종 계산된 속도 벡터를 Rigidbody2D에 적용해 실제 이동 수행
        _rigidbody2D.linearVelocity = direction;
    }

    // 바라보는 방향(lookDirection)에 맞춰 캐릭터 좌우 반전 및 무기 각도를 조절하는 함수
    private void Rotate(Vector2 direction)
    {
        // 방향 벡터(x, y)를 이용해 라디안 각도를 구하고, 이를 일반 각도(Degree, -180 ~ 180도)로 변환
        float rotZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 절대값 각도가 90도보다 크면 왼쪽을 바라보고 있는 것으로 판정
        bool isLeft = Mathf.Abs(rotZ) > 90f;

        // 왼쪽을 볼 때는 캐릭터 이미지를 좌우 반전(Flip)
        characterRenderer.flipX = isLeft;

        // 무기 회전축(Pivot)이 연결되어 있다면 Z축 기준으로 마우스/타겟 방향을 향해 회전
        if (weaponPivot != null)
        {
            weaponPivot.rotation = Quaternion.Euler(0f, 0f, rotZ);
        }
    }

    // 외부에서 공격받았을 때 호출하여 넉백을 적용하는 함수
    public void ApplyKnockback(Transform other, float power, float duration)
    {
        knockbackDuration = duration;

        // 상대방(other) 반대편으로 밀려나는 방향 벡터(길이 1)를 구한 뒤 힘(power)을 곱함
        knockback = -(other.position - transform.position).normalized * power;
    }
}