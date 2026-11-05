using UnityEngine;

public class TheStack : MonoBehaviour
{
    // Const Value
    private const float BoundSize = 3.5f; // 블럭의 기본 너비
    private const float MovingBoundsSize = 3f; //블록이 이동하는 크기
    private const float StackMovingSpeed = 5.0f; //스택이 올라가는 속도
    private const float BlockMovingSpeed = 2.5f; //블록이 좌우로 이동하는 속도
    private const float ErrorMargin = 0.15f;  //정밀도 판정 기준

    public GameObject originBlock = null;  //원본 블록 프리팹

    private Vector3 prevBlockPosition;   //이전 블록의 위치
    private Vector3 desiredPosition;    //전체 스택의 목표 위치
    private Vector3 stackBounds = new Vector2(BoundSize, BoundSize);  //현재 블록의 너비

    Transform lastBlock = null;  //현재 생선된 마지막 블록
    float blockTransition = 0f;  //블록 이동을 위한 시간 누적 변수
    float secondaryPosition = 0f; //고정 위치 X축 or Z축

    int stackCount = -1; //총 쌓은 블록수
    public int Score { get { return stackCount; } }  //점수

    int comboCount = 0;  //퍼펙트 쌓기 횟수
    public int ComboCount { get { return comboCount; } }  //퍼펙트 쌓기 횟수

    private int maxCombo = 0;
    public int MaxCombo { get { return maxCombo; } }  //최대 퍼펙트 쌓기 횟수

    int bestScore = 0; //최고 점수
    public int BestScore { get => bestScore; }

    int bestCombo = 0; //최고 퍼펙트 쌓기 횟수
    public int BestCombo { get => bestCombo; }

    private const string BestScoreKey = "BestScore"; 
    private const string BestComboKey = "BestCombo";


    public Color prevColor;
    public Color nextColor;

    bool isMovingX = true;  //블록 이동 방향 X축 or Z축

    private bool isGameOver = false;  //게임 오버 상태

    void Start()
    {
        if (originBlock == null)
        {
            Debug.Log("OriginBlock is NULL");  //프리팹 설정이 안되었을때
            return;
        }

        //저장된 최고점수 불러오기 없으면 기본값 0
        bestScore = PlayerPrefs.GetInt(BestScoreKey,0);  //최고 점수 불러오기
        bestCombo = PlayerPrefs.GetInt(BestComboKey,0);  //최고 퍼펙트 쌓기 횟수 불러오기

        //시작시 두가지 색상을 랜덤으로 설정
        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        prevBlockPosition = Vector3.down;  //초기 브록보다 아래에 있는 가상 위치   
        Spawn_Block();   //첫번째 블록 생성
        Spawn_Block();   //두번째 블록 생성
    }

    void Update()
    {
        if(isGameOver)
        {
            return;  //게임 오버 상태에서는 블록 생성 및 이동을 막음
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceBlock())
            {
                Spawn_Block();   //마우스 클릭시 블록 생성
            }
            else
            {
                //게임 오버 처리
                Debug.Log("Game Over");
                UpdateScore();
                isGameOver = true;
                GameOverEffect();
                UIManager.Instance.SetScoreUI();
            }
        }

