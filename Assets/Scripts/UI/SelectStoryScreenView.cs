using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class SelectStoryScreenView : ScreenViewWithCommonPayload<SelectStoryScreen>
    {
        public UnityEngine.UI.Button BackToSelectLocationButton;

        public UnityEngine.UI.Button PlayButton;

        public UnityEngine.UI.Toggle StoryButton;

        public UnityEngine.UI.Toggle BonusButton;

        public UnityEngine.UI.Button SelectButton;

        public UnityEngine.UI.Button BuyCoinsButton;

        public Text Header;

        public StoryItem StoryItemPrefab;

        public GameObject ContentParent;

        public ScrollSnap ScrollSnap;

        private SelectStoryScreen _selectStoryScreen;

        public Text Title;

        public LayoutPosKeeper LayoutPosKeeper;

        private Dictionary<StoryItem, bool> _dummies = new Dictionary<StoryItem, bool>();

        private List<StoryItem> _storiesItem = new List<StoryItem>();

        public override void Init(SelectStoryScreen screen)
        {
            BuyCoinsButton.gameObject.SetActive(false);

            _selectStoryScreen = screen;
            BackToSelectLocationButton.onClick.AddListener(new UnityAction(screen.BackToSelectLocationButton.PressedAction));
            StoryButton.onValueChanged.AddListener(isSelected =>
            {
                if (!isSelected || UserDataManager.RuntimeInfo.StoryType == StoryType.Story)
                {
                    return;
                }
                UserDataManager.RuntimeInfo.StoryType = StoryType.Story;
                Game.Instance.ScreenManager.Refresh();
            });
            BonusButton.onValueChanged.AddListener(isSelected =>
            {
                if (!isSelected || UserDataManager.RuntimeInfo.StoryType == StoryType.Bonus)
                {
                    return;
                }
                UserDataManager.RuntimeInfo.StoryType = StoryType.Bonus;
                Game.Instance.ScreenManager.Refresh();
            });
            BuyCoinsButton.onClick.AddListener(() =>
            {
                Game.Instance.ScreenManager.Show<BuyCoinsScreen>(true, true);
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            });
            PlayButton.onClick.AddListener(() =>
            {
                var currentStories = UserDataManager.Instance.CurrentBalanceLocation.CurrentStoryModeStoryInfos;
                var index = UserDataManager.RuntimeInfo.GetLastIndex();
                var story = currentStories[index];
                var storyItem = ScrollSnap.CurrentObject.GetComponent<StoryItem>();
                Play(story, storyItem, UserDataManager.Instance, index);
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            });
            ScrollSnap.SnapEvent += i =>
            {
                UserDataManager.RuntimeInfo.SetLastIndex(i);
                EventSystem.current.SetSelectedGameObject(_storiesItem[i].Button.gameObject);
            };
        }

        private void InsertDummies(int count, bool atStart)
        {
            for (int i = 0; i < count; i++)
            {
                StoryItem dummy = null;

                foreach (var kvp in _dummies.ToList())
                {
                    if (!kvp.Value && kvp.Key != null)
                    {
                        dummy = kvp.Key;
                        _dummies[dummy] = true;
                        break;
                    }
                }
                if (dummy == null)
                {
                    var prefab = Resources.Load<StoryItem>("StoryHolderDummy");
                    dummy = Instantiate(prefab, ContentParent.transform);
                    _dummies[dummy] = true;
                }


                var t = dummy.transform;
                if (t.parent != ContentParent.transform)
                    t.SetParent(ContentParent.transform, false);


                if (atStart)
                {
                    t.SetAsFirstSibling();

                    var layout = dummy.GetComponent<LayoutElement>();
                    if (!LayoutPosKeeper.left.Contains(layout))
                        LayoutPosKeeper.left.Add(layout);
                }
                else
                {
                    t.SetAsLastSibling();

                    var layout = dummy.GetComponent<LayoutElement>();
                    if (!LayoutPosKeeper.right.Contains(layout))
                        LayoutPosKeeper.right.Add(layout);

                }

                if (!dummy.gameObject.activeSelf)
                    dummy.gameObject.SetActive(true);
            }

            //for (int i = count; i > 0; i--)
            //{
            //    var obj = Instantiate(Resources.Load<LayoutElement>("StoryHolderDummy"), ContentParent.transform);
            //    _dummies.Add(obj.GetComponent<StoryItem>(), true);
            //    if (!atStart)
            //    {
            //        obj.transform.SetAsLastSibling();
            //    }
            //    else
            //    {
            //        obj.transform.SetAsFirstSibling();
            //        LayoutPosKeeper.left.Add(obj);
            //    }
            //}
        }

        // private void Update()
        // {
        //     if (Input.GetKeyDown(KeyCode.Return))
        //     {
        //         PlayButton.onClick.Invoke();
        //     }
        // }

        public override void PreShow(CommonPayloadData payload)
        {
            Title.text = LocalizationManager.Instance.GetTranslationByID(UserDataManager.Instance.CurrentBalanceLocation.Name);
            ScrollSnap.enabled = true;
            FillWithContent(UserDataManager.Instance, payload);
        }

        private StoryItem GetStoryItem(int index)
        {
            var item = _storiesItem.ElementAtOrDefault(index);
            if (item == null)
            {
                var obj = Resources.Load<StoryItem>("StoryHolder");
                item = Instantiate(obj, ContentParent.transform);
                _storiesItem.Add(item);
            }
            item.gameObject.SetActive(true);
            return item;
        }

        private void FillWithContent(UserDataManager playerData, CommonPayloadData payload)
        {

            var currentStoryModeInfos = UserDataManager.Instance.CurrentBalanceLocation.CurrentStoryModeStoryInfos;

            foreach (var dummy in _dummies.Keys.ToList())
            {
                _dummies[dummy] = false;
            }

            InsertDummies(10, true);
            for (int i = 0; i < currentStoryModeInfos.Count; i++)
            {
                var storyInfo = currentStoryModeInfos[i];
                var storyItem = GetStoryItem(i);
                CoroutineRunner.Instance.Run(ResourcesLoader.LoadStoriesSpriteAsync(storyInfo.Name.Replace("_HUNTER", ""), loaded => storyItem.Icon.sprite = loaded));
                var points = playerData.GameStats.GetPointsCount(storyInfo.Name);
                if (points < 1)
                {
                    storyItem.Points.gameObject.SetActive(false);
                }
                else
                {
                    storyItem.Points.gameObject.SetActive(true);
                }
                storyItem.Points.text = points.ToString();
                storyItem.Lock.SetActive(SelectLocationScreenView.IsItemLocked(playerData, storyInfo.UnlockInfo, storyInfo.Name, storyInfo));
                storyItem.Button.onClick.RemoveAllListeners();
                int index = i;
                storyItem.Button.onClick.AddListener(() =>
                {
                    Play(storyInfo, storyItem, playerData, index);
                });
                storyItem.StarsView.Set(playerData.GameStats.GetStarsCount(storyInfo.Name));
                storyItem.Caption.text = LocalizationManager.Instance.GetTranslationByID(storyInfo.Name);
                storyItem.HunterCaption.SetActive(UserDataManager.RuntimeInfo.IsHunterMode);

            }
            if (_storiesItem.Count >= currentStoryModeInfos.Count)
            {
                for (int i = currentStoryModeInfos.Count; i < _storiesItem.Count; i++)
                {
                    var extra = _storiesItem[i];
                    if (extra == null) continue;

                    extra.gameObject.SetActive(false);
                    //InsertDummies(1, false);
                }
            }
            InsertDummies(10, false);
            ScrollSnap.StartIndex = 0;
            ScrollSnap.EndIndex = currentStoryModeInfos.Count - 1;

            ScrollSnap._childOffset = 10;
        }

        private void Play(StoryInfo story, StoryItem storyItem, UserDataManager playerData, int index)
        {
            Debug.Log(story.Name);
            var scrollSnap = storyItem.GetComponent<ScrollSnapItem>();
            if (scrollSnap != null)
            {
                if (!scrollSnap.IsSelected)
                {
                    ScrollSnap.Snap(index, false);
                    return;
                }
            }
            if (SelectLocationScreenView.IsItemLocked(playerData, story.UnlockInfo, story.Name, story))
            {
                var mode = UserDataManager.RuntimeInfo.LocationModeType == LocationModeType.Classic ? LocalizationManager.Instance.GetTranslation("item_mode_story") : LocalizationManager.Instance.GetTranslation("item_mode_hunter");
                var caption = LocalizationManager.Instance.GetTranslationByID(UserDataManager.Instance.CurrentBalanceLocation.Name) + " (" + mode + ")";
                var payload = new UnlockStoryPopupPayloadData(story.UnlockInfo.StarsLocationUnlockInfo.Stars, story.UnlockInfo.Price, LocalizationManager.Instance.GetTranslationByID(story.Name), caption, () =>
                {
                    playerData.ShopData.Add(story.Name, 1, false);
                });

                Game.Instance.ScreenManager.Popup<UnlockStoryPopup, UnlockStoryPopupPayloadData>(payload);
            }
            else
            {
                UserDataManager.RuntimeInfo.CurrentStory = index;
                _selectStoryScreen.SelectButton.PressedAction?.Invoke();
            }
        }

        public override void PostShow(CommonPayloadData payload)
        {
            base.PostShow(payload);

            LayoutPosKeeper.SetPositions();

            ScrollSnap.Recalculate();

            ScrollSnap.Snap(UserDataManager.RuntimeInfo.GetLastIndex(), true);
            
            // actions.UI.Enter.performed += _ => PlayButton.onClick?.Invoke();
        }

        public override void SetSelectedGO()
        {
            EventSystem.current.SetSelectedGameObject(_storiesItem[ScrollSnap.CurrentIndex].Button.gameObject);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            ScrollSnap.enabled = true;
            
            actions.UI.XButton.performed += _ =>
            {
                if (UserDataManager.RuntimeInfo.StoryType == StoryType.Story)
                {
                    BonusButton.isOn = true;
                }
                else
                {
                    StoryButton.isOn = true;
                }
            };
        }

        public override void OnDisable()
        {
            base.OnDisable();
            ScrollSnap.enabled = false;
        }

        public override void Back()
        {
            BackToSelectLocationButton.onClick?.Invoke();
        }
    }
}
