using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace UI
{
    public class SelectLocationScreenView : ScreenViewWithCommonPayload<SelectLocationScreen>
    {
        public UnityEngine.UI.Button BackToChooseModeButton;

        public UnityEngine.UI.Button ContinueButton;

        public UnityEngine.UI.Button SelectButton;

        public UnityEngine.UI.Button BuyCoinsButton;

        private SelectLocationScreen _selectLocationScreen;

        public ScrollSnap ScrollSnap;

        public UnityEngine.UI.Button LeftButton;

        public UnityEngine.UI.Button RightButton;

        public override void Init(SelectLocationScreen screen)
        {
            BuyCoinsButton.gameObject.SetActive(false);

            BackToChooseModeButton.onClick.AddListener(new UnityAction(screen.BackToChooseModeButton.PressedAction));
            ContinueButton.onClick.AddListener(() =>
            {
                ScrollSnap.CurrentObject.GetComponent<LocationItem>().Button.onClick?.Invoke();
            });
            SelectButton.onClick.AddListener(new UnityAction(screen.SelectButton.PressedAction));
            BuyCoinsButton.onClick.AddListener(() =>
            {
                Game.Instance.ScreenManager.Show<BuyCoinsScreen>(true, true);
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            });
            LeftButton.onClick.AddListener(() =>
            {
                if (ScrollSnap.StartIndex < ScrollSnap.CurrentIndex)
                {
                    ScrollSnap.ScrollToLeft();
                }
            });
            RightButton.onClick.AddListener(() =>
            {
                if (ScrollSnap.CurrentIndex < ScrollSnap.EndIndex)
                {
                    ScrollSnap.ScrollToRight();
                }
            });

            ScrollSnap.SnapEvent += i =>
            {
                EventSystem.current.SetSelectedGameObject(ScrollSnap.CurrentObject.GetComponent<LocationItem>().Button.gameObject);
            };
        }

        private void InsertDummies(int count)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var obj = Resources.Load<GameObject>("TownItemDummy");
                    obj = Instantiate(obj, ScrollSnap._content);
                }
            }
        }

        public override void PreShow(CommonPayloadData payload)
        {
            foreach (Transform child in ScrollSnap._content)
            {
                Destroy(child.gameObject);
            }
            //InsertDummies(10);
            int count = 0;
            foreach (var type in LocationManager.Instance.locations.Keys)
            {
                count++;
                var info = LocationManager.Instance.GetLocationInfo(type, UserDataManager.RuntimeInfo.LocationModeType);
                var locationItem = Resources.Load<LocationItem>("TownItem");
                locationItem = Instantiate(locationItem, ScrollSnap._content);
                if (IsItemLocked(UserDataManager.Instance, info.UnlockInfo, info.Name))
                {
                    locationItem.Lock.gameObject.SetActive(true);
                }
                else
                {
                    locationItem.Lock.gameObject.SetActive(false);
                }
                string name = info.Name.Replace("_HUNTER", "");
                CoroutineRunner.Instance.Run(ResourcesLoader.LoadLocationSpriteAsync(name, loaded =>
                {
                    locationItem.IconLocation.sprite = loaded;
                }));
                if (UserDataManager.RuntimeInfo.IsHunterMode)
                {
                    locationItem.HunterCaption.gameObject.SetActive(true);
                }
                else
                {
                    locationItem.HunterCaption.gameObject.SetActive(false);
                }
                locationItem.Caption.text = LocalizationManager.Instance.GetTranslationByID(info.Name);
                int stars = 0;
                foreach (var mode in info.LocationInfos.Keys)
                {
                    foreach (var story in info.LocationInfos[mode])
                    {
                        stars += 3;
                    }
                }
                locationItem.StarsCounter.text = string.Format("{0}/{1}", UserDataManager.Instance.GameStats.GetLocationStars(UserDataManager.RuntimeInfo.LocationModeType, type), stars);
                locationItem.Button.onClick.AddListener(() =>
                {
                    if (!IsItemLocked(UserDataManager.Instance, info.UnlockInfo, info.Name))
                    {
                        UserDataManager.RuntimeInfo.CurentLocationType = type;
                        Game.Instance.ScreenManager.Show<SelectStoryScreen>(true, false);
                        SoundsManager.Instance.PlaySounds(SoundType.ui_click);
                    }
                    else
                    {
                        if (info.UnlockInfo.StarsLocationUnlockInfo == null)
                        {
                            var locationPayload = new BuyLocationPayloadData(info.Name, info.UnlockInfo.Price);
                            Game.Instance.ScreenManager.Popup<BuyLocationPopup, BuyLocationPayloadData>(locationPayload);
                        }
                        else
                        {
                            var mode = UserDataManager.RuntimeInfo.LocationModeType == LocationModeType.Classic ? LocalizationManager.Instance.GetTranslation("item_mode_story") : LocalizationManager.Instance.GetTranslation("item_mode_hunter");
                            var caption = LocalizationManager.Instance.GetTranslationByID(info.UnlockInfo.StarsLocationUnlockInfo.StoryId) + " (" + mode + ")";

                            var unlockStoryPayload = new UnlockStoryPopupPayloadData(info.UnlockInfo.StarsLocationUnlockInfo.Stars, info.UnlockInfo.Price, LocalizationManager.Instance.GetTranslationByID(info.Name), caption, () =>
                            {
                                UserDataManager.Instance.ShopData.Add(info.Name, 1, false);
                            });
                            Game.Instance.ScreenManager.Popup<UnlockStoryPopup, UnlockStoryPopupPayloadData>(unlockStoryPayload);
                        }
                    }
                });
            }
            //InsertDummies(10);
            ScrollSnap.StartIndex = 0;
            ScrollSnap.EndIndex = count -1;

            ScrollSnap._childOffset = 0;
        }

        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Return))
        //     {
        //         ContinueButton.onClick.Invoke();
        //     }
        // }

        public override void PostShow(CommonPayloadData payload)
        {
            ScrollSnap.Recalculate();
            ScrollSnap.Snap(0, true);

            
            // actions.UI.Enter.performed += _ => ContinueButton.onClick?.Invoke();
        }

        public override void SetSelectedGO()
        {
            EventSystem.current.SetSelectedGameObject(ScrollSnap.CurrentObject.GetComponent<LocationItem>().Button.gameObject);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            ScrollSnap.enabled = true;
        }

        public override void OnDisable()
        {
            base.OnDisable();
            ScrollSnap.enabled = false;
        }

        public static bool IsItemLocked(UserDataManager playerData, UnlockInfo unlockInfo, string itemName, StoryInfo currentStory = null)
        {
            if (unlockInfo != null && !playerData.ShopData.Contains(itemName))
            {
                if (unlockInfo.StarsLocationUnlockInfo == null)
                {
                    return true;
                }
                int stars = 0;
                if (string.IsNullOrEmpty(unlockInfo.StarsLocationUnlockInfo.StoryId))
                {
                    var locationLocator = unlockInfo.StarsLocationUnlockInfo.LocationLocator;
                    if (locationLocator == null)
                    {
                        stars = playerData.GameStats.GetAllStars();
                    }
                    else
                    {
                        stars = playerData.GameStats.GetLocationStars(locationLocator.LocationModeType, locationLocator.LocationType);
                    }
                }
                else
                {
                    stars = playerData.GameStats.GetStarsCount(unlockInfo.StarsLocationUnlockInfo.StoryId);
                }
                return stars < unlockInfo.StarsLocationUnlockInfo.Stars;
            }
            return false;
        }

        public override void Back()
        {
            BackToChooseModeButton.onClick?.Invoke();
        }
    }
}
