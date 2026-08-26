using UnityEngine;

namespace UI
{
    [View("LobbyScreenView")]
	public class LobbyScreen : Screen
	{
		public Button PlayButton = new Button();

		public Button StoreButton = new Button();

		public Button StatsButton = new Button();

		public Button FollowUsFacebookButton = new Button();

		public Button FollowUsTwitterButton = new Button();

		public Button FollowUsTikTokButton = new Button();

		public Button GetFreeButton = new Button();

		public Button MoreGamesButton = new Button();

		public Button CreditsButton = new Button();

		public Button OptionsButton = new Button();

		public Button BuyCoinsButton = new Button();

		public LobbyScreen(ScreenManager screenManager)
			: base(screenManager)
		{
			PlayButton.PressedAction += () =>
			{
				ScreenManager.Show<ChooseModeScreen>(true, false);
				SoundsManager.Instance.PlaySounds(SoundType.ui_click);
			};
            StoreButton.PressedAction += () =>
            {
                ScreenManager.Show<StoreScreen>(true, false);
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            };
            StatsButton.PressedAction += () =>
			{
                ScreenManager.Show<StatsScreen>(true, false);
                SoundsManager.Instance.PlaySounds(SoundType.ui_click);
            };
            FollowUsFacebookButton.PressedAction += () =>
            {
				Application.OpenURL("https://m.facebook.com/VectorTheGame");
            };
            FollowUsTwitterButton.PressedAction += () =>
            {
				Application.OpenURL("https://mobile.twitter.com/VectorTheGame");
            };
            FollowUsTikTokButton.PressedAction += () =>
            {
				Application.OpenURL("https://www.tiktok.com/@vector_the_game");
            };
            GetFreeButton.PressedAction += () =>
            {
                ScreenManager.CommingSoon();
            };
            MoreGamesButton.PressedAction += () =>
            {
                ScreenManager.CommingSoon();

            };
            CreditsButton.PressedAction += () =>
            {
                ScreenManager.Show<CreditsScreen>(true, false);

            };
            OptionsButton.PressedAction += () =>
            {
                ScreenManager.Show<OptionsScreen>(true, false);
                SoundsManager.Instance.PlaySoundsSequential(SoundType.ui_click, SoundType.ui_window_options);
            };

        }
	}
}
