using System;
using System.Collections.Generic;
using Nekki.Vector.Core.Location;
using Nekki.Vector.Core.Node;

namespace Nekki.Vector.Core.Camera
{
	public class LocationCamera
	{
		private List<ZoomContainer> _Containers = new List<ZoomContainer>();

		private float _Zoom = 1;

		private ModelNode _Position = new ModelNode(new Vector3d());

		private bool _IsDrag;

		private CameraNode _Node;

		private float _TempZoom;

		private bool _IsRender;

		private bool _IgnoreZoom;

		private int _EffectFrames;

		private int _ActionLaunched;

		private List<object> _Layers = new List<object>();

		private static LocationCamera _Current;

		private Random _Random = new Random();

		public static float MinZoom = 0.1f;

		public static float MaxZoom = 1.3f;

		public static float CurrentZoom = 0.5f;

		public static float FluencyCurrent = 2;

		public float Zoom => _Zoom;

		public CameraNode Node
		{
			get => _Node;
			set => _Node = value;
		}

		public bool IsRender => _IsRender;

		public bool IgnoreZoom
		{
			get => _IgnoreZoom;
			set => _IgnoreZoom = value;
		}

		public static LocationCamera Current => _Current;

		public UnityEngine.Camera UnityCamera => UnityEngine.Camera.main;

		public float X => (float)_Position.Start.X;

		public float Y => (float)_Position.Start.Y;

		public void Init()
		{
			_Position.Attenuation = 0;
            _Current = this;
            _IsRender = true;
		}

		public void Reset()
		{
			_Position.PositionStart(new Vector3d(0, 0, 0));
			_Position.PositionEnd(new Vector3d(0, 0, 0));
			_IsRender = true;
			_Node = null;
			Update();
		}

		public void StartPosition(Vector3d p_position)
		{
			_Position = new ModelNode(p_position);
			_Node = new CameraNode(_Position);
		}

		public void UpdatePosition()
		{
			if (!_IsRender)
			{
				return;
			}
            _Position.TimeStep(0);
            var nodeStart = _Node.Start;
            var nodeEnd = _Node.End;
            var posStart = _Position.Start;
            var posEnd = _Position.End;

            nodeStart.Z = 0;
            nodeEnd.Z = 0;
            posStart.Z = 0;
            posEnd.Z = 0;

            Vector3d v1 = posEnd + nodeStart;
            nodeEnd = v1 - nodeEnd;
            nodeEnd = nodeEnd - posStart;

            nodeStart = nodeStart - posStart;
            nodeStart = nodeStart * 0.15;
            nodeEnd = nodeEnd + nodeStart;

            if (FluencyCurrent < nodeEnd.Length)
            {
                nodeEnd.Normalize();
                nodeEnd.Multiply(FluencyCurrent);
            }
            posStart.Add(nodeEnd);
            nodeEnd = posStart - posEnd;

            if (nodeEnd.Length > 50)
            {
                nodeEnd.Multiply(50 / nodeEnd.Length);
                posStart = posEnd + nodeEnd;
            }
            _Position.PositionStart(posStart);
            Update();

        }

		public void Update()
		{
			foreach (var container in _Containers)
			{
				container.Move(_Position.Start);
			}
        }

        public void Stop()
		{
            Vector3d p_position = _Node.Start + (_Node.Start - _Node.End) * 20f;
            _Node = new CameraNode(new ModelNode(p_position));
        }

		public void Stop(float? XPos = null, float? YPos = null)
        {
			float X = XPos ?? (float)(_Node.Start.X + (_Node.Start.X - _Node.End.X) * 20f);
            float Y = YPos ?? (float)(_Node.Start.Y + (_Node.Start.Y - _Node.End.Y) * 20f);
            Vector3d p_position = new Vector3d(X, Y);
            _Node = new CameraNode(new ModelNode(p_position));
        }

        public void Layers(BaseSets p_sets)
		{
			foreach (var container in p_sets.Containers.Values)
			{
				var zoomContainer = new ZoomContainer();
				zoomContainer.Add(container.Object, container.Factor);
				_Containers.Add(zoomContainer);
				_Layers.Add(container);
			}
		}

		public void Zooming(float p_value = 1f, bool p_isStart = false, int frames = 30)
		{
            if (_Zoom != p_value && _IgnoreZoom == false)
            {
                if (p_value < MinZoom)
                {
                    p_value = MinZoom;
                }
                if (p_value > MaxZoom)
                {
                    p_value = MaxZoom;
                }
                _Zoom = p_value;
                foreach (var container in _Containers)
                {
                    container.Zooming(p_value, p_isStart, frames);
                }
            }
        }

		public void ZoomIncrease(float p_value)
		{
			Zooming(_Zoom + p_value);
		}

		public void ZoomReduce(float p_value)
		{
            Zooming(_Zoom - p_value);
        }
	}
}
