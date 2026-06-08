using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utils;

namespace UI
{
	public class StoreTricksScreenView : ScreenViewWithCommonPayload<StoreTricksScreen>
	{
		public UnityEngine.UI.Button BackToLobbyButton;

		public UnityEngine.UI.Button BuyCoinsButton;

		public UnityEngine.UI.Button BuyButton;

		public GameObject ContentParent;

		public ScrollSnap ScrollSnap;

		public LayoutPosKeeper LayoutPosKeeper;

		public override void Init(StoreTricksScreen screen)
		{
			BuyCoinsButton.gameObject.SetActive(false);
			BackToLobbyButton.onClick.AddListener(new UnityAction(screen.BackToLobbyButton.PressedAction));
			BuyButton.onClick.AddListener(() =>
			{
				var items = StoreManager.Instance.GetItems(StoreItemType.Tricks);
                var item = items[ScrollSnap.CurrentIndex];
                if (!UserDataManager.Instance.ShopData.IsBought(item.Id))
                {
                    Buy(item, false, ScrollSnap.CurrentIndex);
                }
            });
			
			ScrollSnap.SnapEvent += i =>
			{
				EventSystem.current.SetSelectedGameObject(ScrollSnap.CurrentObject.GetComponent<HolderItem>().Button.gameObject);
			};
		}

		public static void Buy(Item trick, bool canEquip, int index)
		{
			if (UserDataManager.Instance.MainData.GetCoins() < trick.Price)
			{
				Game.Instance.ScreenManager.Popup<BuyCoinsPopup, BuyCoinsPayloadData>(new BuyCoinsPayloadData(trick.Price));
			}
			else
			{
				UserDataManager.RuntimeInfo.CurentItemId = trick.Id;
				UserDataManager.RuntimeInfo.CurentItemType = StoreItemType.Tricks;
				BuyItemAdditionalPayloadData payloadAdditional = null;
                if (trick.ItemType == StoreItemType.Gadgets)
				{
					var desc = LocalizationManager.Instance.GetTranslation("store_blaster_des");
					payloadAdditional = new BuyItemAdditionalPayloadData(desc, true);
				}

				CoroutineRunner.Instance.Run(ResourcesLoader.LoadItemSpriteAsync(trick.IconId, loaded =>
				{
                    var payload = new BuyItemPayloadData(loaded, trick.Id, trick.ItemType, canEquip, payloadAdditional, index);
                    Game.Instance.ScreenManager.Popup<BuyItemPopup, BuyItemPayloadData>(payload);
                }));


			}
		}

		public static void InsertDummies(Transform content, int count, bool right = false, LayoutPosKeeper layoutPosKeeper = null)
		{
            if (count > 0)
            {
                for (int i = count; i > 0; i--)
                {
                    var obj = Instantiate(Resources.Load<LayoutElement>("HolderItemDummy"), content);
                    if (right)
                    {
						if (layoutPosKeeper != null)
							layoutPosKeeper.right.Add(obj);
                    }
                    else
                    {
                        if (layoutPosKeeper != null)
                            layoutPosKeeper.left.Add(obj);
                    }
                }
            }
        }

		public static void InsertEmptyDummies(Transform content, int count)
		{
			if (count > 0)
			{
				for (int i = count; i > 0; i--)
				{
					Instantiate(Resources.Load<ScrollSnapItem>("HolderItemDummyEmpty"), content);
				}
			}
		}

		private void FillWithContent()
		{
		}

