using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;

namespace Nekki.Vector.Core.Scripts
{
    public class AnimationSprite : MonoBehaviour
    {
        private List<Sprite> _frames;

        public SpriteRenderer _spriteRenderer;

        private int _totalFrames;

        private int _currentFrame;

        private float _times;

        [SerializeField]
        private float _FPS = 10;

        private int _iterations = -1;

        private int _iterationsCounter = -1;

        private bool _isWork;

        public Action OnIterationsEnd;

        public float FPS
        {
            set => _FPS = value;
        }

        public int Iterations
        {
            set => _iterations = value;
        }

        public bool IsWork
        {
            get => _isWork;
            set
            {
                _currentFrame = 0;
                _times = 0;
                _isWork = value;
                _iterationsCounter = 0;
            }
        }

        public int TotalFrames => _frames == null ? 0 : _frames.Count;

        public virtual void Init(string p_name, SpriteRenderer p_spriteRender, float pivotX = 0, float pivotY = 1)
        {
            Init(GetFramesSequence(p_name, pivotX, pivotY), p_spriteRender);
        }

        public virtual void Init(SpriteAtlas atlas, SpriteRenderer p_spriteRender)
        {
            Init(GetFramesSequence(atlas), p_spriteRender);
        }

        public void Init(List<Sprite> sprites, SpriteRenderer p_spriteRender)
        {
            if (sprites == null)
            {
                DebugUtils.Dialog("Error create animation", false, true);

                return;
            }
            _frames = sprites;
            _totalFrames = _frames.Count;

            if (_totalFrames == 0)
            {
                DebugUtils.Dialog("Error create animation", false);
                return;
            }
            if (p_spriteRender == null)
            {
                p_spriteRender = gameObject.GetComponent<SpriteRenderer>();
            }
            _spriteRenderer = p_spriteRender;
            _spriteRenderer.sprite = _frames[0];
            _currentFrame = 0;
            _iterationsCounter = 0;
        }

        public virtual void SetSpriteFrame(int p_index)
        {
            if (_totalFrames <= p_index)
            {
                return;
            }
            _spriteRenderer.sprite = _frames[p_index];
        }

        public void SetSpriteAnimation()
        {
            SetSpriteFrame(_currentFrame);
            _currentFrame++;
            if (_totalFrames <= _currentFrame)
            {
                _currentFrame = 0;
                _iterationsCounter++;
                if (_iterations != -1 && _iterations <= _iterationsCounter)
                {
                    _isWork = false;
                    OnIterationsEnd?.Invoke();
                }
            }
        }

        private void FixedUpdate()
        {
            if (_isWork)
            {
                _times += Time.deltaTime;
                if (1 / _FPS <= _times)
                {
                    SetSpriteAnimation();
                    _times -= 1 / _FPS;
                }
            }
        }

        public void Reset()
        {
            _currentFrame = 0;
            _times = 0;
            _iterationsCounter = 0;
        }

        public static List<Sprite> GetFramesSequence(SpriteAtlas atlas)
        {
            if (atlas != null)
            {
                Sprite[] sprites = new Sprite[atlas.spriteCount];
                atlas.GetSprites(sprites);
                List<Sprite> list = new List<Sprite>(sprites);
                Comparison<Sprite> comparison = (sprite1, sprite2) => string.Compare(sprite1.name, sprite2.name, StringComparison.Ordinal);
                list.Sort(comparison);
                return list;
            }
            return null;
        }

        public static List<Sprite> GetFramesSequence(string p_name, float pivotX = 0, float pivotY = 1)
        {
            string atlasPath = p_name;

            if (p_name.StartsWith(Application.streamingAssetsPath) && !ResourceManager.FileExists(p_name, out atlasPath, ".plist", ".json"))
            {
                DebugUtils.Dialog("Animation not found: " + Path.GetFileNameWithoutExtension(p_name), false, true);
            }


            var sprites = AtlasDecoder.Decode(atlasPath, p_name, pivotX, pivotY);
            if (sprites != null)
            {
                return sprites;
            }


            return null;
        }
    }
}
