using System;
using System.Collections.Generic;

namespace SRSS.IAM.Services.DTOs.Checklist
{
    public class ChecklistAutoFillWorkItem
    {
        public Guid ReviewChecklistId { get; set; }
        public Guid UserId { get; set; }
        public string FileName { get; set; } = string.Empty;
        /// <summary>
        /// The PDF content stored as a byte array so the work item outlives the HTTP request stream.
        /// </summary>
        public byte[] PdfBytes { get; set; } = Array.Empty<byte>();
    }

    public class GeminiChecklistMappingResponse
    {
        public List<GeminiChecklistItemMapping> Mappings { get; set; } = new();
    }

    public class GeminiChecklistItemMapping
    {
        public string ItemNumber { get; set; } = string.Empty;
        public string Topic { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public bool IsReported { get; set; }
        public string Reasoning { get; set; } = string.Empty;
    }

    /// <summary>
    /// SignalR payload sent to the client for each stage of the auto-fill pipeline.
    /// </summary>
    public class ChecklistAutoFillStatusDto
    {
        public Guid ReviewChecklistId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public double? CompletionPercentage { get; set; }
        public int? TotalItems { get; set; }
        public int? MappedItems { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// Status constants for the auto-fill workflow.
    /// </summary>
    public static class AutoFillStatus
    {
        public const string Queued = "Queued";
        public const string ExtractingText = "ExtractingText";
        public const string TextExtracted = "TextExtracted";
        public const string AnalyzingWithAI = "AnalyzingWithAI";
        public const string SavingResults = "SavingResults";
        public const string Completed = "Completed";
        public const string Failed = "Failed";
    }
}
