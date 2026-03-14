namespace SRSS.IAM.Services.DTOs.DataExtraction
{
	/// <summary>
	/// 1 column trong grid
	/// </summary>
	public class DataGridColumnDto
	{
		public Guid FieldId { get; set; }
		public string ColumnKey { get; set; } = string.Empty;
		public string ColumnLabel { get; set; } = string.Empty;

		/// <summary>
		/// Field type: 0=Text, 1=Integer, 2=Decimal, 3=Boolean, 4=SingleSelect, 5=MultiSelect
		/// </summary>
		public int FieldType { get; set; }
	}

	/// <summary>
	/// 1 cell trong grid
	/// </summary>
	public class DataGridCellDto
	{
		public Guid FieldId { get; set; }
		public string DisplayValue { get; set; } = string.Empty;
		public ExtractionAnswerValueDto? RawValue { get; set; }
	}

	/// <summary>
	/// 1 row trong grid
	/// </summary>
	public class DataGridRowDto
	{
		public Guid PaperId { get; set; }
		public string Title { get; set; } = string.Empty;

		/// <summary>
		/// Status: 0=ToDo, 1=InProgress, 2=AwaitingConsensus, 3=Completed
		/// </summary>
		public int Status { get; set; }
		public string StatusText { get; set; } = string.Empty;

		public List<DataGridCellDto> Cells { get; set; } = new();
	}

	/// <summary>
	/// Grid data với pagination
	/// </summary>
	public class DataGridPageDto
	{
		public List<DataGridColumnDto> Columns { get; set; } = new();
		public List<DataGridRowDto> Rows { get; set; } = new();

		public int TotalCount { get; set; }
		public int PageNumber { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public bool HasPreviousPage { get; set; }
		public bool HasNextPage { get; set; }
	}
}