using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FX.SalesProcess;
using FX.SalesProcess.Model;
using Sublogix;

namespace FX.SalesProcess.Tests
{
	public class SalesProcessAdapterTests
	{
		private Repository Repository { get; set; }

		public SalesProcessAdapterTests()
		{
			this.Repository = new Repository(Global.ConnectionString);
		}

		[Fact] // integration test
		public void Can_Create_Sales_Process()
		{
			var adapter = new SalesProcessAdapter(Repository);
			Assert.DoesNotThrow(() => adapter.CreateProcess(Global.ProcessPluginId, Global.OpportunityId));
		}
	}
}
