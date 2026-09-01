using Nekki.Vector.Core.Models;
using UnityEngine;
using Xml2Prefab;

namespace Nekki.Vector.Core.Location
{
	public class CoinRunner : ItemScoreRunner
	{
		private bool _enabled;

		private int _groupID;

		private int _defaultScore;

		public bool enabled => _enabled;

		public Color color
		{
			set => _content.color = value;
		}

		public int GroupID => _groupID;

		public CoinRunner(int type, string prefabName, int groupId, int score, float x, float y)
			: base(type, prefabName, score, x, y, "credits", "credits_off")
		{
			_groupID = groupId;
			_defaultScore = score;
		}

		protected override void SerializeData()
		{
			_UnityObject.AddComponent<Xml2PrefabCoinContainer>().Init(_type, _x, _y, _prefabName, _groupID, _score, TransformationDataRaw, Choice);
		}

		protected override void GenerateObject()
		{
			_UnityObject = Object.Instantiate(Resources.Load<GameObject>("LevelContent/Prefabs/" + _prefabName));
			_CachedTransform = _UnityObject.transform;
			_CachedTransform.localPosition = new Vector3(_DefautPosition.X, _DefautPosition.Y, 0);
            if (_Layer != null)
            {
                _UnityObject.transform.parent = _Layer.transform;
            }
        }

		public override void Collision(ModelHuman modelHuman)
		{
			if (_enabled)
			{
				base.Collision(modelHuman);
			}
		}

		public override void Init()
		{
		}

		public override void InitRunner(Point point, bool serialize = false)
		{
			base.InitRunner(point, serialize);
			SetEnable(false);
		}

		public override void AddBonus(ModelHuman modelHuman)
		{
			SoundsManager.Instance.PlaySounds(SoundType.bonus_pickup);
			modelHuman.AddCoin(this);
		}

		public void SetEnable(bool value)
		{
			_enabled = value;
			_content.gameObject.SetActive(value);
			_score = _defaultScore;
		}

		public override void Reset()
		{
			base.Reset();
			SetEnable(false);
			_content.color = Color.white;
		}
	}
}
