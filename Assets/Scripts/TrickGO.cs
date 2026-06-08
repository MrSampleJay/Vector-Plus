using System.IO;
using Nekki.Vector.Core.Scripts;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;
using Core._Common;
using UI;

public class TrickGO : MonoBehaviour
{
    [SerializeField]
    private AnimationSprite _animation;

    [SerializeField]
    private SpriteRenderer _animationSprite;

    [SerializeField]
    private SpriteRenderer _trickIcon;

    [SerializeField]
    private string _idleAnimation;

    [SerializeField]
    private string _activateAnimation;

    private List<Sprite> idle;

    private List<Sprite> activate;

    public void Init(string itemName, bool isActive, float w, float h)
    {
        idle = AnimationSprite.GetFramesSequence(VectorPaths.AnimatedTextures + "/" + _idleAnimation, 0.5f ,0.5f);
        activate = AnimationSprite.GetFramesSequence(VectorPaths.AnimatedTextures + "/" + _activateAnimation, 0.5f ,0.5f);

        if (_animation != null)
        {
            _animation.gameObject.SetActive(true);
            _trickIcon.gameObject.SetActive(true);
            _animation.Init(idle, _animationSprite);
            if (isActive)
            {
                InitActiveState(itemName);
            }
            else
            {
                InitDisableState();
            }
            //if (Xml2PrefabRoot.UseOnlyXML)
            {
                UpdatePosition(w, h);
                // UpdateScale(w, h);
            }

        }
    }

    public void UpdatePosition(float w, float h)
    {
        var vector = new Vector3(w * 0.5f, h * 0.5f);
        _animation.transform.localPosition = vector;

        var vector2 = new Vector3(w * 0.5f, h * 0.5f + 23.28f);
        _trickIcon.transform.localPosition = vector2;
    }

    public void UpdateScale(float w, float h)
    {
        var animScale = _animation.transform.localScale;
        animScale.x = 1 / w;
        animScale.y = 1 / h;
        _animation.transform.localScale = animScale;

        var iconScale = _trickIcon.transform.localScale;
        iconScale.x = 0.9f / w;
        iconScale.y = 0.9f / h;
        _trickIcon.transform.localScale = iconScale;

        _animationSprite.flipY = true;
        _trickIcon.flipY = true;
    }

    private void InitDisableState()
    {
        CoroutineRunner.Instance.Run(ResourcesLoader.LoadSpriteAsync(VectorPaths.Icons + "/tricks/lock", sprite =>
        {
            _trickIcon.sprite = sprite;

        }, new Vector2(0.5f, 0.5f), 1));

    }

    private void InitActiveState(string itemName)
    {
        _animation.IsWork = true;
        _animation.Iterations = -1;

        CoroutineRunner.Instance.Run(ResourcesLoader.LoadSpriteAsync(VectorPaths.Icons + "/tricks/TRACK_" + itemName, sprite =>
        {
            _trickIcon.sprite = sprite;

        }, new Vector2(0.5f, 0.5f), 1));
    }

    public void RunActivate()
    {
        _animation.Init(activate, _animationSprite);
        _animation.Iterations = 1;
        _animation.OnIterationsEnd = () =>
        {
            _animation.gameObject.SetActive(false);
        };
        _trickIcon.gameObject.SetActive(false);

    }
}
