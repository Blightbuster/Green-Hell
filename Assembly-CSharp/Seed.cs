using System;

public class Seed : Food
{
	public override bool CanBeFocuedInInventory()
	{
		return (!(base.transform.parent != null) || !(base.transform.parent.parent != null) || !(base.transform.parent.parent.GetComponent<Acre>() != null)) && base.CanBeFocuedInInventory();
	}
}
