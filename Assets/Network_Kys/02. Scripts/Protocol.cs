/// <summary> 헤더 프로토콜 타입 </summary>
public enum PROTOCOL : ushort
{
    ROOM_REQUEST = 0,  // 방 목록 받음
    ROOM_CREATE,       // 방 목록 생성 결과
    ROOM_JOIN,         // 방 참가
    ROOM_EXIT,         // 방 퇴장
    GAME_DO,           // 상대방 착수 결과
    GAME_WIN,          // 상대방 승리 Alert
    GAME_START,        // 게임 시작
}

public enum GameResultState : byte
{
    Win = 0,
    Lose,
    Draw
}

/// <summary> 회원가입 및 로그인 응답 타입 </summary>
public enum ResponseType : int
{
    INVALID_USERNAME = 0, // ID가 중복 혹은 잘못됐을 시
    INVALID_PASSWORD,     // Password가 잘못됐을 시 
    SUCCESS,             // 성공
}