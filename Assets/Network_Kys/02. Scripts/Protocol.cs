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