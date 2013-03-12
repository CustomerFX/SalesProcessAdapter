using System;
using System.Text;
using Sublogix;
using Sublogix.Entities;

namespace FX.SalesProcess
{
	public enum ProcessElementType
	{
		Header,
		Footer
	}

	public class SalesProcessAdapter
	{
		public SalesProcessAdapter(string connectionString)
		{
			Repository = new Repository(connectionString);
		}

		public SalesProcessAdapter(Repository repository)
		{
			Repository = repository;
		}

		private Repository Repository { get; set; }
		private ProcessPlugin ProcessPlugin { get; set; }

		private string _userid;
		private string UserId
		{
			get
			{
				if (string.IsNullOrEmpty(_userid))
					_userid = Sublogix.Helpers.SalesLogixHelper.CurrentUserID;

				return _userid;
			}
		}

		private int _sequencenum = -1;
		private int SequenceNumber
		{
			get
			{
				_sequencenum++;
				return _sequencenum;
			}
		}

		public ProcessPlugin GetProcess(string pluginId)
		{
			return new ProcessPlugin(Repository, pluginId);
		}

		public string CreateProcess(string pluginId, string entityId)
		{
			ProcessPlugin = new ProcessPlugin(Repository, pluginId);

			var salesProcess = Repository.Create<Salesprocesses>();
			salesProcess.EntityId = entityId;
			salesProcess.Name = ProcessPlugin.Name;
			salesProcess.BasedonId = pluginId;
			salesProcess.Isactive = true;
			salesProcess.Save();

			AddProcessElement(salesProcess, ProcessElementType.Header);

			bool first = true;
			var stages = ProcessPlugin.GetStages();
			foreach (var stage in stages)
			{
				var auditStage = Repository.Create<Salesprocessaudit>();
				auditStage.EntityId = entityId;
				auditStage.Name = ProcessPlugin.Name;
				auditStage.SalesprocessId = salesProcess.SalesprocessesId;
				auditStage.Stagename = stage.Name;
				auditStage.Probability = (float)stage.Probability;
				auditStage.Updateprobability = ProcessPlugin.UpdateOpportunityPercent;
				auditStage.Processtype = "STAGE";
				auditStage.Stageorder = stage.OrderNumber;
				auditStage.Steporder = 0;
				auditStage.Required = false;
				auditStage.Completed = false;
				auditStage.StageguidId = stage.Id;
				auditStage.NextId = stage.NextId;
				auditStage.Estdays = stage.EstimatedDays;
				auditStage.Seq = SequenceNumber;
				auditStage.Inlan = true;
				auditStage.Inweb = true;

				if (first)
				{
					first = false;
					UpdateOpportunity(entityId, auditStage.Name, auditStage.Stageorder, ProcessPlugin.UpdateOpportunityPercent, auditStage.Probability);

					auditStage.Startdate = DateTime.Now;
					auditStage.Startedby = UserId;
					auditStage.Iscurrent = true;
				}
				else
					auditStage.Iscurrent = false;

				auditStage.Save();

				if (stage.Steps.Count == 0) AddNoStep(salesProcess, auditStage);

				foreach (var step in stage.Steps)
				{
					var auditStep = Repository.Create<Salesprocessaudit>();
					auditStep.EntityId = salesProcess.EntityId;
					auditStep.Name = ProcessPlugin.Name;
					auditStep.SalesprocessId = salesProcess.SalesprocessesId;
					auditStep.Stagename = stage.Name;
					auditStep.Stepname = step.Name;
					auditStep.Probability = stage.Probability;
					auditStep.Updateprobability = ProcessPlugin.UpdateOpportunityPercent;
					auditStep.Processtype = "STEP";
					auditStep.Stageorder = stage.OrderNumber;
					auditStep.Steporder = step.OrderNumber;
					auditStep.Completed = false;
					auditStep.StageId = auditStage.SalesprocessauditId;
					auditStep.StageguidId = stage.Id;
					auditStep.StepguidId = step.Id;
					auditStep.NextId = step.NextId;
					auditStep.Estdays = step.EstimatedDays;
					auditStep.Required = step.Required;
					auditStep.Actiontype = step.ActionType;
					auditStep.Inlan = step.InLan;
					auditStep.Inweb = step.InWeb;
					auditStep.Description = step.Description;
					auditStep.Seq = SequenceNumber;
					auditStep.Data = step.Data;
					auditStep.Save();
				}
			}

			AddProcessElement(salesProcess, ProcessElementType.Footer);

			return salesProcess.SalesprocessesId;
		}

		private void AddProcessElement(Salesprocesses salesProcess, ProcessElementType elementType)
		{
			var audit = Repository.Create<Salesprocessaudit>();
			audit.EntityId = salesProcess.EntityId;
			audit.Name = salesProcess.Name;
			audit.SalesprocessId = salesProcess.SalesprocessesId;
			audit.Probability = 0.0F;
			audit.Updateprobability = ProcessPlugin.UpdateOpportunityPercent;
			audit.Processtype = elementType.ToString().ToUpper();
			audit.Stageorder = 0;
			audit.Steporder = 0;
			audit.Required = false;
			audit.Startdate = DateTime.Now;
			audit.Startedby = UserId;
			audit.Completed = false;
			audit.Seq = SequenceNumber;
			audit.Inlan = true;
			audit.Inweb = true;
			audit.Save();
		}

		private void AddNoStep(Salesprocesses salesProcess, Salesprocessaudit stageAudit)
		{
			var audit = Repository.Create<Salesprocessaudit>();
			audit.EntityId = salesProcess.EntityId;
			audit.Name = salesProcess.Name;
			audit.SalesprocessId = salesProcess.SalesprocessesId;
			audit.Stagename = stageAudit.Name;
			audit.Stepname = "No Action";
			audit.Probability = stageAudit.Probability;
			audit.Processtype = "STEP";
			audit.Stageorder = stageAudit.Stageorder;
			audit.Steporder = 1;
			audit.Completed = false;
			audit.StageId = stageAudit.SalesprocessauditId;
			audit.StageguidId = stageAudit.StageguidId;
			audit.StepguidId = "STEPGUIDID";
			audit.Estdays = 0.0F;
			audit.Required = false;
			audit.Inlan = true;
			audit.Inweb = true;
			audit.Description = stageAudit.Name;
			audit.Iscurrent = false;
			audit.Seq = SequenceNumber;
			audit.Data = Encoding.UTF8.GetBytes("<Action></Action>");
			audit.Save();
		}

		private void UpdateOpportunity(string opportunityId, string stageName, int? stageOrder, bool updateProbability, float? probability)
		{
			var opp = Repository.GetById<Opportunity>(opportunityId);
			if (opp == null) return;

			opp.Stage = string.Format("{0}-{1}", (stageOrder.HasValue ? stageOrder.Value : 0), stageName);
			if (updateProbability)
				opp.Closeprobability = (int)(probability.HasValue ? probability.Value : 0.0F);

			opp.Save();
		}
	}
}
