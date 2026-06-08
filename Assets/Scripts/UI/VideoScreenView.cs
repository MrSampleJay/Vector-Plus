using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

namespace UI
{
	public class VideoScreenView : ScreenView<VideoScreen, VideoScreenPayloadData>
	{
		public UnityEngine.UI.Button CloseButton;

		public VideoPlayer VideoPlayer;

		public RenderTexture RenderTexture;

		private VideoScreenPayloadData _lastPayload;

		public override void Init(VideoScreen screen)
		{
			CloseButton.onClick.AddListener(() => StopVideo(VideoPlayer));
		}

		public override void PreShow(VideoScreenPayloadData payload)
		{
			SoundsManager.Instance.AudioSourceBackground.Stop();
			VideoPlayer.gameObject.SetActive(false);
#if PLATFORM_ANDROID
			VideoPlayer.source = VideoSource.VideoClip;
			VideoPlayer.clip = Resources.Load<VideoClip>(payload.Video);
#else
			VideoPlayer.source = VideoSource.Url;
			VideoPlayer.url = "file:///" + Application.streamingAssetsPath + "/" + payload.Video;
#endif
            _lastPayload = payload;
            VideoPlayer.gameObject.SetActive(true);
            VideoPlayer.Prepare();
			VideoPlayer.prepareCompleted += vp =>
			{
                vp.Play();
                _lastPayload.IsPlaying = true;
            };
            VideoPlayer.loopPointReached += OnFinished;
		}

		public override void PostShow(VideoScreenPayloadData payload)
		{
		}
		
		public override void SetSelectedGO()
		{
			EventSystem.current.SetSelectedGameObject(CloseButton.gameObject);
		}

		public override void Back()
		{
		}

		private void OnFinished(VideoPlayer source)
		{
			VideoPlayer.loopPointReached -= OnFinished;
			StopVideo(source);
        }

        private void StopVideo(VideoPlayer source)
		{
			source.Stop();
			_lastPayload.Action?.Invoke();
            _lastPayload.IsPlaying = false;
            VideoPlayer.gameObject.SetActive(false);
			RenderTexture.Release();
            SoundsManager.Instance.AudioSourceBackground.Play();


        }
    }
}
