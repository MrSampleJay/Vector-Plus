using Nekki.Vector.Core.Controllers;
using Nekki.Vector.Core.Models;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
	public class TutorialAreaRunner : AreaRunner
	{
		private uint _repeatCounter;

		private const uint RepeatsToNotShowTutorial = 2u;

		private string _key;

		public KeyVariables Key
		{
			get;
			private set;
		}

		public string Description
		{
			get;
			private set;
		}

		public static TutorialAreaRunner current
		{
			get;
			private set;
		}

		private bool ShowTutorial
		{
			get
			{
				if (_repeatCounter < 2)
				{
					if (Game.Instance.Snail)
					{
						return true;
					}
					return !LevelMainController.current.HasCurrentLocationEverBeenCompleted();
				}
				return false;
			}
		}

		public TutorialAreaRunner(float x, float y, float width, float height, string typeName, string name, string key, string description, int direction = 0)
			: base(AreaType.Help, x, y, width, height, typeName, name, direction)
		{
			_key = key;
			Key = new KeyVariables(key);
			Description = description;
		}

		public override bool OnKeyClick(KeyVariables keyVariables)
		{
			if (Key.IsEqual(keyVariables))
			{
				LevelMainController.current.TutorialUnLockGame();
				current = null;
				_repeatCounter++;
				return true;
			}
			return false;
		}

		public override void InitRunner(Point point, bool serialize = false)
		{
			base.InitRunner(point, serialize);
			UpdateUnityObjectPosition(Position);
		}

		protected override void SerializeData()
		{
            if (_UnityObject == null)
            {
                CreateObject();
            }
            _UnityObject.AddComponent<Xml2PrefabTutorialAreaContainer>().Init(TransformationDataRaw, _TypeName, _Name, _X, _Y, _W, _H, _key, Description, Choice);
            _CachedTransform = _UnityObject.transform;
        }

        public override void Activate(ModelHuman model)
		{
			if (ShowTutorial)
			{
				if (LevelMainController.current.IsDeath || model.IsBot || !model.ControllerAnimations.IsActionInterval)
				{
					return;
				}
				current = this;
				LevelMainController.current.TutorialAreaActivate(this);
			}
		}

		public override void Deactivate(ModelHuman model)
		{
			base.Deactivate(model);
			if (current == null || !ShowTutorial)
			{
				return;
			}
			LevelMainController.current.TutorialLockGame();
		}
	}
}
