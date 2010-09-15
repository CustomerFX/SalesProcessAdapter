using System;
using System.Collections.Generic;

namespace FX.SalesProcess.Model
{
	internal class Stage
	{
		public Stage()
		{
			this.Steps = new List<Step>();
		}

		public string Id { get; set; }
		public string Name { get; set; }
		public int Probability { get; set; }
		public int OrderNumber { get; set; }
		public string NextId { get; set; }
		public float EstimatedDays { get; set; }

		public List<Step> Steps { get; set; }
	}
}
