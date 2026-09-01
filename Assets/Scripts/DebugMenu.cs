using Nekki.Vector.GUI.Scenes.Run;
using UnityEngine;
using UnityEngine.UI;

public class DebugMenu : MonoBehaviour
{
    public FPSMeter FPSMeter;

    public RunStats RunStats;

    protected PlayerInputActions actions;

    private void Awake()
    {
        if (!Game.Instance.Snail)
        {
            return;
        }

        if (actions == null)
            actions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (!Game.Instance.Snail)
        {
            return;
        }

        actions.Enable();

        actions.UI.Back.performed += _ =>
        {
            if (LevelMainController.current == null)
            {
                return;
            }
            LevelMainController.current.pauseRender = !LevelMainController.current.pauseRender;
        };

        actions.Gameplay.Restart.performed += _ =>
        {
            if (LevelMainController.current == null || !LevelMainController.current.CanPauseOrReload)
            {
                return;
            }
            LevelMainController.current.ReloadButton();
        };
    }

    private void OnDisable()
    {
        if (!Game.Instance.Snail)
        {
            return;
        }

        actions.Disable();
    }

    public void Init()
	{
        if ((Game.Instance.Snail && Game.Instance.SnailSett.ShowUI) || Game.Instance.ForceShowStats)
        {
            FPSMeter.gameObject.SetActive(true);
            RunStats.gameObject.SetActive(true);
        }
	}
}
