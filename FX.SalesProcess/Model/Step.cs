using System;

namespace FX.SalesProcess.Model
{
	public class Step
	{
		public string Id { get; set; }
		public string Name { get; set; }
		public int OrderNumber { get; set; }
		public string NextId { get; set; }
		public float EstimatedDays { get; set; }
		public string Required { get; set; }
		public string ActionType { get; set; }
		public string InLan { get; set; }
		public string InWeb { get; set; }
		public string Description { get; set; }
		public byte[] Data { get; set; }
	}
}
