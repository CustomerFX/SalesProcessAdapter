using System;
using System.Collections.Generic;
using System.Linq;
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
		public SalesProcessAdapter(string ConnectionString)
		{
			this.Repository = new Repository(ConnectionString);
		}

		public SalesProcessAdapter(Repository Repository)
		{
			this.Repository = Repository;
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

		public void CreateProcess(string pluginId, string entityId)
		{
			this.ProcessPlugin = new ProcessPlugin(this.Repository, pluginId);

			var salesProcess = Repository.Create<SalesProcesses>();
			salesProcess.EntityId = entityId;
			salesProcess.Name = ProcessPlugin.Name;
			salesProcess.BasedOnId = pluginId;
			salesProcess.Isactive = "T";
			salesProcess.Save();

			AddProcessElement(salesProcess, ProcessElementType.Header);

			bool first = true;
			string firstStageName = string.Empty;
			int firstStageNumber = 0;
			float firstStageProbability = 0.0F;

			var stages = ProcessPlugin.GetStages();
			foreach (var stage in stages)
			{
				var auditStage = Repository.Create<SalesProcessAudit>();
				auditStage.EntityId = entityId;
				auditStage.Name = ProcessPlugin.Name;
				auditStage.SalesprocessId = salesProcess.Id;
				auditStage.StageName = stage.Name;
				auditStage.Probability = (float)stage.Probability;
				auditStage.Updateprobability = ProcessPlugin.UpdateOpportunityPercent;
				auditStage.Processtype = "STAGE";
				auditStage.StageOrder = stage.OrderNumber;
				auditStage.StepOrder = 0;
				auditStage.Required = "F";
				auditStage.Completed = "F";
				auditStage.StageguidId = stage.Id;
				auditStage.NextId = stage.NextId;
				auditStage.Estdays = stage.EstimatedDays;
				auditStage.Seq = SequenceNumber;
				auditStage.Inlan = "T";
				auditStage.Inweb = "T";

				if (first)
				{
					first = false;
					firstStageName = auditStage.Name;
					if (auditStage.StageOrder.HasValue) firstStageNumber = auditStage.StageOrder.Value;

					UpdateOpportunity(entityId, auditStage.Name, auditStage.StageOrder, (ProcessPlugin.UpdateOpportunityPercent == "T"), auditStage.Probability);

					auditStage.Startdate = DateTime.Now;
					auditStage.Startedby = UserId;
					auditStage.IsCurrent = "T";
				}
				else
					auditStage.IsCurrent = "F";

				auditStage.Save();

				if (stage.Steps.Count == 0) AddNoStep(salesProcess, auditStage);

				foreach (var step in stage.Steps)
				{
					var auditStep = Repository.Create<SalesProcessAudit>();
					auditStep.EntityId = salesProcess.EntityId;
					auditStep.Name = ProcessPlugin.Name;
					auditStep.SalesprocessId = salesProcess.Id;
					auditStep.StageName = stage.Name;
					auditStep.Stepname = step.Name;
					auditStep.Probability = stage.Probability;
					auditStep.Updateprobability = ProcessPlugin.UpdateOpportunityPercent;
					auditStep.Processtype = "STEP";
					auditStep.StageOrder = stage.OrderNumber;
					auditStep.StepOrder = step.OrderNumber;
					auditStep.Completed = "F";
					auditStep.StageId = auditStage.Id;
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
		}

		private void AddProcessElement(SalesProcesses salesProcess, ProcessElementType elementType)
		{
			var audit = Repository.Create<SalesProcessAudit>();
			audit.EntityId = salesProcess.EntityId;
			audit.Name = salesProcess.Name;
			audit.SalesprocessId = salesProcess.Id;
			audit.Probability = 0.0F;
			audit.Updateprobability = ProcessPlugin.UpdateOpportunityPercent;
			audit.Processtype = elementType.ToString().ToUpper();
			audit.StageOrder = 0;
			audit.StepOrder = 0;
			audit.Required = "F";
			audit.Startdate = DateTime.Now;
			audit.Startedby = UserId;
			audit.Completed = "F";
			audit.Seq = SequenceNumber;
			audit.Inlan = "T";
			audit.Inweb = "T";
			audit.Save();
		}

		private void AddNoStep(SalesProcesses salesProcess, SalesProcessAudit stageAudit)
		{
			var audit = Repository.Create<SalesProcessAudit>();
			audit.EntityId = salesProcess.EntityId;
			audit.Name = salesProcess.Name;
			audit.SalesprocessId = salesProcess.Id;
			audit.StageName = stageAudit.Name;
			audit.Stepname = "No Action";
			audit.Probability = stageAudit.Probability;
			audit.Processtype = "STEP";
			audit.StageOrder = stageAudit.StageOrder;
			audit.StepOrder = 1;
			audit.Completed = "F";
			audit.StageId = stageAudit.Id;
			audit.StageguidId = stageAudit.StageguidId;
			audit.StepguidId = "STEPGUIDID";
			audit.Estdays = 0.0F;
			audit.Required = "F";
			audit.Inlan = "T";
			audit.Inweb = "T";
			audit.Description = stageAudit.Name;
			audit.IsCurrent = "F";
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
