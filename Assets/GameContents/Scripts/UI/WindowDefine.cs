/// <summary>
/// 각각의 윈도우 페이지가 하나씩 소유하는 아이디
/// 리소스 네임과 일치시켜야 한다
/// 
/// 2017. 06. 03. 클라이언트 김선재
/// </summary>

namespace Game
{
    /// <summary>
    /// 프리펩 네이밍과 동일해야함
    /// 
    /// 해당 프리펩은 Assets/Resources/AB_UI에 위치할 것
    /// 
    /// 일부 WindowManager->IsSystemUI에서 True값을 리턴하는 UI의 경우
    /// Assets/Resources/System_UI에 위치할 것
    /// </summary>
    public enum WindowID
    {
        NONE,

        // System
        Window_System_Intro,

        // Normal
        Window_NickName_Popup,
        Window_LobbyMain,

        Window_RoomList,
        Window_RoomCreate_Popup,
        Window_Room,

        Window_BattleMain,
        Window_BattleStart,
        Window_BattleEnd,
        Window_BattlePause,

        Window_Settings
    }

    /// <summary>
    /// 프리펩 네이밍과 동일해야함
    /// 
    /// 해당 프리펩은 AB_UI_Loading에 위치할 것
    /// </summary>
    public enum LoadingID
    {
        Loading_FadeInOut,
        Loading_Box_Scale,
        Loading_ScreenShotFadeOut,
    }

    /// <summary>
    /// 프리펩 네이밍과 동일해야함
    /// 
    /// 해당 프리펩은 AB_UI_Background에 위치할 것
    /// </summary>
    public enum BackgroundID
    {
        BG_None,  // 프리펩 존재하지 않음
        BG_Lobby,
    }
}