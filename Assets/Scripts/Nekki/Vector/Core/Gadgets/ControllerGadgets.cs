using System.Collections.Generic;

namespace Nekki.Vector.Core.Gadgets
{
	public class ControllerGadgets
	{
		private Dictionary<GadgetType, Gadget> _allGadgets = new Dictionary<GadgetType, Gadget>();

		public ControllerGadgets()
		{
			AddGadget(new GadgetKillBot());
		}

		private void AddGadget(Gadget gadget)
		{
			_allGadgets.Add(gadget.gadgetType, gadget);
		}

		public void ActivateGadget(GadgetType gadgetType)
		{
			var gadget = GetGadget(gadgetType);
			if (!CanActivateGadget(gadget))
			{
				return;
			}
			SpendCharge(gadgetType);
			gadget.Play();
			LevelMainController.current.Location.GetUserModel().UseGadget(gadget);
		}

		private Gadget GetGadget(GadgetType gadgetType)
		{
			_allGadgets.TryGetValue(gadgetType, out var gadget);
			return gadget;
		}

		private bool CanActivateGadget(Gadget gadget)
		{
			if (GetChargeCount(gadget.gadgetType) > 0)
			{
				return gadget.IsCanUse();
			}
			return false;
		}

		public int GetChargeCount(GadgetType gadgetType)
		{
			return UserDataManager.Instance.ShopData.GetCount("GADGET_FORCEBLASTER");
		}

		private void SpendCharge(GadgetType gadgetType)
		{
			UserDataManager.Instance.ShopData.Consume("GADGET_FORCEBLASTER");
		}
	}
}
