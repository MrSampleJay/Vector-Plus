using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
	public class PlatformRunner : QuadRunner
	{
		private new string _Name;

		private float _X;

		private float _Y;

		private float _W;

		private float _H;

		private bool _S;

		public PlatformRunner(string Name, float X, float Y, float Width, float Height, bool p_Stikly, string p_material = "")
			: base(X, Y, Width, Height, p_Stikly, 0, Name, p_material)
		{
			_Name = Name;
			_X = X;
			_Y = Y;
			_W = Width;
			_H = Height;
			_S = p_Stikly;
		}

		protected override void GenerateObject()
		{
			base.GenerateObject();
		}

        protected override void SerializeData()
        {
			if (_UnityObject == null)
			{
				CreateObject();
			}
            _UnityObject.AddComponent<Xml2PrefabPlatformContainer>().Init(_Name, _X, _Y, _W, _H, _S, TransformationDataRaw, Choice);
            _CachedTransform = _UnityObject.transform;
        }

        public override string ToString()
		{
			return null;
		}
	}
}
