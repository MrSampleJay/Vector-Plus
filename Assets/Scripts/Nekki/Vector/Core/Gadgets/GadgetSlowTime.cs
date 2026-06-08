using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UI;
using UnityEngine;

namespace Nekki.Vector.Core.Gadgets
{
    public class GadgetSlowTime : Gadget
    {
        public const float Duration = 3;

        public const float Cooldown = 5;

        public float CurrentCooldown = 0;

        public GadgetSlowTime() : base(GadgetType.KillBot)
        {
        }

        public override void Play()
        {
            base.Play();
            CoroutineRunner.Instance.Run(SlowTime());
            CurrentCooldown = Cooldown;
        }

        public override void Stop()
        {
            base.Stop();
            CoroutineRunner.Instance.Run(CooldownTime());
        }

        public IEnumerator SlowTime()
        {
            float time = Duration;
            while (time > 0 && LevelMainController.current != null)
            {
                if (!LevelMainController.current.pauseRender)
                {
                    
                    LevelMainController.current.slowMode = true;
                    time -= Time.deltaTime;

                    float progress = Mathf.Clamp01(1f - (time / Duration));
                    GameplayView.Current.GadgetCooldownIcon.fillAmount = progress;
                }
                yield return null;
            }

            LevelMainController.current.slowMode = false;
            Stop();

            yield break;
        }

        public IEnumerator CooldownTime()
        {
            while (CurrentCooldown > 0 && LevelMainController.current != null)
            {
                if (!LevelMainController.current.pauseRender)
                {
                    CurrentCooldown -= Time.deltaTime;

                    float progress = Mathf.Clamp01(CurrentCooldown / Cooldown);
                    GameplayView.Current.GadgetCooldownIcon.fillAmount = progress;
                }
                yield return null;
            }

            CurrentCooldown = 0;
            yield break;
        }

        public override bool IsCanUse()
        {
            return CurrentCooldown <= 0 && !LevelMainController.current.tutorialPause;
        }

    }
}
