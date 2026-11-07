using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// 게임 오브젝트에 붙이지 않고 어디서든 바로 호출해 쓸 수 있는 정적(static) 유틸리티 클래스
public static class UiInput
{
    // 현재 마우스(포인터) 입력이 UI 위에서 발생했는지 확인하여, 인게임 행동을 막을지(true/false) 결정하는 함수
    public static bool Blocks(PlayerInput input, InputAction.CallbackContext context)
    {
        // 1. 입력 장치가 마우스/터치(Pointer)가 아니거나(예: 키보드, 게임패드), 
        //    현재 씬에 UI 이벤트를 관리하는 EventSystem이 없다면 막지 않음(false 반환)
        if (!(context.control.device is Pointer) || EventSystem.current == null)
            return false;

        // 2. 현재 마우스 위치 정보를 담을 UI 이벤트 데이터 생성
        var data = new PointerEventData(EventSystem.current);

        // 3. Input Actions의 'UI' 맵에 있는 'Point' 액션에서 현재 마우스 화면 좌표(Vector2)를 읽어와 저장
        data.position = input.actions["UI/Point"].ReadValue<Vector2>();

        // 4. 마우스 위치에 보이지 않는 광선(Raycast)을 쏴서 맞은 UI 요소들을 담을 리스트 생성
        var hits = new List<RaycastResult>();

        // 5. 실제로 UI 레이캐스트를 발사하여 마우스 커서 아래에 있는 모든 UI를 hits 리스트에 담음
        EventSystem.current.RaycastAll(data, hits);

        // 6. 검출된 UI가 1개 이상이면 true(입력 차단), 허공을 클릭했으면 false(정상 공격/동작) 반환
        return hits.Count > 0;
    }
}