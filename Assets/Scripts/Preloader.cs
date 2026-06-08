using System;
using System.Collections;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using Nekki.Vector.Core.Animation;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Preloader : MonoBehaviour
{

    [SerializeField]
    private ProgressBar _progressBar;

    [SerializeField]
    private GDPRPopupView _gdprPopup;

    [SerializeField]
    private GameObject _preloaderUIGO;

    [DllImport("user32.dll", EntryPoint = "SetWindowText")]
    private static extern bool SetWindowText(IntPtr hWnd, string text);

    [DllImport("user32.dll", EntryPoint = "FindWindow")]
    private static extern IntPtr FindWindow(string className, string windowName);

    private void Awake()
    {
        string[] args = Environment.GetCommandLineArgs();
        int levelArg = Array.IndexOf(args, "-level");
        if (levelArg >= 0 && levelArg < args.Length - 1)
        {
            string levelName = args[levelArg + 1];
            Game.Instance.Snail = true;
            Game.Instance.SnailSett.SnailLevel = levelName;

            IntPtr windowPtr = FindWindow(null, Application.productName);
            if (windowPtr != IntPtr.Zero)
            {
                SetWindowText(windowPtr, "Snail Runner");
            }

            int showUIArg = Array.IndexOf(args, "-noui");
            if (showUIArg >= 0)
            {
                Game.Instance.SnailSett.ShowUI = false;
            }
            int hunterModeArg = Array.IndexOf(args, "-huntermode");
            if (hunterModeArg >= 0)
            {
                Game.Instance.SnailSett.HunterMode = true;
            }

            int triggerArg = Array.IndexOf(args, "-showtriggers");
            if (triggerArg >= 0)
            {
                Game.Instance.SnailSett.ShowTriggers = true;
            }
            int platformArg = Array.IndexOf(args, "-showplatforms");
            if (platformArg >= 0)
            {
                Game.Instance.SnailSett.ShowPlatforms = true;
            }

            int areaArg = Array.IndexOf(args, "-showareas");
            if (areaArg >= 0)
            {
                Game.Instance.SnailSett.ShowAreas = true;
            }

            int detectorArg = Array.IndexOf(args, "-showdetectors");
            if (detectorArg >= 0)
            {
                Game.Instance.SnailSett.ShowDetectors = true;
            }
        }
        //Game.Instance.SnailSett.HunterMode = true;
        //Game.Instance.Snail = true;
        //Game.Instance.SnailSett.SnailLevel = "CONSTRUCTION_BONUS_01";
        //Game.Instance.SnailSett.ShowPlatforms = true;
        //Game.Instance.SnailSett.ShowAreas = true;
        // Game.Instance.SnailSett.ShowTriggers = true;
        //Game.Instance.SnailSett.ShowDetectors = true;

        //Game.Instance.SnailSett.UsePrefab = true;

        DontDestroyOnLoad(gameObject);
        //AbstractManager<ConfigManager>.Init(); AD RELATED
        LocalizationManager.Init();
        ResolutionManager.Init();
        UserDataManager.Init();
        LicenseCheckingManager.Init();

        LicenseCheckingManager.Instance.RunCheck();
        StartCoroutine(LicenseChecking());
    }

    public IEnumerator LicenseChecking()
    {
        if (UserDataManager.Instance.MainData == null)
        {
            Application.Quit();
            yield
            break;
        }
        if (UserDataManager.Instance.Options.GDPR)
        {
            StartCoroutine(LoadProcess());
            yield
            break;
        }
        _preloaderUIGO.SetActive(false);
        _gdprPopup.Init(() =>
        {
            _gdprPopup.gameObject.SetActive(false);
            _preloaderUIGO.gameObject.SetActive(true);
            UserDataManager.Instance.Options.GDPR = true;
            UserDataManager.Instance.Options.SaveData();
            StartCoroutine(LoadProcess());
        });
    }

    private void GDPRConfirm() { }

    public IEnumerator LoadProcess()
    {
        yield return new WaitForEndOfFrame();
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
        {
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1;
        }
        else
        {
            Application.targetFrameRate = 60;
        }
        _progressBar.SetValue(0.10f);
        yield return null;
        StoreManager.Init();
        LocationManager.Init();
        _progressBar.SetValue(0.15f);
        yield return null;
        AnimationLoader.Current.Init();
        _progressBar.SetValue(0.20f);
        yield return null;
        var animations = Animations.ToList();
        int count = animations.Count;
        for (int i = 0; i < count; i++)
        {
            animations[i].LoadBin();
            if ((i + 1) % 20 == 0 || i == count - 1)
            {
                float t = (i + 1) / (float)count;
                float value = 0.20f + 0.60f * t;
                _progressBar.SetValue(value);

                yield return null;
            }
        }

        if (count == 0)
        {
            _progressBar.SetValue(0.80f);
            yield return null;
        }
        GC.Collect();
        yield return null;
        SoundsManager.Instance.Init();

        if (Game.Instance.Snail)
        {
            if (Game.Instance.SnailSett.HunterMode)
            {
                UserDataManager.RuntimeInfo.IsHunterMode = true;
                UserDataManager.Instance.SetHunterMode();
            }

            var load = SceneManager.LoadSceneAsync("Level", LoadSceneMode.Single);
            while (!load.isDone)
            {
                var progress = Mathf.Clamp01(load.progress / 0.9f);
                _progressBar.SetValue(progress * 0.2f + 0.8f);
                yield return null;
            }

            yield return new WaitUntil(() => LevelMainController.current != null);

            LevelMainController.current.pauseRender = false;

            yield break;
        }

        var asyncLoad = SceneManager.LoadSceneAsync("UI", LoadSceneMode.Single);
        while (!asyncLoad.isDone)
        {
            var progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            _progressBar.SetValue(progress * 0.2f + 0.8f);
            yield return null;
        }

        StartCoroutine(Game.Instance.Start());
    }

}