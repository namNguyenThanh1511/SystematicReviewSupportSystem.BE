namespace SRSS.IAM.Repositories.Entities
{
    /// <summary>
    /// PRISMA 2020 flow diagram stages
    /// </summary>
    public enum PrismaStage
    {
        /// <summary>
        /// Identification phase (root)
        /// </summary>
        Identification = 0,

        /// <summary>
        /// Total records identified from all sources
        /// </summary>
        RecordsIdentified = 1,

        /// <summary>
        /// Duplicate records removed
        /// </summary>
        DuplicateRecordsRemoved = 2,

        /// <summary>
        /// Records screened (after duplicate removal)
        /// </summary>
        RecordsScreened = 3,

        /// <summary>
        /// Records excluded during screening
        /// </summary>
        RecordsExcluded = 4,

        /// <summary>
        /// Reports sought for retrieval
        /// </summary>
        ReportsSoughtForRetrieval = 5,

        /// <summary>
        /// Reports not retrieved
        /// </summary>
        ReportsNotRetrieved = 6,

        /// <summary>
        /// Reports assessed for eligibility
        /// </summary>
        ReportsAssessed = 7,

        /// <summary>
        /// Reports excluded after assessment
        /// </summary>
        ReportsExcluded = 8,

        /// <summary>
        /// Studies included in final review
        /// </summary>
        StudiesIncludedInReview = 9
    }
}
