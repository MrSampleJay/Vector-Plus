using Nekki.Vector.Core.Location.Animation;
using Nekki.Vector.Core.Node;
using Nekki.Vector.Core.Controllers;
using UnityEngine;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
	public class ParticleRunner : AnimationRendering
	{
		private int _liveCount;

		private int _liveTime;

		private int _impuls;

		private bool _isRender;

		private ModelNode _node;

		public ParticleRunner(float p_x, float p_y, float p_width, float p_height, string p_name)
			: base(p_name, p_x, p_y, p_width, p_height, true, 1)
		{
			_isRender = false;
			_TypeClass = RunnerType.Particle;
			_impuls = 10;
			_IsPlay = false;
			_liveTime = (int)(Random.Range(0.5f, 1.5f) * 60);
        }

		protected override void SerializeData()
		{
			_UnityObject.AddComponent<Xml2PrefabParticleContainer>().Init(_Name, _DefautPosition.X, _DefautPosition.Y, _Width, _Height, TransformationDataRaw, Choice);
		}

		public override void Init(float pivotX = 0, float pivotY = 1)
		{
			if (_Name == "p_glass1_mini")
			{
				Name = "glass_1";
            }
			base.Init();
			_CachedTransform.localScale *= Random.Range(0.5f, 1f);
			var range = Random.Range(0, 360);
			_CachedTransform.eulerAngles = new Vector3(range, range, 0);
		}

		public void PlayAnimation(ModelNode p_node)
		{
			RunnerRender.AddRunner(this);
			_liveCount = 0;
			_IsPlay = true;
			_isRender = true;
			_node = new ModelNode(new Vector3d());
			_node.Attenuation = 0;
			_node.PositionStart((Position.X + _Width * 0.5), (Position.Y + _Height * 0.5), 0);
			_node.EndAssignStart();
			MoveParticle();
			SetImpuls(p_node);
			IsEnabled = true;
			PlayFrom(Random.Range(0, _TotalFrames));
		}

		private void SetImpuls(ModelNode p_node)
		{
			var nodeStart = new Vector3f(_node.Start);
			var start = new Vector3f(p_node.Start);
			var end = new Vector3f(p_node.End);

			var vector = end - start;
			vector.Normalize();
			vector.Multiply(100);

			var vector2 = end + vector;
			vector2 = nodeStart - vector2;
			vector2.Normalize();
			vector2.Multiply(10 / Mathf.Sqrt(LevelMainController.current.slowModeFrames));
			vector2 += nodeStart;

			var vector3 = new Vector3f(Random.Range(-5, 5), Random.Range(-5, 5));
			vector2 += vector3;
			_node.PositionStart(vector2);
		}

		private void MoveParticle()
		{
			_CachedTransform.localPosition = new Vector3((float)_node.Start.X, (float)_node.Start.Y, -11);
		}

		public override bool Render()
		{
			if (!_isRender)
			{
				return true;
			}
			base.Render();
			_node.TimeStep(ControllerPhysics.Gravity);
			MoveParticle();
			IsCollision();
			return false;
		}

		private void IsCollision()
		{
			_liveCount++;
			if (_liveCount < _liveTime)
			{
				return;
			}
			Stop(true);
			_isRender = false;
			IsEnabled = false;
		}

		public override void Reset()
		{
			base.Reset();
			_node = null;
			Stop();
			IsEnabled = false;
			_IsPlay = false;
			_isRender = false;
		}
	}
}
