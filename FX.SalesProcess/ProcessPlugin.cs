using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using Sublogix;
using Sublogix.Entities;
using FX.SalesProcess.Model;

namespace FX.SalesProcess
{
	public class ProcessPlugin
	{
		public ProcessPlugin(Repository repository, string pluginId)
		{
			this.Repository = repository;

			try
			{
				LoadPluginData(pluginId);
			}
			catch { throw; }
		}

		private Repository Repository { get; set; }

		public string Name
		{
			get
			{
				return _pluginData.DocumentElement.SelectSingleNode("//SalesProcess/Name").InnerText;
			}
		}

		public bool UpdateOpportunityPercent
		{
			get
			{
				return (_pluginData.DocumentElement.SelectSingleNode("//SalesProcess/UpdateOppPercent").InnerText == "T");
			}
		}

		public List<Stage> GetStages()
		{
			var stages = new List<Stage>();

			var stageNodes = _pluginData.DocumentElement.SelectNodes("//SalesProcess/Stages/Stage");
			foreach (XmlNode stageNode in stageNodes)
			{
				var stage = new Stage();
				stage.Probability = Convert.ToInt32(stageNode.SelectSingleNode("Probability").InnerText ?? "0");
				stage.OrderNumber = Convert.ToInt32(stageNode.SelectSingleNode("OrderNumber").InnerText ?? "0");
				stage.Id = stageNode.Attributes.GetNamedItem("ID").InnerText;
				stage.NextId = stageNode.Attributes.GetNamedItem("NextID").InnerText;
				stage.EstimatedDays = float.Parse(stageNode.SelectSingleNode("EstDays").InnerText ?? "0");
				stage.Name = stageNode.SelectSingleNode("Name").InnerText ?? this.Name;

				var stepNodes = stageNode.SelectNodes("Steps/Step");
				foreach (XmlNode stepNode in stepNodes)
				{
					var step = new Step();
					step.Name = stepNode.SelectSingleNode("Name").InnerText;
					step.OrderNumber = Convert.ToInt32(stepNode.SelectSingleNode("OrderNumber").InnerText);
					step.Id = stepNode.Attributes.GetNamedItem("ID").InnerText;
					step.NextId = stepNode.Attributes.GetNamedItem("NextID").InnerText;
					step.EstimatedDays = float.Parse(stepNode.SelectSingleNode("EstDays").InnerText);
					step.Required = (stepNode.SelectSingleNode("Required").InnerText == "T");

					XmlNode actionNode = stepNode.SelectSingleNode("Action");

					step.ActionType = actionNode.Attributes.GetNamedItem("Type").InnerText;

					switch (step.ActionType.ToLower())
					{
						case "form":
							step.InLan = (actionNode.SelectSingleNode("//FormAction/LanForm/Name").InnerText != string.Empty);
							step.InWeb = (actionNode.SelectSingleNode("//FormAction/WebForm/Name").InnerText != string.Empty);
							break;
						case "script":
							step.InLan = (actionNode.SelectSingleNode("//ScriptAction/LanForm/Name").InnerText != string.Empty);
							step.InWeb = (actionNode.SelectSingleNode("//ScriptAction/WebForm/Name").InnerText != string.Empty);
							break;
						default:
							step.InLan = true;
							step.InWeb = true;
							break;
					}

					step.Data = Encoding.UTF8.GetBytes(actionNode.OuterXml);

					step.Description = stepNode.SelectSingleNode("Description").InnerText;
					if (string.IsNullOrEmpty(step.Description)) step.Description = step.ActionType;

					stage.Steps.Add(step);
				}

				stages.Add(stage);
			}

			return stages;
		}

		private XmlDocument _pluginData;
		private void LoadPluginData(string pluginId)
		{
			var plugin = GetPlugin(pluginId);
			if (plugin == null)
				throw new Exception(string.Format("The plugin ID {0} does not exist or could not be loaded.", pluginId));

			_pluginData = new XmlDocument();
			_pluginData.LoadXml(Encoding.UTF8.GetString(plugin.Data));
		}

		private Plugin GetPlugin(string pluginId)
		{
			return Repository.GetById<Plugin>(pluginId);
		}
	}
}
