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
	public class ProcessPluginTests
	{
		private Repository Repository { get; set; }

		public ProcessPluginTests()
		{
			this.Repository = new Repository(Global.ConnectionString);
		}

		[Fact]
		public void Can_Get_ProcessName()
		{
			var plugin = new ProcessPlugin(Repository, Global.ProcessPluginId);
			Assert.Equal("New Technology SMB", plugin.Name);
		}

		[Fact]
		public void Can_Get_Update_Opportunity_Percent()
		{
			var plugin = new ProcessPlugin(Repository, Global.ProcessPluginId);
			Assert.Equal(true, plugin.UpdateOpportunityPercent);
		}

		[Fact]
		public void Can_Get_Process_Stages()
		{
			var plugin = new ProcessPlugin(Repository, Global.ProcessPluginId);
			Assert.Equal(6, plugin.GetStages().Count);
		}

		[Fact]
		public void Can_Get_Process_Stage_Steps()
		{
			var plugin = new ProcessPlugin(Repository, Global.ProcessPluginId);
			Assert.Equal(2, plugin.GetStages().FirstOrDefault().Steps.Count);
		}
	}
}
