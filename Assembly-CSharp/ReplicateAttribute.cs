using System;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class ReplicateAttribute : Attribute
{
	public ReplicateAttribute(params string[] values)
	{
		this.parameters = values;
	}

	public string[] parameters;
}
