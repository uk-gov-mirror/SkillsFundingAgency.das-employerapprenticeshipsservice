using AutoMapper;
using Dapper;
using SFA.DAS.EAS.Domain.Configuration;
using SFA.DAS.NLog.Logger;
using SFA.DAS.Sql.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using SFA.DAS.EAS.Infrastructure.Data;

namespace SFA.DAS.EAS.TestCommon.TestRepositories
{
    public class TestTransactionRepository : BaseRepository, ITestTransactionRepository
    {
        private readonly IMapper _mapper;
        private readonly Lazy<EmployerFinanceDbContext> _db;

        public TestTransactionRepository(LevyDeclarationProviderConfiguration configuration, IMapper mapper,
            ILog logger, Lazy<EmployerFinanceDbContext> db)
            : base(configuration.DatabaseConnectionString, logger)
        {
            _mapper = mapper;
            _db = db;
        }

        public async Task SetTransactionLineDateCreatedToTransactionDate(IEnumerable<long> submissionIds)
        {
            var ids = submissionIds as long[] ?? submissionIds.ToArray();
            var idsDataTable = CreateSubmissionIdsDataTable(ids);
            var parameters = new DynamicParameters();

            parameters.Add("@submissionIds", idsDataTable.AsTableValuedParameter("[employer_financial].[SubmissionIds]"));

            await _db.Value.Database.Connection.ExecuteAsync(
                sql: "[employer_financial].[UpdateTransactionLineDateCreatedToTransactionDate_BySubmissionId]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);
        }

        public async Task SetTransactionLineDateCreatedToTransactionDate(IDictionary<long, DateTime?> submissionIds)
        {
            var idsDataTable = CreateSubmissionIdsDateDataTable(submissionIds);
            var parameters = new DynamicParameters();

            parameters.Add("@SubmissionIdsDates", idsDataTable.AsTableValuedParameter("[employer_financial].[SubmissionIdsDate]"));

            await _db.Value.Database.Connection.ExecuteAsync(
                sql: "[employer_financial].[UpdateTransactionLinesDateCreated_BySubmissionId]",
                param: parameters,
                transaction: _db.Value.Database.CurrentTransaction.UnderlyingTransaction,
                commandType: CommandType.StoredProcedure);
        }

        private static DataTable CreateSubmissionIdsDataTable(IEnumerable<long> submissionIds)
        {
            var table = new DataTable();

            table.Columns.AddRange(new[]
            {
                new DataColumn("SubmissionId", typeof(long))
            });

            foreach (var submissionId in submissionIds)
            {
                table.Rows.Add(submissionId);
            }

            table.AcceptChanges();

            return table;
        }


        private static DataTable CreateSubmissionIdsDateDataTable(IDictionary<long, DateTime?> submissionIds)
        {
            var table = new DataTable();

            table.Columns.AddRange(new[]
            {
                new DataColumn("SubmissionId", typeof(long)),
                new DataColumn("CreatedDate", typeof(DateTime))
            });

            foreach (var row in submissionIds)
            {
                table.Rows.Add(row.Key, row.Value);
            }

            table.AcceptChanges();

            return table;
        }
    }
}