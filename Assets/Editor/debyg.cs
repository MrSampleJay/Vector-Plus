using System;
using Nekki.Vector.Core.Location;
using UnityEditor;
using UnityEngine;

public class debyg : MonoBehaviour
{
    [MenuItem("Debug/Refresh current screen")]
    public static void Refresh()
    {
        Game.Instance.ScreenManager.Refresh();
    }
    
    [MenuItem("Debug/Test")]
    public static void Test()
    {
        ResourceManager.ClearTextureCache();
    }

    [MenuItem("Debug/Add Coins")]
    public static void AddCoins()
    {
        UserDataManager.Instance.MainData.AddCoins(1000);
    }
    [MenuItem("Debug/Add Track stat")]
    public static void AddTrackStat()
    {
        foreach (var info in LocationManager.Instance.locations.Values)
        {
            foreach (var location in info.Values)
            {
                foreach (var track in location.CurrentStoryInfos)
                {
                    var record = new Record();
                    record.Stars = 3;
                    record.Points = 100;
                    record.TimeSpan = new TimeSpan(0, 0, 1, 0, 0);
                    UserDataManager.Instance.GameStats.TrackStatsAdd(track.Name, record);
                }
            }

        }

        foreach (var trick in StoreManager.Instance.GetItems(StoreItemType.Tricks))
        {
            UserDataManager.Instance.ShopData.Add(trick.Id, 1);
        }

        foreach (var trick in StoreManager.Instance.GetItems(StoreItemType.Gear))
        {
            UserDataManager.Instance.ShopData.Add(trick.Id, 1);
        }
    }

    [MenuItem("Debug/Reload")]
    public static void Reload()
    {
        LevelMainController.current.ReloadButton();
    }

    [MenuItem("Debug/Get First Bonus")]
    public static void GetBonus()
    {
        foreach (var item in LevelMainController.current.Location.Sets.Items)
        {
            if (item is ItemScoreRunner bonus)
            {
                bonus.Play();
                bonus.AddBonus(LevelMainController.current.Location.GetUserModel());
                return;
            }
        }
    }

    [MenuItem("Debug/Get First Trick")]
    public static void GetFirstTrick()
    {
        foreach (var area in LevelMainController.current.Location.Sets.Areas)
        {
            if (area is TrickAreaRunner trick)
            {
                trick.Activate(LevelMainController.current.Location.GetUserModel());
                TrickAreaRunner.TrickGetByModel(LevelMainController.current.Location.GetUserModel());
                return;
            }
        }
    }

    [MenuItem("Debug/Start Player Physics")]
    public static void ModelPhysics()
    {
        LevelMainController.current.Location.GetUserModel().ControllerPhysics.Start();
    }

    [MenuItem("Debug/Run Effect")]
    public static void RunEffect()
    {
        LevelMainController.current.Location.GetUserModel().controllerModelEffect.RunAntibotEffect();
    }
}