		public static void PutItemsIntoContent(ScrollSnap scrollSnap, List<Item> items, StoreItemType itemType, bool canEquip, bool buySeveralTimes = false)
		{
			scrollSnap.SnapEvent += i =>
			{
				switch (itemType)
				{
					case StoreItemType.Tricks:
						UserDataManager.RuntimeInfo.LastSelectedTrick = i;
						break;
					case StoreItemType.Gear:
						UserDataManager.RuntimeInfo.LastSelectedGear = i;
						break;
				}
			};
			for (int i = 0; i < items.Count; i++)
			{
                var item = items[i];

                var count = UserDataManager.Instance.ShopData.GetCount(item.Id);
				var isBought = UserDataManager.Instance.ShopData.IsBought(item.Id);

                int index = i;
				var obj = Instantiate(Resources.Load<HolderItem>("HolderItem"), scrollSnap._content);
				obj.Button.onClick.AddListener(() =>
				{
					var scrollItem = obj.GetComponent<ScrollSnapItem>();
					if (scrollItem != null && !scrollItem.IsSelected)
					{
						scrollSnap.Snap(index, false);
						return;
					}
					if (!buySeveralTimes && isBought)
					{
						return;
					}
					Buy(item, canEquip, index);
				});
				CoroutineRunner.Instance.Run(ResourcesLoader.LoadItemSpriteAsync(item.IconId, loaded => obj.Icon.sprite = loaded));
				obj.Set(LocalizationManager.Instance.GetTranslationByID(item.Id), false, count > 0, isBought, item.Price, item.IconId);
				var equipText = UserDataManager.Instance.ShopData.IsEquipped(item.Id) ? "store_unequip_but" : "store_equip_but";
				obj.EquipButtonText.text = LocalizationManager.Instance.GetTranslation(equipText);
				if (isBought)
					obj.EquipButton.gameObject.SetActive(canEquip);

                obj.ItemShopView.IconCheck.SetActive(UserDataManager.Instance.ShopData.IsEquipped(item.Id));

                obj.EquipButton.onClick.AddListener(() =>
				{
					var shopdata = UserDataManager.Instance.ShopData;
					var equipped = shopdata.IsEquipped(item.Id);
					if (equipped)
					{
						shopdata.Unequip(item.Id);
					}
					else
					{
						shopdata.Equip(item.Id);
					}
                    equipped = shopdata.IsEquipped(item.Id);

                    var equipText1 = equipped ? "store_unequip_but" : "store_equip_but";
                    obj.EquipButtonText.text = LocalizationManager.Instance.GetTranslation(equipText1);

					obj.ItemShopView.IconCheck.SetActive(equipped);
					UserDataManager.Instance.SaveUserDate();

					EventUtil.DispatchEvent(EventTypes.EQUIP_ITEM, equipped);
                });

				if (buySeveralTimes && isBought)
				{
                    obj.SetCount((uint)count);
                }
			}
		}

		public override void PreShow(CommonPayloadData payload)
		{
			LayoutPosKeeper.Clear();

			foreach (Transform child in ContentParent.transform)
			{
				Destroy(child.gameObject);
			}
			var items = StoreManager.Instance.GetItems(StoreItemType.Tricks);
			InsertDummies(ContentParent.transform, 10, false, LayoutPosKeeper);
			PutItemsIntoContent(ScrollSnap, items, StoreItemType.Tricks, false);
            InsertDummies(ContentParent.transform, 10, true, LayoutPosKeeper);

            ScrollSnap.StartIndex = 0;
            ScrollSnap.EndIndex = items.Count - 1;

			ScrollSnap._childOffset = 10;
        }

        public override void PostShow(CommonPayloadData payload)
		{
			LayoutPosKeeper.SetPositions();

			ScrollSnap.Recalculate();
			ScrollSnap.Snap(UserDataManager.RuntimeInfo.LastSelectedTrick, true);
		}
        
		public override void SetSelectedGO()
		{
			EventSystem.current.SetSelectedGameObject(ScrollSnap.CurrentObject.GetComponent<HolderItem>().Button.gameObject);
		}
        
		public override void OnEnable()
		{
			base.OnEnable();
			ScrollSnap.enabled = true;
		}

		public override void OnDisable()
		{
			base.OnDisable();
			ScrollSnap.enabled = false;
		}

		public override void Back()
		{
			BackToLobbyButton.onClick?.Invoke();
		}
	}
}
