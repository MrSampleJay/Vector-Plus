using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game
{
    private const string PaidApplicationIDAndroid = "com.nekki.vector.paid";

    private const string PaidApplicationIDIOS = "com.nekki.vector.iphone";

    private static Game _instance;

    private bool _isInited;

    private bool _needShowRefreshApplication;

    public bool ForceShowStats = false;

    public ScreenManager ScreenManager;

    public class SnailSettings
    {
        public bool ShowUI = true;

        public string SnailLevel;

        public bool HunterMode = false;

        public bool ShowPlatforms = false;

        public bool ShowAreas = false;

        public bool ShowTriggers = false;

        public bool ShowDetectors = false;

        public bool UsePrefab = false;
    }

    public bool Snail = false;

    public SnailSettings SnailSett = new SnailSettings();

    public static bool IsPaidID => true;

    public static Game Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new Game();
            }
            return _instance;
        }
    }

    public static bool IsTrickBought(string trickId)
    {
        return UserDataManager.Instance.ShopData.IsBought(trickId);
    }

    public static bool IsItemBought(string itemId)
    {
        return UserDataManager.Instance.ShopData.IsBought(itemId);
    }

    public static bool IsItemEquipped(string itemId)
    {
        return UserDataManager.Instance.ShopData.IsEquipped(itemId);
    }

    public static bool IsHunterMode()
    {
        return UserDataManager.RuntimeInfo.IsHunterMode;
    }

    public IEnumerator Start()
    {
        ScreenManager = new ScreenManager();

        yield return new WaitForEndOfFrame();

        var payload = new VideoScreenPayloadData("intro.mp4", () =>
        {
            ScreenManager.Show<LobbyScreen>(true, false);
            SoundsManager.Instance.PlayBackground(MusicType.menu);
            _isInited = true;
            if (_needShowRefreshApplication)
            {
                ShowRefreshApplication();
            }
        });
        ScreenManager.Show<VideoScreen, VideoScreenPayloadData>(payload, true, false);
        BackButtonManager.Instance.OnBackButton += OnBackButton;

    }

    private void OnBackButton()
    {
        ScreenManager.Back();
    }

    public void ShowRefreshApplication()
    {
        if (_isInited)
        {
            _needShowRefreshApplication = false;
            var onPlay = new Action(() =>
            {
                UserDataManager.Instance.MainData.Unlock();
                UserDataManager.Instance.SaveUserDate();
                Utils.Utils.QuitGame();
            });
            ShowRefreshApplication(onPlay);
        }
    }

    private void ShowRefreshApplication(Action onPlay)
    {
        ScreenManager.ClosePopup();
        var title = LocalizationManager.Instance.GetTranslation("terms_of_use_title");
        var message = LocalizationManager.Instance.GetTranslation("buy_premium_desc");
        var ok = LocalizationManager.Instance.GetTranslation("ctrl_ok");
        var onClick = new Action<bool>(result =>
        {
            if (result)
                onPlay.Invoke();
        });

        var payload = new AlertPayloadData(title, message, ok, "", onClick);
        ScreenManager.Popup<AlertPopup, AlertPayloadData>(payload);
    }
}
