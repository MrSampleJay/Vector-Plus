using System.Collections.Generic;
using Nekki.Vector.Core.Controllers;
using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Node;
using Nekki.Vector.Core.Scripts;
using UnityEngine;
using Collision = Nekki.Vector.Core.Result.Collision;

namespace Nekki.Vector.Core.Models
{
    public class Model
    {
        private bool _IsEnabled;

        private string _Name;

        protected ModelObject _ModelObject;

        protected ControllerPhysics _ControllerPhysics;

        protected ControllerCollisions _ControllerCollisions;

        protected ControllerStrike _controllerStrike;

        protected double _DeltaBox = 300;

        protected ModelType _Type;

        public const double BoundingBoxSize = 300.0;

        public virtual GameObject Layer
        {
            get => _ModelObject.Layer;
            set => _ModelObject.Layer = value;
        }

        public virtual bool IsEnabled
        {
            get => _IsEnabled;
            set => _IsEnabled = value;
        }

        public bool IsVisible
        {
            get => _ModelObject.IsVisible;
            set => _ModelObject.IsVisible = value;
        }

        public string Name
        {
            get => _Name;
            set
            {
                _Name = value;
                _ModelObject.Name = value;
            }
        }

        public ModelObject ModelObject => _ModelObject;

        public ControllerPhysics ControllerPhysics => _ControllerPhysics;

        public ControllerCollisions ControllerCollisions => _ControllerCollisions;

        public ControllerStrike ControllerStrike => _controllerStrike;

        public double DeltaBox => _DeltaBox;

        public ModelType Type
        {
            get => _Type;
            set => _Type = value;
        }

        public Color Color
        {
            get => _ModelObject.Color;
            set => _ModelObject.Color = value;
        }

        public virtual Rectangle Rectangle => _ModelObject.Rectangle == null ? new Rectangle() : _ModelObject.Rectangle;

        public bool IsPhysics => _ControllerPhysics.IsPhysics;

        public bool DebugMode
        {
            set => _ModelObject.DebugMode = value;
        }

        public Model(List<string> skins, ModelType type)
        {
            _Type = type;
            _ModelObject = new ModelObject(skins);
            _ModelObject.IsAuxiliary = false;
        }

        public virtual void Init()
        {
            _ControllerPhysics = new ControllerPhysics(_ModelObject);
            _ControllerCollisions = new ControllerCollisions(this);
            _controllerStrike = new ControllerStrike(this);
            _ControllerPhysics.Stop();
        }

        public virtual void StartPhysics()
        {
            _ControllerPhysics.Start();
        }

        public void StopPhysics()
        {
            _ControllerPhysics.Stop();
        }

        public virtual void Strike(ModelLine edge, Vector3d point, Vector3d impulse)
        {
            _controllerStrike.Striking(edge, point, impulse);
        }

        public virtual void OnCollisionPlatform(ModelEvent<Collision> Event)
        {
        }

        public virtual void OnCollisionModel(Collision collision)
        {
        }

        public virtual void Render(List<QuadRunner> platforms = null)
        {
        }

        public virtual void Reset()
        {
            _ModelObject.Reset();
        }

        public void Position(Vector3d vector, string Name = "NPivot")
        {
            _ModelObject.Position(vector, Name);
        }

        public Vector3d Position(string name = "NPivot", bool isCurrent = true)
        {
            return _ModelObject != null ? _ModelObject.Position(name, isCurrent) : null;
        }

        public ModelNode GetNode(string nodeName = "NPivot")
        {
            return _ModelObject.GetNode(nodeName);
        }

        public Vector2 GetPositionForIcon()
        {
            return _ModelObject.GetPositionForIcon();
        }
    }
}
