using System;
using UnityEngine;

namespace Nekki.Vector.Core.Camera
{
    public class ZoomContainer
    {

        private float _Factor;

        private float _ZoomValue;

        private float _Scale;

        private bool _IsZoom;

        private int _Frame;

        private int _CameraFrame;

        private float _Time;

        public float Time
        {
            get { return _Time; }
            set { _Time = value; }
        }

        private GameObject _Layer;

        public float FrameScale
        {
            get
            {
                float num = 2 * _Frame / _Time;
                num *= num;
                float num2 = (_ZoomValue - _Scale) / 2f;
                if (_Frame < _Time / 2f)
                {
                    return _Scale + num2 * num;
                }
                return _Scale + num2 * (8 * _Frame / _Time - 2f - num);
            }
        }

        public void Add(GameObject p_object, float p_factor)
        {
            _Factor = p_factor;
            _Layer = p_object;
            _ZoomValue = 1;
            _Scale = 4;
            _Time = 30;
        }

        public void Move(Vector3d p_point)
        {
            if (p_point == null)
            {
                return;
            }
            Play();
            var vector = _Layer.transform.position;
            vector.x = -(float)(p_point.X * _Factor * _Layer.transform.localScale.x);
            vector.y = -(float)(p_point.Y * _Factor * _Layer.transform.localScale.x);
            _Layer.transform.position = vector;
        }

        public void Zooming(float p_value, bool p_isStart, int frames = 30)
        {
            _Time = frames;
            _Frame = 0;
            _Scale = _ZoomValue;
            _ZoomValue = Utilites.Math.Round(p_value / (p_value + _Factor * (1 - p_value)), 10);
            if (!p_isStart)
            {
                _IsZoom = true;
                return;
            }
            _Scale = _ZoomValue;
            _Layer.transform.localScale = new Vector3(FrameScale, FrameScale, FrameScale);
        }

        public void Play()
        {
            if (!_IsZoom)
            {
                return;
            }
            _Frame++;
            _Layer.transform.localScale = new Vector3(FrameScale, FrameScale, FrameScale);
            if (_Frame < _Time)
            {
                return;
            }
            _IsZoom = false;
            _Time = 30;
        }
    }
}