        //스택 전체를 위로 부드럽게 이용 - 보간을 이용해 자연스러운 카메라 이동
        MoveBlock();
        transform.position = Vector3.Lerp(transform.position, desiredPosition, StackMovingSpeed * Time.deltaTime);
    }
    void UpdateScore()
    {
        if(bestScore < stackCount)
        {
            Debug.Log("New Best Score! " + stackCount);
            bestScore = stackCount;
            bestCombo = maxCombo;

            //playerprefs에 최고점수와 최고 퍼펙트 쌓기 횟수 저장 앱을 종료해도 유지됨
            PlayerPrefs.SetInt(BestScoreKey, bestScore);
            PlayerPrefs.SetInt(BestComboKey, bestCombo);
        }
    }
    
    void GameOverEffect()
    {
        int childCount = this.transform.childCount;

        //가장 위 블록부터 최대 20개까지 물리 적용하여 떨어지게 함
        for (int i = 1; i < 20; i++ )
        {
            if (childCount < i) //처리할 자식이 없으면 종료
                break;

            GameObject go = this.transform.GetChild(childCount - i).gameObject;  //뒤에 있는 블록부터 가져오기

            if (go.name.Equals("rubble")) //rubble 블록은 이미 떨어진 블록이므로 제외
                continue;
            
            Rigidbody rigid = go.AddComponent<Rigidbody>();  //물리 적용

            rigid.AddForce(
            (Vector3.up * Random.Range(0, 10f)
             + Vector3.right * (Random.Range(0, 10f) - 5f))
            * 100f
        );
        }
    }

    public void Restart()
    {
        int childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            Destroy(transform.GetChild(i).gameObject);
        }

        isGameOver = false;

        lastBlock = null;
        desiredPosition = Vector3.zero;
        stackBounds = new Vector3(BoundSize, BoundSize);

        stackCount = -1;
        isMovingX = true;
        blockTransition = 0f;
        secondaryPosition = 0f;

        comboCount = 0;
        maxCombo = 0;

        prevBlockPosition = Vector3.down;

        prevColor = GetRandomColor();
        nextColor = GetRandomColor();

        Spawn_Block();
        Spawn_Block();
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

        isMovingX = !isMovingX;  //블록 이동 방향 전환 x <-> z

        UIManager.Instance.UpdateScore();  //점수 UI 갱신

        return true;
    }
    void MoveBlock()
    {
        //블록 이동을 위한 시간 누적
        blockTransition += Time.deltaTime * BlockMovingSpeed;

        //블록 이동 위치 계산 - PingPong을 이용해 BoundSize 범위 내에서 반복 이동
        float movePosition = Mathf.PingPong(blockTransition, BoundSize) - BoundSize / 2f; 

        if (isMovingX)
        {
            //블록 이동 방향이 X축일 경우 X축 이동, Z축은 고정    
            lastBlock.localPosition = new Vector3(movePosition * MovingBoundsSize, stackCount, secondaryPosition);
        }
        else
        {
            //블록 이동 방향이 Z축일 경우 Z축 이동, X축은 고정
            lastBlock.localPosition = new Vector3(secondaryPosition, stackCount, -movePosition * MovingBoundsSize);
        }
    }

    bool PlaceBlock()
    {
        Vector3 lastPosition = lastBlock.localPosition;  //마지막 블록 위치

        if (isMovingX)
        {
            float deltaX = prevBlockPosition.x - lastPosition.x;  //이전 블록과 현재 블록의 X축 차이 계산
            bool isNegativeNum = (deltaX < 0) ?  true : false;  //음수인지 확인

            deltaX = Mathf.Abs(deltaX);  //오차 절대값으로 변환
            if(deltaX > ErrorMargin)
            {
                stackBounds.x -= deltaX;  //블록 너비 감소
                if (stackBounds.x <= 0f)
                {
                    //블록이 더 이상 쌓일 수 없는 경우
                    return false;
                }

                float middle = (prevBlockPosition.x + lastPosition.x) / 2; //이전 블록과 현재 블록의 중간 위치 계산
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);  //블록 크기 조정

                Vector3 tempPosition = lastBlock.localPosition;  //현재 블록 위치 저장
                tempPosition.x = middle;  //중간 위치로 이동
                lastBlock.localPosition = lastPosition = tempPosition;  //블록 위치 업데이트

                float rubbleHalfScale = deltaX / 2;  //잘린 블록의 절반 크기
                CreateRubble(
                    new Vector3(isNegativeNum
                    ? lastPosition.x + stackBounds.x / 2 + rubbleHalfScale
                    : lastPosition.x - stackBounds.x / 2 - rubbleHalfScale
                    , lastPosition.y
                    , lastPosition.z),
                    new Vector3(deltaX, 1, stackBounds.y)
                    );  //잘린 블록 생성
                comboCount = 0;  //퍼펙트 쌓기 초기화
            }
            else
            {
                ComboCheck();
                lastBlock.localPosition = prevBlockPosition + Vector3.up;  //퍼펙트 쌓기 처리
            }
        }
        else
        {
            float deltaZ = prevBlockPosition.z - lastPosition.z;  //이전 블록과 현재 블록의 Z축 차이 계산
            bool isNegativeNum = (deltaZ < 0) ? true : false;  //음수인지 확인

            deltaZ = Mathf.Abs(deltaZ);  //오차 절대값으로 변환
            if(deltaZ > ErrorMargin)
            {
                stackBounds.y -= deltaZ;
                if(stackBounds.y <= 0f)
                {
                    //블록이 더 이상 쌓일 수 없는 경우
                    return false;
                }

                float middle = (prevBlockPosition.z + lastPosition.z) / 2; //이전 블록과 현재 블록의 중간 위치 계산
                lastBlock.localScale = new Vector3(stackBounds.x, 1, stackBounds.y);  //블록 크기 조정

                Vector3 tempPosition = lastBlock.localPosition;
                tempPosition.z = middle;  //중간 위치로 이동
                lastBlock.localPosition = lastPosition = tempPosition;  //블록 위치 업데이트

                float rubbleHalfScale = deltaZ / 2;  //잘린 블록의 절반 크기
                CreateRubble(
                    new Vector3(
                        lastPosition.x
                        ,lastPosition.y
                        ,isNegativeNum
                        ? lastPosition.z + stackBounds.y / 2 + rubbleHalfScale
                        : lastPosition.z - stackBounds.y / 2 - rubbleHalfScale),
                    new Vector3(stackBounds.x, 1, deltaZ )
                    );  //잘린 블록 생성
                comboCount = 0;  //퍼펙트 쌓기 초기화
            }
            else
            {
                ComboCheck();
                lastBlock.localPosition = prevBlockPosition + Vector3.up;  //퍼펙트 쌓기 처리
            }
        }
        secondaryPosition = (isMovingX) ? lastPosition.x : lastPosition.z;  //다음 블록 이동을 위한 고정 위치 업데이트

        return true;
    }

    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject go = Instantiate(lastBlock.gameObject); 
        go.transform.parent = this.transform;  //부모 설정

        go.transform.localPosition = pos;  //위치 설정
        go.transform.localScale = scale;  //크기 설정
        go.transform.localRotation = Quaternion.identity;  //회전 초기화

        go.AddComponent<Rigidbody>();  //물리 적용
        go.name = "Rubble";  //이름 변경
    }
    Color GetRandomColor()
    {
        //100~200 사이의 랜덤값을 생성하여 RGB 색상으로 변환 
        float r = Random.Range(100f, 200f) / 255f;
        float g = Random.Range(100f, 200f) / 255f;
        float b = Random.Range(100f, 200f) / 255f;

        return new Color(r, g, b);
    }

    void ComboCheck()
    {
        comboCount++;  //퍼펙트 쌓기 횟수 증가

        if(comboCount > maxCombo)
        {
            maxCombo = comboCount;
        }

        if((comboCount % 5) == 0)
        {
            Debug.Log("5Combo Bonus!");  //5회마다 보너스 처리
            stackBounds += new Vector3(0.5f, 0f, 0.5f);  //블록 크기 증가
            //블록 크기가 BoundSize를 초과하지 않도록 제한
            stackBounds.x =
            (stackBounds.x > BoundSize) ? BoundSize : stackBounds.x;
            stackBounds.y =
                (stackBounds.y > BoundSize) ? BoundSize : stackBounds.y;
        }
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
