using UnityEngine;

public class TheStack : MonoBehaviour
{
    // Const Value
    private const float BoundSize = 3.5f; // 블럭의 기본 너비
    private const float MovingBoundsSize = 3f; //블록이 이동하는 크기
    private const float StackMovingSpeed = 5.0f; //스택이 올라가는 속도
    private const float BlockMovingSpeed = 3.5f; //블록이 좌우로 이동하는 속도
    private const float ErrorMargin = 0.1f;  //정밀도 판정 기준

    public GameObject originBlock = null;  //원본 블록 프리팹

    private Vector3 prevBlockPosition;   //이전 블록의 위치
    private Vector3 desiredPosition;    //전체 스택의 목표 위치
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);  //현재 블록의 너비

    Transform lastBlock = null;  //현재 생선된 마지막 블록
    float blockTransition = 0f;  //블록 이동을 위한 시간 누적 변수
    float secondaryPosition = 0f; //고정 위치 X축 or Z축

    int stackCount = -1; //총 쌓은 블록수
    int comboCount = 0;  //퍼펙트 쌓기 횟수

    public Color prevColor;
    public Color nextColor;

    void Start()
    {
        if (originBlock == null)
        {
            Debug.Log("OriginBlock is NULL");  //프리팹 설정이 안되었을때
            return;
        }

        //시작시 두가지 색상을 랜덤으로 설정
        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        prevBlockPosition = Vector3.down;  //초기 브록보다 아래에 있는 가상 위치   
        Spawn_Block();   //첫번째 블록 생성
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Spawn_Block();   //마우스 클릭시 블록 생성
        }

        //스택 전체를 위로 부드럽게 이용 - 보간을 이용해 자연스러운 카메라 이동
        transform.position = Vector3.Lerp(transform.position, desiredPosition, StackMovingSpeed * Time.deltaTime);
    }

    bool Spawn_Block()
    {
        // 이전블럭 위치 저장
        if (lastBlock != null)
            prevBlockPosition = lastBlock.localPosition;

        GameObject newBlock = null;
        Transform newTrans = null;

        //새블럭 생성
        newBlock = Instantiate(originBlock);

        if (newBlock == null)
        {
            Debug.Log("NewBlock Instantiate Failed!");
            return false;
        }

        //새 블럭에 색상 적용
        ColorChange(newBlock);

        newTrans = newBlock.transform;
        //부모 설정
        newTrans.parent = this.transform;
        //위치 설정 이전 블록 위에 쌓임
        newTrans.localPosition = prevBlockPosition + Vector3.up;
        //회전 초기화
        newTrans.localRotation = Quaternion.identity;
        //블록 크기 설정
        newTrans.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);

        //블록 개수증가
        stackCount++;

        //카메라 스택이 이동할 목표 위치 업데이트
        desiredPosition = Vector3.down * stackCount;
        //블록 이동 초기화
        blockTransition = 0f;
        //마지막 블록 갱신
        lastBlock = newTrans;

        return true;
    }

    Color GetRandomColor()
    {
        //100~200 사이의 랜덤값을 생성하여 RGB 색상으로 변환 
        float r = Random.Range(100f, 200f) / 255f;
        float g = Random.Range(100f, 200f) / 255f;
        float b = Random.Range(100f, 200f) / 255f;

        return new Color(r, g, b);
    }

    void ColorChange(GameObject go)
    {
        //prevColor 와 nextColor 사이에서 stackCount에 따라 보간된 색상을 적용
        Color applyColor = Color.Lerp(prevColor, nextColor, (stackCount % 11) / 10f);

        //블록에 적용
        Renderer rn = go.GetComponent<Renderer>();

        if(rn == null)
        {
            Debug.Log("Renderer is null"); return;
        }

        rn.material.color = applyColor;

        //카메라 배경색도 블록보다 조금 어둡게 적용
        Camera.main.backgroundColor = applyColor - new Color(0.1f, 0.1f, 0.1f);

        //만약 보간이 끝까지 도달하면 새로운 색으로 준비
        if(applyColor.Equals(nextColor) == true)
        {
            prevColor = nextColor;
            nextColor = GetRandomColor();
        }
    }
}
