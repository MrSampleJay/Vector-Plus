using System;
using System.Collections.Generic;
using Core._Common;
using DG.Tweening;
using Nekki.Vector.Core;
using Nekki.Vector.Core.Camera;
using Nekki.Vector.Core.Controllers;
using Nekki.Vector.Core.Gadgets;
using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Models;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class LevelMainController
{
    public static class Layer
    {
        public const float Base = -1f;

        public const float Debug = -10f;

        public const float Player = -15f;
    }

    protected Location _Location;

    protected LocationCamera _Camera;

    private static bool _isHunterMode;

    private bool _pauseRender;

    private bool _tutorialPause;

    private uint _FrameCount;

    private bool _inputBeen;

    private ControllerGadgets _controllerGadgets;

    public Model debugModel;

    private string _trackIDForAnalytic;

    private bool _canShowAd;

    private int _consecutiveLossesMax = 3;
    private int _consecutiveLosses;
    private float _lastInputTime;
    private bool _pauseAfterReload;

    private const float AFK_DELAY = 10f;

    public static LevelMainController current
    {
        get;
        private set;
    }

    public LevelSceneController levelSceneController
    {
        get;
        private set;
    }

    public Location Location => _Location;

    public LocationCamera Camera => _Camera;

    private ModelHuman UserModel
    {
        get
        {
            foreach (var model in _Location.Models)
            {
                if (model.IsBot)
                {
                    continue;
                }
                return model;
            }
            return null;
        }
    }

    public bool slowMode
    {
        set
        {
            float num = 10;
            if (!value)
            {
                num = 1;
            }
            slowModeFrames = num;
        }
    }

    public float slowModeFrames
    {
        get;
        private set;
    }

    public static bool IsHunterMode => _isHunterMode;

    public bool pauseRender
    {
        get => _pauseRender;
        set
        {
            _pauseRender = value;
            levelSceneController.SetVisibleTutorialOnPause(value);
        }
    }

    public bool tutorialPause => _tutorialPause;

    public uint FrameCount => _FrameCount;

    public bool CanPauseOrReload
    {
        get;
        private set;
    }

    public bool IsDeath
    {
        get;
        private set;
    }

    public bool IsWin
    {
        get;
        private set;
    }

    public int SkipPlayerFrames
    {
        get;
        set;
    }

    public ControllerGadgets controllerGadgets => _controllerGadgets;

    public static void Init(LevelSceneController levelSceneController)
    {
        _isHunterMode = UserDataManager.RuntimeInfo.IsHunterMode;
        current = new LevelMainController();
        current.slowModeFrames = 1;
        current.levelSceneController = levelSceneController;
        current.InitLevel();
        current.pauseRender = true;
    }

    public void InitLevel()
    {
        _consecutiveLosses = 0;
        _lastInputTime = Time.unscaledTime;
        _canShowAd = false;
        _Location = CreateLocation();
        AddModeles();
        InitCoins();
        CreateCamera();
        _controllerGadgets = new ControllerGadgets();
        SoundsManager.Instance.PlayBackground(_Location.Music);
        _Location.Start();
        CanPauseOrReload = true;
        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    private Location CreateLocation()
    {
        var filePath = CurrentTrackInfo.Current.LocationFile;
        var prefabPath = VectorPaths.LevelsPrefab;
        var xmlPath = VectorPaths.XmlLevels;
        if (Game.Instance.Snail)
        {
            filePath = Game.Instance.SnailSett.SnailLevel;
        }
        return new Location(xmlPath, filePath, prefabPath);
    }

    private void AddModeles()
    {
        foreach (var data in _Location.UserData)
        {
            _Location.CreateModelHuman(data);
        }
    }

    private void InitCoins()
    {
        var coinrunners = Location.GetAllCoins();
        CoinSetter.SetCoins(coinrunners, Location.Sets.TotalCoins);
        CoinsSystem.CurrentType = CoinsSystemType.CST_DEFAULT;
        var settings = LocationManager.Instance.coinsSystem.GetCoinsSettings(CoinsSystem.CurrentType, CoinType.CT_DEFAULT);
        var settings1 = LocationManager.Instance.coinsSystem.GetCoinsSettings(CoinsSystem.CurrentType, CoinType.CT_BRONZE);
        var settings2 = LocationManager.Instance.coinsSystem.GetCoinsSettings(CoinsSystem.CurrentType, CoinType.CT_GOLD);

        List<CoinRunner> activeCoins = new List<CoinRunner>(coinrunners);
        foreach (var coinRunner in coinrunners)
        {
            activeCoins.Add(coinRunner);
            InitCoins(activeCoins, settings2, new Color(1, 0.8117647f, 0.28235295f));
            InitCoins(activeCoins, settings1, new Color(0.9647059f, 0.2901961f, 0.27450982f));
        }
    }

    private static void InitCoins(List<CoinRunner> activeCoins, CoinsSettings coinsSettings, Color color)
    {
        if (coinsSettings.count > 0)
        {
            for (int i = 0; i < coinsSettings.count; i++)
            {
                var random = Random.Range(0, 100);
                if (random % 100 < coinsSettings.chance * 100)
                {
                    var index = Random.Range(0, activeCoins.Count);
                    var coin = activeCoins[index];
                    coin.color = color;
                    coin.score = coinsSettings.nominal;
                    activeCoins.RemoveAt(index);
                }
            }
        }
    }

    private void CreateCamera()
    {
        _Camera = new LocationCamera();
        _Camera.Init();
        _Camera.Layers(_Location.Sets);
        _Camera.Zooming(LocationCamera.CurrentZoom, true);
        _Camera.Update();
    }

    public void Render()
    {
        CheckAfkPause();

        if (!_pauseRender && !_tutorialPause)
        {
            _Camera.UpdatePosition();
            _Location.Render();
            _FrameCount++;
        }
    }

    public void HandleNewInput(KeyVariables key)
    {
        _inputBeen = true;

        _lastInputTime = Time.unscaledTime;

        UserModel.ControllerKeys.SetKeyVariable(key);
    }

    private void CheckAfkPause()
    {
        if (_consecutiveLosses < _consecutiveLossesMax)
            return;

        float inactiveTime =
            Time.unscaledTime - _lastInputTime;

        if (inactiveTime < AFK_DELAY)
            return;

        // already scheduled
        if (_pauseAfterReload)
            return;

        _pauseAfterReload = true;

        Debug.Log(
            $"AFK due to {_consecutiveLosses}/{_consecutiveLossesMax} consecutive non-Win EndGames and no key presses"
        );
    }

    public void TutorialAreaActivate(TutorialAreaRunner area)
    {
        slowModeFrames = 10;
        levelSceneController.ShowTutorialUIController(area.Key, area.Description);
    }

    public void TutorialLockGame()
    {
        _tutorialPause = true;
    }

    public void TutorialUnLockGame()
    {
        levelSceneController.HideTutorialUIController();
        _tutorialPause = false;
        slowModeFrames = 1;
    }

    public bool HasCurrentLocationEverBeenCompleted()
    {
        var info = UserDataManager.Instance.CurrentBalanceLocation.CurrentStoryModeStoryInfos[UserDataManager.RuntimeInfo.CurrentStory];
        var stats = UserDataManager.Instance.GameStats.GetTrackStats(info.Name);
        if (stats != null)
        {
            return stats.Stars > 0;
        }
        return false;
    }

    public void RefreshTricks()
    {
        _Location.RefreshTricks();
    }

    public void Win(ModelHuman modelHuman, float time)
    {
        if (IsWin || IsDeath)
        {
            return;
        }
        modelHuman.IsGadget = false;
        IsWin = true;
        CanPauseOrReload = false;
        UserDataManager.Instance.Stats.Add(modelHuman.controllerStatistics);
        UserDataManager.Instance.SaveUserDate();
        LevelResult.LastLevelResult = new LevelResult(_Location, _FrameCount);
        var s = DOTween.Sequence();
        s.AppendInterval(time);
        s.AppendCallback(() =>
        {
            if (Game.Instance.Snail)
            {
                Reload();
                return;
            }
            GoToWinScene();
        });
        s.Play();
    }

    public void GoToWinScene()
    {
        ClearScene();
        var storyInfo = UserDataManager.Instance.CurrentBalanceLocation.CurrentStoryModeStoryInfos[UserDataManager.RuntimeInfo.CurrentStory];
        if (storyInfo.CutsceneEnd != null)
        {
            var data = new VideoScreenPayloadData(storyInfo.CutsceneEnd, () =>
            {
                Game.Instance.ScreenManager.Show<PostGameplayWinScreen>(true, false);
            });
            Game.Instance.ScreenManager.Show<VideoScreen, VideoScreenPayloadData>(data, true, false);
            return;
        }
        Game.Instance.ScreenManager.Show<PostGameplayWinScreen>(true, false);
    }

    public void Murder(ModelHuman modelHuman)
    {
        if (!IsWin && !IsDeath)
        {
            {
                _consecutiveLosses++;
                CanPauseOrReload = false;
                modelHuman.IsGadget = false;
                modelHuman.Death(GameEndType.GE_MURDER);
                Debug.Log($"Murder [{_consecutiveLosses}/{_consecutiveLossesMax}]");
                var s = DOTween.Sequence();
                s.AppendInterval(0.5f);
                s.AppendCallback(() => { Reload(); });
                s.Play();
                IsDeath = true;
            }
        }
    }

    public void Death(ModelHuman modelHuman, float time)
    {
        if (!IsWin && !IsDeath)
        {
            if (!modelHuman.IsBot)
            {
                _consecutiveLosses++;
                CanPauseOrReload = false;
                modelHuman.IsGadget = false;
                modelHuman.Death(GameEndType.GE_DEATH);
                modelHuman.StartPhysics();
                Debug.Log($"Death [{_consecutiveLosses}/{_consecutiveLossesMax}]");
                var s = DOTween.Sequence();
                s.AppendInterval(time);
                s.AppendCallback(() => { Reload(); });
                s.Play();
                IsDeath = true;
            }
        }
    }

    public void Loss(ModelHuman modelHuman, float time)
    {
        if (!IsWin && !IsDeath)
        {
            {
                _consecutiveLosses++;
                LocationCamera.Current.Stop();
                CanPauseOrReload = false;
                modelHuman.IsGadget = false;
                modelHuman.Death(GameEndType.GE_LOSS);
                Debug.Log($"Loss [{_consecutiveLosses}/{_consecutiveLossesMax}]");
                var s = DOTween.Sequence();
                s.AppendInterval(time);
                s.AppendCallback(() => { Reload(); });
                s.Play();
                IsDeath = true;
            }
        }
    }

    public void Arrest(ModelHuman modelHuman)
    {
        if (!IsWin && !IsDeath)
        {
            _consecutiveLosses++;
            CanPauseOrReload = false;
            modelHuman.IsGadget = false;
            Debug.Log($"Arrest [{_consecutiveLosses}/{_consecutiveLossesMax}]");
            var s = DOTween.Sequence();
            s.AppendInterval(1);
            s.AppendCallback(() => { Reload(); });
            s.Play();
            IsDeath = true;
        }
    }

    public void ClearScene()
    {

        Object.Destroy(GameObject.Find("Level_root_object"));

        ResourceManager.ClearTextureCache();
        AtlasDecoder.ClearCache();

        SceneManager.UnloadSceneAsync("Level");

        Resources.UnloadUnusedAssets();
        GC.Collect();
    }

    public void ReloadButton()
    {
        Reload();
    }

    protected void Reload()
    {
        Debug.Log("Reload");
        _Location.Reload();
        TutorialUnLockGame();
        InitCoins();
        _Camera.Reset();
        _Camera.Zooming(LocationCamera.CurrentZoom, true);
        _Location.Start();
        _Camera.Update();
        _pauseRender = false;
        IsWin = false;
        slowModeFrames = 1;
        CanPauseOrReload = true;
        IsDeath = false;
        _FrameCount = 0;
if (_Location.ResetMusicOnReload)
{
    SoundsManager.Instance.StopBackground();
    SoundsManager.Instance.PlayBackground(_Location.Music, true);
}

        if (_pauseAfterReload)
        {
            _pauseAfterReload = false;
            Game.Instance.ScreenManager.Show<GameplayPauseScreen>(false, false);
            pauseRender = true;
        }
    }

    public List<ModelHuman> GetModelsByNames(List<string> modelHumanNames)
    {
        var list = new List<ModelHuman>();
        foreach (var modelName in modelHumanNames)
        {
            var model = GetModelByName(modelName);
            if (model != null)
                list.Add(model);
        }
        return list;
    }

    public ModelHuman GetModelByName(string name)
    {
        return _Location.GetModelByName(name);
    }

    public void SelectDebugModel()
    {
    }

    private void CheckInputBeen()
    {
        if (_inputBeen)
        {
            TryShowAd();
            _inputBeen = false;
            return;
        }
        Game.Instance.ScreenManager.Show<GameplayPauseScreen>(false, false);
        pauseRender = true;
    }

    private void TryShowAd()
    {
        //AD RELATED
    }

    private void OnInterstitialEnd()
    {
        UIBlocker.UnBlock();
        pauseRender = false;
    }

    public static void Clear()
    {
        //FirebaseHandler.instance.SetCustomKey("Track", "-");
        current = null;
    }
}
