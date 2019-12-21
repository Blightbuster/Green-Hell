using System;

public static class P2PAddressCreator
{
	public static IP2PAddress CreateAddress(ETransporLayerType transport_layer)
	{
		if (transport_layer == ETransporLayerType.UNet)
		{
			return new P2PAddressUnet();
		}
		if (transport_layer != ETransporLayerType.Steam)
		{
			throw new NotImplementedException();
		}
		return new P2PAddressSteam();
	}
}
