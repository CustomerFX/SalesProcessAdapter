using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using FX.SalesProcess;
using FX.SalesProcess.Model;
using Sublogix;
using Sublogix.Entities;

namespace FX.SalesProcess.Tests
{
	public class SalesProcessAdapterTests
	{
		private Repository Repository { get; set; }

		public SalesProcessAdapterTests()
		{
			this.Repository = new Repository(Global.ConnectionString);
		}

		private void ProcessCleanup()
		{
			var procList = Repository.Find<SalesProcesses>(x => x.EntityId == Global.OpportunityId);
			foreach (var proc in procList)
				proc.Delete();

			var auditList = Repository.Find<SalesProcessAudit>(x => x.EntityId == Global.OpportunityId);
			foreach (var audit in auditList)
				audit.Delete();
		}

		[Fact] // integration test
		public void Can_Create_Sales_Process()
		{
			var adapter = new SalesProcessAdapter(Repository);
			Assert.DoesNotThrow(() => adapter.CreateProcess(Global.ProcessPluginId, Global.OpportunityId));

			ProcessCleanup();
		}
	}
}
