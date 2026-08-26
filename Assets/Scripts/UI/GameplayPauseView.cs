using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class GameplayPauseView : ScreenViewWithCommonPayload<GameplayPauseScreen>
    {
        public UnityEngine.UI.Button BackToLobbyButton;

        public UnityEngine.UI.Button BankButton;

        public UnityEngine.UI.Button MusicButton;

        public UnityEngine.UI.Button SoundButton;

        public GameObject MusicEnabledIcon;

        public GameObject MusicDisabledIcon;

        public GameObject SoundEnabledIcon;

        public GameObject SoundDisabledIcon;

        public UnityEngine.UI.Button ResumeButton;

        public UnityEngine.UI.Button ReplayButton;

        public UnityEngine.UI.Button BackToSelectionButton;

        public UnityEngine.UI.Button BuyGadgetsButton;

        public GameObject GadgetIcon;

        public Text Title;

        public Text LevelDescription;

        public Text TrackDescription1;

        public Text TrackDescription2;

        public ScrollSnap ScrollSnap;

        public UnityEngine.UI.Button SkipButton;

        private int _dummiesCount = 10;

        private bool snapSelected;

        public override void Init(GameplayPauseScreen screen)
        {
            BankButton.gameObject.SetActive(false);

            SkipButton.gameObject.SetActive(false);
            BackToLobbyButton.onClick.AddListener(new UnityAction(screen.BackToLobbyButton.PressedAction));
            ResumeButton.onClick.AddListener(() =>
            {
                Game.Instance.ScreenManager.Show<EmptyScreen>(false, false);
                Game.Instance.ScreenManager.Popup<CountdownPopup>();
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            });
            BankButton.onClick.AddListener(() =>
            {
                Game.Instance.ScreenManager.Show<BuyCoinsScreen>(true, true);
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            });
            BackToSelectionButton.onClick.AddListener(() =>
            {
                Game.Instance.ScreenManager.Popup<QuitStoryPopup>();
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            });
            BuyGadgetsButton.onClick.AddListener(() =>
            {
                Game.Instance.ScreenManager.Show<StoreGadgetsScreen, GadgetScreenPayloadData>(new GadgetScreenPayloadData(true), true, true);
            });
            MusicButton.onClick.AddListener(() =>
            {
                UserDataManager.Instance.Options.ToggleMusic();
            });
            SoundButton.onClick.AddListener(() =>
            {
                UserDataManager.Instance.Options.ToggleSound();
            });

            UserDataManager.Instance.Options.ToggleMusicEvent += musicLevel => { MusicDisabledIcon.SetActive(musicLevel <= 0); MusicEnabledIcon.SetActive(0 < musicLevel); };
            UserDataManager.Instance.Options.ToggleSoundEvent += soundLevel => { SoundDisabledIcon.SetActive(soundLevel <= 0); SoundEnabledIcon.SetActive(0 < soundLevel); };
            
            ScrollSnap.SnapEvent += i =>
            {
                EventSystem.current.SetSelectedGameObject(ScrollSnap.CurrentObject.GetComponent<HolderItem>().Button.gameObject);
            };
        }

        public override void PreShow(CommonPayloadData payload)
        {
            ReplayButton.onClick.RemoveAllListeners();
            ReplayButton.onClick.AddListener(() =>
            {
                Game.Instance.ScreenManager.Show<GameplayScreen>(true, false);
                LevelMainController.current.ReloadButton();
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            });
            var musicLevel = UserDataManager.Instance.Options.MusicLevel;
            MusicDisabledIcon.SetActive(musicLevel <= 0);
            MusicEnabledIcon.SetActive(0 < musicLevel);

            var soundLevel = UserDataManager.Instance.Options.SoundLevel;
            SoundDisabledIcon.SetActive(soundLevel <= 0);
            SoundEnabledIcon.SetActive(0 < soundLevel);

            var storyInfo = UserDataManager.Instance.CurrentBalanceLocation.CurrentStoryModeStoryInfos[UserDataManager.RuntimeInfo.CurrentStory];
            Title.text = LocalizationManager.Instance.GetTranslationByID(storyInfo.Name);

            if (storyInfo.Description != null)
            {
                TrackDescription1.gameObject.SetActive(false);
                TrackDescription2.gameObject.SetActive(false);
                LevelDescription.text = LocalizationManager.Instance.GetTranslation(storyInfo.Description);
                LevelDescription.gameObject.SetActive(true);
            }
            else
            {
                TrackDescription1.gameObject.SetActive(true);
                TrackDescription2.gameObject.SetActive(true);
                LevelDescription.gameObject.SetActive(false);
            }
            GadgetIcon.SetActive(UserDataManager.Instance.ShopData.IsEquipped("GADGET_FORCEBLASTER"));
            foreach (RectTransform obj in ScrollSnap._content)
            {
                Destroy(obj.gameObject);
            }
            var tricks = StoreManager.Instance.GetItems(StoreItemType.Tricks).Where(trick => storyInfo.TrickIds.Contains(trick.Id)).ToList();
            //StoreTricksScreenView.InsertEmptyDummies(ScrollSnap._content, _dummiesCount);
            StoreTricksScreenView.PutItemsIntoContent(ScrollSnap, tricks, StoreItemType.Tricks, false);
            //StoreTricksScreenView.InsertEmptyDummies(ScrollSnap._content, _dummiesCount);
            ScrollSnap.StartIndex = 0;
            ScrollSnap.EndIndex = tricks.Count - 1;

            ScrollSnap._childOffset = 0;
        }

        public override void PostShow(CommonPayloadData payload)
        {
            ScrollSnap.Recalculate();
            ScrollSnap.Snap(ScrollSnap.StartIndex, true);
        }

        public override void SetSelectedGO()
        {
            if (!GamepadController.Instance.IsGamepadConnected)
            {
                ScrollSnap.Enable();
                EventSystem.current.SetSelectedGameObject(ScrollSnap.CurrentObject.GetComponent<HolderItem>().Button.gameObject);
                return;
            }
            
            if (snapSelected)
            {
                ScrollSnap.Disable();
                EventSystem.current.SetSelectedGameObject(BackToSelectionButton.gameObject);
            }
            else
            {
                ScrollSnap.Enable();
                EventSystem.current.SetSelectedGameObject(ScrollSnap.CurrentObject.GetComponent<HolderItem>().Button.gameObject);
            }
            
        }

        public override void OnEnable()
        {
            base.OnEnable();
            ScrollSnap.enabled = true;
            
            actions.UI.XButton.performed += _ =>
            {
                snapSelected = !snapSelected;
                SetSelectedGO();
            };
            actions.UI.YButton.performed += _ => BuyGadgetsButton.onClick.Invoke();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            ScrollSnap.enabled = false;
        }

        public override void Back()
        {
            ResumeButton.onClick?.Invoke();
        }
    }
}
