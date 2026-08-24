using UnityEngine;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location.Animation
{
	public class AnimationVectorRunner : AnimationRunner
	{
		private Vector3f _Direction = new Vector3f();

		private Vector3f _Acceleration = new Vector3f();

		private Vector3f _DirectionResult = new Vector3f();

		private Vector3f _AccelerationResult = new Vector3f();

		private int _Life;

		private int _Count;

		public AnimationVectorRunner(float x, float y, float width, float height, string name, int type, float scaleX, float scaleY, Vector2? direction, Vector2? acceleration, int life, bool replay = false, bool iskill = false, bool isSmooth = false, int speed = 1, int thin = 1)
			: base(x, y, width, height, name, type, scaleX, scaleY, true, 2)
		{
			if (direction != null)
			{
				_Direction = new Vector3f(direction.Value);
			}
			if (acceleration != null)
			{
				_Acceleration = new Vector3f(acceleration.Value);
			}
			_AccelerationResult = new Vector3f(_Acceleration);
			_Life = life;
			_IsPlay = false;
		}

		protected override void SerializeData()
		{
			_UnityObject.AddComponent<Xml2PrefabAnimationVectorContainer>().Init(_Name, _X, _Y, _Width, _Height, _Type, _ScaleX, _ScaleY, TransformationDataRaw, _Direction.X, _Direction.Y, _Acceleration.X, _Acceleration.Y, _Life, Choice); ;
		}

		public override void Reset()
		{
			_DirectionResult.Set(_DefautPosition);
			_AccelerationResult.Set(_Acceleration);
			_Count = 0;
			_UnityObject.transform.localPosition = new Vector3(_DefautPosition.X, _DefautPosition.Y, -1);
			IsEnabled = false;
			base.Reset();
		}

		public override void Move(Point Point)
		{
			base.Move(Point);
			_DirectionResult.X = Position.X;
			_DirectionResult.Y = Position.Y;
		}

		public override bool Render()
		{
			if (!_IsPlay)
			{
				return true;
			}
			_AccelerationResult.Add(_Acceleration);
			_DirectionResult.Add(_Direction);
			_DirectionResult.Add(_AccelerationResult);
			_UnityObject.transform.localPosition = new Vector3(_DirectionResult.X, _DirectionResult.Y, -11);
			base.Render();
			if (LevelMainController.current.slowModeFrames * _Life < _Count)
			{
				Stop(true);
				return true;
			}
			_Count++;
			return false;
		}
	}
}
