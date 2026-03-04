using System.Text.Json;
using SRSS.IAM.Repositories.Entities;
using SRSS.IAM.Repositories.UnitOfWork;
using SRSS.IAM.Services.DTOs.Protocol;
using SRSS.IAM.Services.Mappers;

namespace SRSS.IAM.Services.ProtocolService
{
	public class ProtocolService : IProtocolService
	{
		private readonly IUnitOfWork _unitOfWork;

		public ProtocolService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task ApproveProtocolAsync(Guid protocolId, Guid reviewerId)
		{
			var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			if (protocol.Status == "Approved")
				throw new InvalidOperationException("Protocol đã được phê duyệt trước đó");

			protocol.Status = "Approved";
			protocol.ApprovedAt = DateTimeOffset.UtcNow;

			await _unitOfWork.Protocols.UpdateAsync(protocol);

			// Create evaluation record
			var evaluation = new ProtocolEvaluation
			{
				ProtocolId = protocolId,
				ReviewerId = reviewerId,
				EvaluationResult = "Approved",
				EvaluatedAt = DateTimeOffset.UtcNow
			};

			await _unitOfWork.ProtocolEvaluations.AddAsync(evaluation);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<ProtocolDetailResponse> CreateProtocolAsync(CreateProtocolRequest request)
		{
			var projectExists = await _unitOfWork.SystematicReviewProjects.AnyAsync(p => p.Id == request.ProjectId);
			if (!projectExists)
			{
				throw new KeyNotFoundException($"Project với ID {request.ProjectId} không tồn tại");
			}
			var protocol = new ReviewProtocol
			{
				ProjectId = request.ProjectId,
				ProtocolVersion = request.ProtocolVersion,
				Status = "Draft"
			};

			await _unitOfWork.Protocols.AddAsync(protocol);
			await _unitOfWork.SaveChangesAsync();

			return await GetProtocolByIdAsync(protocol.Id);
		}

		public async Task DeleteProtocolAsync(Guid protocolId)
		{
			var protocol = await _unitOfWork.Protocols.FindSingleAsync(p => p.Id == protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			if (protocol.Status == "Approved")
			{
				throw new InvalidOperationException("Không thể xóa protocol đã được phê duyệt");
			}

			// 1. Delete Protocol Versions
			var versions = await _unitOfWork.ProtocolVersions.FindAllAsync(v => v.ProtocolId == protocolId);
			foreach (var version in versions)
			{
				await _unitOfWork.ProtocolVersions.RemoveAsync(version);
			}

			// 2. Delete Protocol Evaluations
			var evaluations = await _unitOfWork.ProtocolEvaluations.FindAllAsync(e => e.ProtocolId == protocolId);
			foreach (var evaluation in evaluations)
			{
				await _unitOfWork.ProtocolEvaluations.RemoveAsync(evaluation);
			}

			// 3. Delete Search Strategies and children
			var searchStrategies = await _unitOfWork.SearchStrategies.GetByProtocolIdAsync(protocolId);
			foreach (var strategy in searchStrategies)
			{
				var searchStrings = await _unitOfWork.SearchStrings.GetByStrategyIdAsync(strategy.Id);
				foreach (var searchString in searchStrings)
				{
					var searchStringTerms = await _unitOfWork.SearchStringTerms.GetBySearchStringIdAsync(searchString.Id);
					foreach (var sst in searchStringTerms)
					{
						await _unitOfWork.SearchStringTerms.RemoveAsync(sst);
					}
					await _unitOfWork.SearchStrings.RemoveAsync(searchString);
				}

				var searchTerms = await _unitOfWork.SearchTerms.FindAllAsync(t => t.Id == strategy.Id);
				foreach (var term in searchTerms)
				{
					await _unitOfWork.SearchTerms.RemoveAsync(term);
				}
				await _unitOfWork.SearchStrategies.RemoveAsync(strategy);
			}

			// 4. Delete Search Sources
			var searchSources = await _unitOfWork.SearchSources.GetByProtocolIdAsync(protocolId);
			foreach (var source in searchSources)
			{
				await _unitOfWork.SearchSources.RemoveAsync(source);
			}

			// 5. Delete Study Selection Criteria
			var selectionCriterias = await _unitOfWork.SelectionCriterias.FindAllAsync(sc => sc.ProtocolId == protocolId);
			foreach (var criteria in selectionCriterias)
			{
				var inclusionCriteria = await _unitOfWork.InclusionCriteria.FindAllAsync(ic => ic.CriteriaId == criteria.Id);
				foreach (var ic in inclusionCriteria)
				{
					await _unitOfWork.InclusionCriteria.RemoveAsync(ic);
				}

				var exclusionCriteria = await _unitOfWork.ExclusionCriteria.FindAllAsync(ec => ec.CriteriaId == criteria.Id);
				foreach (var ec in exclusionCriteria)
				{
					await _unitOfWork.ExclusionCriteria.RemoveAsync(ec);
				}
				await _unitOfWork.SelectionCriterias.RemoveAsync(criteria);
			}

			// 6. Delete Study Selection Procedures
			var selectionProcedures = await _unitOfWork.SelectionProcedures.FindAllAsync(sp => sp.ProtocolId == protocolId);
			foreach (var procedure in selectionProcedures)
			{
				await _unitOfWork.SelectionProcedures.RemoveAsync(procedure);
			}

			// 7. Delete Quality Assessment Strategies
			var qaStrategies = await _unitOfWork.QualityStrategies.FindAllAsync(qa => qa.ProtocolId == protocolId);
			foreach (var qaStrategy in qaStrategies)
			{
				var checklists = await _unitOfWork.QualityChecklists.FindAllAsync(qc => qc.QaStrategyId == qaStrategy.Id);
				foreach (var checklist in checklists)
				{
					var criteria = await _unitOfWork.QualityCriteria.FindAllAsync(qcr => qcr.ChecklistId == checklist.Id);
					foreach (var criterion in criteria)
					{
						await _unitOfWork.QualityCriteria.RemoveAsync(criterion);
					}
					await _unitOfWork.QualityChecklists.RemoveAsync(checklist);
				}
				await _unitOfWork.QualityStrategies.RemoveAsync(qaStrategy);
			}

			// 8. Delete Data Extraction Strategies
			var extractionStrategies = await _unitOfWork.ExtractionStrategies.FindAllAsync(de => de.ProtocolId == protocolId);
			foreach (var extractionStrategy in extractionStrategies)
			{
				var forms = await _unitOfWork.ExtractionForms.FindAllAsync(f => f.ExtractionStrategyId == extractionStrategy.Id);
				foreach (var form in forms)
				{
					var dataItems = await _unitOfWork.DataItems.FindAllAsync(di => di.FormId == form.Id);
					foreach (var dataItem in dataItems)
					{
						await _unitOfWork.DataItems.RemoveAsync(dataItem);
					}
					await _unitOfWork.ExtractionForms.RemoveAsync(form);
				}
				await _unitOfWork.ExtractionStrategies.RemoveAsync(extractionStrategy);
			}

			// 9. Delete Data Synthesis Strategies
			var synthesisStrategies = await _unitOfWork.SynthesisStrategies.FindAllAsync(ds => ds.ProtocolId == protocolId);
			foreach (var synthesisStrategy in synthesisStrategies)
			{
				await _unitOfWork.SynthesisStrategies.RemoveAsync(synthesisStrategy);
			}

			// 10. Delete Dissemination Strategies
			var disseminationStrategies = await _unitOfWork.DisseminationStrategies.FindAllAsync(ds => ds.ProtocolId == protocolId);
			foreach (var disseminationStrategy in disseminationStrategies)
			{
				await _unitOfWork.DisseminationStrategies.RemoveAsync(disseminationStrategy);
			}

			// 11. Delete Project Timetables
			var timetables = await _unitOfWork.Timetables.FindAllAsync(pt => pt.ProtocolId == protocolId);
			foreach (var timetable in timetables)
			{
				await _unitOfWork.Timetables.RemoveAsync(timetable);
			}

			// Finally delete protocol
			await _unitOfWork.Protocols.RemoveAsync(protocol);
			await _unitOfWork.SaveChangesAsync();
		}

		public async Task<ProtocolDetailResponse> GetProtocolByIdAsync(Guid protocolId)
		{
			var protocol = await _unitOfWork.Protocols.GetByIdWithVersionsAsync(protocolId)
				?? throw new KeyNotFoundException($"Protocol {protocolId} không tồn tại");

			return protocol.ToDetailResponse();  
		}

		public async Task<List<ProtocolDetailResponse>> GetProtocolsByProjectIdAsync(Guid projectId)
		{
			var protocols = await _unitOfWork.Protocols.GetByProjectIdAsync(projectId);
			return protocols.Select(p => p.ToDetailResponse()).ToList();
		}

		public async Task<ProtocolDetailResponse> UpdateProtocolAsync(UpdateProtocolRequest request)
		{
			var protocol = await _unitOfWork.Protocols.GetByIdWithVersionsAsync(request.ProtocolId)
				?? throw new KeyNotFoundException($"Protocol {request.ProtocolId} không tồn tại");

			// If protocol is approved, create version history before updating
			if (protocol.Status == "Approved")
			{
				await CreateProtocolVersionAsync(protocol, request.ChangeSummary);

				// Increment version number
				var currentVersion = double.Parse(protocol.ProtocolVersion);
				protocol.ProtocolVersion = (currentVersion + 0.1).ToString("F1");
				protocol.Status = "Draft";
			}

			await _unitOfWork.Protocols.UpdateAsync(protocol);
			await _unitOfWork.SaveChangesAsync();

			return await GetProtocolByIdAsync(protocol.Id);
		}

		private async Task CreateProtocolVersionAsync(ReviewProtocol protocol, string? changeSummary)
		{
			var snapshotData = JsonSerializer.Serialize(new
			{
				protocol.Id,
				protocol.ProjectId,
				protocol.ProtocolVersion,
				protocol.Status,
				protocol.CreatedAt,
				protocol.ApprovedAt
			});

			var version = new ProtocolVersion
			{
				ProtocolId = protocol.Id,
				VersionNumber = protocol.ProtocolVersion,
				ChangeSummary = changeSummary ?? "Protocol updated after approval",
				SnapshotData = snapshotData
			};

			await _unitOfWork.ProtocolVersions.AddAsync(version);
		}
	}
}