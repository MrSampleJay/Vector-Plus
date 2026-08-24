using Nekki.Vector.Core.Camera;
using Nekki.Vector.Core.Models;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Nekki.Vector.Core.Location
{
    public class ArrestAreaRunner : AreaRunner
    {
        private bool _IsArrest;

        private ModelHuman _ActiveModel;

        private List<ModelHuman> _ArrestModels = new List<ModelHuman>();

        private float _Distance;

        public static ArrestAreaRunner ActiveArrest;

        public bool IsArrest => _IsArrest;

        public ArrestAreaRunner(float x, float y, float width, float height, string typeName, string name, float distance, int direction = 0)
            : base(AreaType.Catch, x, y, width, height, typeName, name, direction)
        {
            _IsEnabled = true;
            _Distance = distance;
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
            _UnityObject.AddComponent<Xml2PrefabArrestAreaContainer>().Init(TransformationDataRaw, _TypeName, _Name, _X, _Y, _W, _H, Choice);
            _CachedTransform = _UnityObject.transform;
        }

        public override void Activate(ModelHuman modelHuman)
        {
            if (modelHuman.Arrests.Count == 0)
            {
                IsEnabled = false;
            }
            else if (IsEnabled)
            {
                _IsArrest = false;
                _ActiveModel = modelHuman;
                ActiveArrest = this;

                foreach (var model in LevelMainController.current.GetModelsByNames(modelHuman.Arrests))
                {
                    var distanceX = (modelHuman.ModelObject.CenterOfMassNode.Start -
                                     model.ModelObject.CenterOfMassNode.Start).X;

                    bool valid = (distanceX < 0 && modelHuman.Sign < 0) || (distanceX >= 0 && modelHuman.Sign >= 1);

                    if (valid && _Distance < Mathf.Abs((float)distanceX))
                    {
                        _IsArrest = true;
                        _ArrestModels.Add(model);
                    }
                }

            }
        }

        public override void Deactivate(ModelHuman modelHuman)
        {
            _IsArrest = false;
            ActiveArrest = null;
        }

        public override void Reset()
        {
            base.Reset();
            _ArrestModels.Clear();
            IsEnabled = true;
        }

        public void Arresting()
        {
            foreach (var model in _ArrestModels)
            {
                if (model.IsSelf)
                {
                    LocationCamera.Current.Node = _ActiveModel.ModelObject.CameraNode;
                    LevelMainController.current.Arrest(model);
                }
            }
        }
    }
}
