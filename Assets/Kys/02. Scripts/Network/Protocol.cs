/// <summary> 헤더 프로토콜 타입 </summary>
public enum PROTOCOL : ushort
{
    ROOM_REQUEST = 0,  // 방 목록 받음
    ROOM_CREATE,       // 방 목록 생성 결과
    ROOM_JOIN,         // 방 참가
    ROOM_EXIT,         // 방 퇴장
    GAME_MOVE_REQ,     // 게스트 수 좌표 REQ
    GAME_MOVE_COM,     // 호스트가 수 확정 후 Broad Cast
    GAME_RESULT,       // 호스트가 결과 확인 후 Broad Cast
    GAME_START,        // 호스트가 게임 시작 

    LOBBY_CHAT,        // 로비 채팅
    ROOM_CHAT,         // 방 채팅
    SERVER_BRC         // 서버 브로드캐스팅
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
    SUCCESS,              // 성공
}