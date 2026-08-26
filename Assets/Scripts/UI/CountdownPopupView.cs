using System.Collections;
using UnityEngine;

namespace UI
{
	public class CountdownPopupView : ScreenViewWithCommonPayload<CountdownPopup>
	{
		public CountdownController CountdownController;

		public override void Init(CountdownPopup popup)
		{
		}

		public override void PreShow(CommonPayloadData payload)
		{
			CoroutineRunner.Instance.Run(Countdown());
        }

        private IEnumerator Countdown()
		{
			CountdownController.Text.text = "3";
			yield return new WaitForSeconds(0.5f);

            CountdownController.Text.text = "2";
            yield return new WaitForSeconds(0.5f);

            CountdownController.Text.text = "1";
            yield return new WaitForSeconds(0.5f);

			Game.Instance.ScreenManager.ClosePopup();
			Game.Instance.ScreenManager.Show<GameplayScreen>(false, false);
			LevelMainController.current.pauseRender = false;
            SoundsManager.Instance.PauseAll(false);

            yield break;
        }
	}
}
