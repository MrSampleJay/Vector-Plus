using DG.Tweening;
using Nekki.Vector.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
    public class PostGameplayWinView : ScreenViewWithCommonPayload<PostGameplayWinScreen>
    {
        public UnityEngine.UI.Button BackToLobbyButton;

        public UnityEngine.UI.Button ReplayButton;

        public UnityEngine.UI.Button PlayNextButton;

        public StarsView StarsView;

        public ItemStoryView Time;

        public ItemStoryView Score;

        public ItemStoryView Bonuses;

        public ItemStoryView Tricks;

        public Text Title;

        private int _starRecord;

        private int[] _coinsByStars;

        private bool _showRateDialog;

        private Sequence _starsSequence;

        public Color Color;

        public override void Init(PostGameplayWinScreen screen)
        {
            BackToLobbyButton.onClick.AddListener(() =>
            {
                GoBack();
            });
            ReplayButton.onClick.AddListener(() =>
            {
                Reload();
            });
            PlayNextButton.onClick.AddListener(() =>
            {
                PlayNext();
            });
        }

        public override void PreShow(CommonPayloadData payload)
        {
            var currentStory = UserDataManager.Instance.CurrentBalanceLocation.CurrentStoryModeStoryInfos[UserDataManager.RuntimeInfo.CurrentStory];
            var userData = UserDataManager.Instance;
            var stats = userData.GameStats.GetTrackStats(currentStory.Name);
            var res = LevelResult.LastLevelResult;

            _starRecord = 1;
            if (res.bonusCollected == res.bonusMax)
            {
                _starRecord = 2;
            }
            if (res.trickCollected == res.trickMax)
            {
                _starRecord++;
            }
            var record = new Record();
            record.Points = res.pointsCollected;
            record.Stars = _starRecord;
            record.TimeSpan = res.timeSpan;

            var stars = 0;
            if (stats != null)
            {
                stars = stats.Stars;
            }

            _coinsByStars = new int[3];

            if (userData.MainData.IsUnlock)
            {
                var rewardData = LocationManager.Instance.GetRewards(currentStory.RewardTemplate);
                var extension = rewardData.GetReward(stars, _starRecord);
                if (_starRecord > 0)
                {
                    for (int i = 0; i < _starRecord; i++)
                    {
                        if (i < stars)
                        {
                            _coinsByStars[i] = -1;
                        }
                        else
                        {
                            _coinsByStars[i] = rewardData[i + 1];
                        }
                    }
                }
                else
                {
                    extension = 0;
                }
                userData.MainData.AddCoins(extension);
            }
            Title.text = LocalizationManager.Instance.GetTranslationByID(currentStory.Name);
            UpdateTimeLabel(res, stats);
            UpdateScoreLabel(res, stats);
            Bonuses.Current.text = string.Format("{0}/{1}", res.bonusCollected, res.bonusMax);
            Tricks.Current.text = string.Format("{0}/{1}", res.trickCollected, res.trickMax);
            if (stats == null)
            {
                userData.GameStats.TrackStatsAdd(currentStory.Name, record);
            }
            else
            {
                var timeSpan = res.timeSpan;
                if (stats.TimeSpan > timeSpan)
                {
                    timeSpan = stats.TimeSpan;
                }
                stats.Points = Mathf.Max(stats.Points, res.pointsCollected);
                stats.Stars = Mathf.Max(stats.Stars, _starRecord);
                stats.TimeSpan = timeSpan;

                userData.GameStats.SaveData();
            }
            _showRateDialog = false;
            userData.SaveUserDate();
        }

        private void UpdateTimeLabel(LevelResult res, Record trackStatRecord)
        {
            Time.BestMax.gameObject.SetActive(trackStatRecord != null);
            Time.Current.text = res.timeSpan.ToString("mm\\:ss\\:ff");
            if (trackStatRecord != null)
                Time.BestMax.text = trackStatRecord.TimeSpan.ToString("mm\\:ss\\:ff");
            bool flag = true;
            if (trackStatRecord != null)
            {
                flag = res.timeSpan <= trackStatRecord.TimeSpan;
            }
            if (flag)
            {
                Time.BestMax.color = Color.white;
                Time.Current.color = Color;
            }
            else
            {
                Time.Current.color = Color.white;
                Time.BestMax.color = Color;
            }
        }

        private void UpdateScoreLabel(LevelResult res, Record trackStatRecord)
        {
            Score.BestMax.gameObject.SetActive(trackStatRecord != null);
            Score.Current.text = string.Format("{0}", res.pointsCollected);
            if (trackStatRecord != null)
                Score.BestMax.text = string.Format("{0}", trackStatRecord.Points);
            bool flag = true;
            if (trackStatRecord != null)
            {
                flag = trackStatRecord.Points <= res.pointsCollected;
            }
            if (flag)
            {
                Score.BestMax.color = Color.white;
                Score.Current.color = Color;
            }
            else
            {
                Score.Current.color = Color.white;
                Score.BestMax.color = Color;
            }
        }

        public override void PostShow(CommonPayloadData payload)
        {
            _starsSequence = StarsView.SetWithAnimation(_starRecord, _coinsByStars);
            _starsSequence.AppendInterval(1);
            var s = DOTween.Sequence();
            s.AppendInterval(1);
            //s.OnKill(() =>
            //{
            //    UiRoot.Instance.OnWalletAnimation();
            //});
            s.Play();

            actions.UI.Enter.performed += PlayNext;
            actions.UI.YButton.performed += Reload;
        }

        private void ShowRateDialog()
        {
        }

        private void Reload(InputAction.CallbackContext ctx = default)
        {
            Game.Instance.ScreenManager.Show<PreGameplayScreen>(true, false);
            SoundsManager.Instance.PlaySounds(SoundType.ui_click);
        }

        public void GoBack()
        {
            Game.Instance.ScreenManager.Show<SelectStoryScreen>(true, false);
            SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            SoundsManager.Instance.PlayBackground(MusicType.menu);
        }

        private void PlayNext(InputAction.CallbackContext ctx = default)
        {
            if (UserDataManager.RuntimeInfo.CurrentStory < UserDataManager.Instance.CurrentBalanceLocation.CurrentStoryModeStoryInfos.Count - 1)
            {
                var nextStory = UserDataManager.Instance.CurrentBalanceLocation.CurrentStoryInfos[UserDataManager.RuntimeInfo.CurrentStory + 1];
                if (SelectLocationScreenView.IsItemLocked(UserDataManager.Instance, nextStory.UnlockInfo, nextStory.Name))
                {
                    Game.Instance.ScreenManager.Show<SelectStoryScreen>(true, true);
                    SoundsManager.Instance.PlayBackground(MusicType.menu);
                    return;
                }
                UserDataManager.RuntimeInfo.CurrentStory++;
                Game.Instance.ScreenManager.Show<PreGameplayScreen>(true, false);
            }
            else
            {
                Game.Instance.ScreenManager.Show<SelectLocationScreen>(true, false);
            }
            SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            SoundsManager.Instance.PlayBackground(MusicType.menu);
        }

        public override void SetSelectedGO()
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        public override void Back()
        {
            BackToLobbyButton.onClick.Invoke();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            _starsSequence.Kill();
        }
    }
}
