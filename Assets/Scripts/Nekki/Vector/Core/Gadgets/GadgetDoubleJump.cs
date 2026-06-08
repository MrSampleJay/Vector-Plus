using Nekki.Vector.Core.Animation;
using Nekki.Vector.Core.Models;
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
    public class GadgetDoubleJump : Gadget
    {
        public const float Cooldown = 5;

        public float CurrentCooldown = 0;

        ModelHuman _userModel;

        AnimationReaction _doubleJump; 

        public GadgetDoubleJump() : base(GadgetType.KillBot)
        {
            _userModel = LevelMainController.current.Location.GetUserModel();
            var str = "DoubleJump|5";
            _doubleJump = AnimationLoader.ParseReaction(str.Split('|'));
        }

        public override void Play()
        {
            base.Play();
            _userModel.PlayAnimation(_doubleJump);
            GameplayView.Current.GadgetCooldownIcon.fillAmount = 1;
            CurrentCooldown = Cooldown;
            Stop();
        }

        public override void Stop()
        {
            base.Stop();
            CoroutineRunner.Instance.Run(CooldownTime());
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
