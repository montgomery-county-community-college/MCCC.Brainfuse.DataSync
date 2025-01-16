using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Dapper;
using MCCC.Colleague.SQL.Wrapper.Models;

namespace MCCC.Colleague.SQL.Wrapper;

public partial class ColleagueSqlWrapper
{
	private readonly SqlConnection _connection;

	public ColleagueSqlWrapper(string connectionString)
	{
		// make a sql connection with the given connection string
		_connection = new SqlConnection(connectionString);
		DefaultTypeMap.MatchNamesWithUnderscores = true;
	}

	public async Task<IEnumerable<ColleagueStudentEnrollment>> GetEnrollmentsForStudents(IEnumerable<string> ids)
	{
		var studentIds = ids as string[] ?? ids.ToArray();
		
		var badStudentRecords = studentIds.Where(id => !RegexColleagueStudentId().IsMatch(id)).ToList();
		
		if (badStudentRecords.Any())
		{
			var badStudentIds = string.Join(",", badStudentRecords);
			throw new ArgumentException($"Student ID must be a 7-10 digit number. Bad ids include {badStudentIds}.");
		}
		
		var dataTable = new DataTable();
		dataTable.Columns.Add("STUDENT_ID", typeof(string));
		foreach (var studentId in studentIds)
		{
			dataTable.Rows.Add(studentId);
		}
		
		if (dataTable.Rows.Count == 0)
		{
			return new List<ColleagueStudentEnrollment>();
		}

		var dynamicParameters = new DynamicParameters();
		dynamicParameters.Add("@STUDENT_IDS", dataTable.AsTableValuedParameter("srvintegration.M45_STUDENT_ID_LIST_TYPE"));
		return await _connection.QueryAsync<ColleagueStudentEnrollment>("srvintegration.GET_ENROLLMENTS_FOR_STUDENT_LIST",
			dynamicParameters, commandType: CommandType.StoredProcedure);
	}
	
	~ColleagueSqlWrapper()
	{
		_connection.Close();
	}

    [GeneratedRegex(@"^\d{7,10}$")]
    private static partial Regex RegexColleagueStudentId();
}