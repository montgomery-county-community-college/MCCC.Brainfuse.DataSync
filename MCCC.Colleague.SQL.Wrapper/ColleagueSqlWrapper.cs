using System.Data.SqlClient;
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

	public async Task<IEnumerable<ColleagueStudentEnrollment>> GetEnrollmentsForStudent(string studentId)
	{
		// make sure student id is a 7-10 digit number
		if (!RegexColleagueStudentId().IsMatch(studentId))
		{
			throw new ArgumentException("Student ID must be a 7-10 digit number.");
		}

		const string query = """
		                     SELECT STC_PERSON_ID COLLEAGUE_ID, CRS_NAME COURSE_NAME, COURSE_SECTIONS_ID COURSE_SECTION
		                     FROM STUDENT_ACAD_CRED STAC
		                              INNER JOIN STC_STATUSES SS
		                                         ON STAC.STUDENT_ACAD_CRED_ID = SS.STUDENT_ACAD_CRED_ID AND POS = 1 AND STC_STATUS IN ('A', 'N')
		                              INNER JOIN COURSES C ON STAC.STC_COURSE = C.COURSES_ID
		                     INNER JOIN STUDENT_COURSE_SEC  SCS ON STAC.STC_STUDENT_COURSE_SEC = SCS.STUDENT_COURSE_SEC_ID
		                     INNER JOIN COURSE_SECTIONS CS ON SCS.SCS_COURSE_SECTION = CS.COURSE_SECTIONS_ID
		                     WHERE STC_START_DATE <= GETDATE() AND STC_END_DATE >= CAST(GETDATE() AS DATE) AND STC_PERSON_ID = @StudentID
		                     """;
		return await _connection.QueryAsync<ColleagueStudentEnrollment>(query, new { StudentID = studentId });
	}

	public async Task<IEnumerable<ColleagueStudentEnrollment>> GetEnrollmentsForStudents(IEnumerable<string> ids)
	{
		var studentIds = ids as string[] ?? ids.ToArray();
		if (studentIds.Any(studentId => studentId != null && !RegexColleagueStudentId().IsMatch(studentId)))
		{
			throw new ArgumentException("Student ID must be a 7-10 digit number.");
		}

		const string query = """
		                     SELECT STC_PERSON_ID COLLEAGUE_ID, CRS_NAME COURSE_NAME, CRS_SUBJECT COURSE_SUBJECT, CRS_NO COURSE_NUMBER, COURSE_SECTIONS_ID COURSE_SECTION
		                     FROM STUDENT_ACAD_CRED STAC
		                              INNER JOIN STC_STATUSES SS
		                                         ON STAC.STUDENT_ACAD_CRED_ID = SS.STUDENT_ACAD_CRED_ID AND POS = 1 AND STC_STATUS IN ('A', 'N')
		                              INNER JOIN COURSES C ON STAC.STC_COURSE = C.COURSES_ID
		                     INNER JOIN STUDENT_COURSE_SEC  SCS ON STAC.STC_STUDENT_COURSE_SEC = SCS.STUDENT_COURSE_SEC_ID
		                     INNER JOIN COURSE_SECTIONS CS ON SCS.SCS_COURSE_SECTION = CS.COURSE_SECTIONS_ID
		                     WHERE STC_START_DATE <= GETDATE() AND STC_END_DATE >= CAST(GETDATE() AS DATE) AND STC_PERSON_ID IN @StudentIDs
		                     """;
		return await _connection.QueryAsync<ColleagueStudentEnrollment>(query, new { StudentIDs = studentIds });
	}
	
	~ColleagueSqlWrapper()
	{
		_connection.Close();
	}

    [GeneratedRegex(@"^\d{7,10}$")]
    private static partial Regex RegexColleagueStudentId();
}