using System.Data.SqlClient;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using CsvHelper;
using Dapper;
using MCCC.Brainfuse.API.Wrapper;
using MCCC.Brainfuse.DataSync.Models;
using MCCC.Colleague.SQL.Wrapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using OfficeOpenXml;

namespace MCCC.Brainfuse.DataSync;

public class Worker : BackgroundService
{
    private readonly AppSettings _options;
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger, IOptions<AppSettings> options)
    {
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Process has started.");
        _logger.LogInformation("Update Mode is {UpdateMode}", _options.UpdateMode);
        _logger.LogInformation("Export Type is {ExportType}", _options.ExportType);

        var reportLocation = _options.ExportLocation;

        var colleagueSqlWrapper =
            new ColleagueSqlWrapper(_options.ConnectionStrings.Colleague);

        var baseUrl = _options.BrainfuseApiSettings.BrainfuseApiBaseUrl;
        var brainFuse = new BrainfuseApiWrapper(baseUrl, _options.BrainfuseApiSettings.BrainfuseApiKey,
            _options.UseProxy, _options.BrainfuseApiSettings.ThreadCount,
            _options.BrainfuseApiSettings.PageSize);

        var lookupStartDate = DateTime.Today.AddDays(-(_options.ReportLookBackPeriod ?? 1));
        var lookupStartDateString = lookupStartDate.ToString("MM/dd/yyyy");

        var lookupEndDate = DateTime.Today.AddDays(-1);
        var lookupEndDateString = lookupEndDate.ToString("MM/dd/yyyy");

        if (!string.IsNullOrEmpty(_options.SpecificSearchDate) && !string.IsNullOrEmpty(_options.ReportStartDate))
        {
            throw new ArgumentException("SpecificSearchDate and ReportStartDate should not both be set.");
        }

        if (!string.IsNullOrEmpty(_options.SpecificSearchDate))
        {
            lookupStartDateString = _options.SpecificSearchDate;
            lookupEndDateString = !string.IsNullOrEmpty(_options.SpecificEndDate) ? _options.SpecificEndDate : _options.SpecificSearchDate;
        }
        else if (!string.IsNullOrEmpty(_options.ReportStartDate))
        {
            lookupStartDateString = _options.ReportStartDate;
        }

        var courseMappings = GetCourseMappings(_options.McccTutorCourseMappingFile);

        var tutorData = new List<ColleagueTutoringSessionData>();

        var boostAttendanceData = await GetBoostAttendance(brainFuse, courseMappings, colleagueSqlWrapper,
            lookupStartDateString, lookupEndDateString);

        if (boostAttendanceData != null)
        {
            tutorData.AddRange(boostAttendanceData);
        }

        var liveSessions = await brainFuse.GetLiveSessions(lookupStartDateString, lookupEndDateString);

        // create a mapper from BrainfuseLiveSession to ColleagueTutoringSessionData is it's not null
        if (liveSessions != null)
        {
            var liveSessionData = liveSessions.Select(e =>
            {
                if (e.CollegeId == null) throw new ApplicationException($"Live session {e.EventId} College ID is null");

                var eventId = e.EventId;
                if (e.Type == "IA")
                {
                    eventId = e.RequestId.ToString();
                }

                return new ColleagueTutoringSessionData(e.CollegeId,
                    $"live_session_{eventId}_{e.CollegeId}_{e.Date:yyyyMMdd_HHmmss}",
                    e.Source == "Other" ? "MCCC" : "Brainfuse")
                {
                    SessionType = ColleagueTutoringSessionData.SessionTypes.LiveSession,
                    StartDate = e.Date,
                    EndDate = e.Date.AddMinutes(e.Minutes),
                    Location = "Online",
                    Subject = e.Subject,
                    Course = $"Live Session for {e.Subject}",
                    Provider = e.Source == "Other" ? "MCCC" : "Brainfuse",
                    Attended = true, // Live attendance is always true
                };
            }).ToList();
            tutorData.AddRange(liveSessionData.DistinctBy(td => td.IntegrationId));
        }

        var writingLabAttendance = await brainFuse.GetWritingLabAttendance(lookupStartDateString, lookupEndDateString);

        if (writingLabAttendance != null)
        {
            writingLabAttendance = writingLabAttendance.DistinctBy(wl => wl.Uid).ToList();

            // create a mapper from BrainfuseWritingLab to ColleagueTutoringSessionData
            var writingLabAttendanceData = writingLabAttendance.Select(e =>
            {
                if (e.CollegeId == null) throw new ApplicationException($"Writing Lab {e.Uid} College ID is null");
                return new ColleagueTutoringSessionData(e.CollegeId, $"writing_lab_{e.Uid}_{e.CollegeId}_{e.Date:yyyyMMdd_HHmmss}",
                    e.Source == "Other" ? "MCCC" : "Brainfuse")
                {
                    SessionType = ColleagueTutoringSessionData.SessionTypes.WritingLab,
                    StartDate = e.Date,
                    EndDate = e.Date.AddMinutes(e.Minutes),
                    Location = "Online",
                    Subject = "Writing Lab",
                    Course = "Writing Lab",
                    Provider = e.Source == "Other" ? "MCCC" : "Brainfuse",
                    Attended = true, // Writing Lab attendance is always true
                };
            }).ToList();
            tutorData.AddRange(writingLabAttendanceData);
        }

        tutorData = tutorData.Where(td => !string.IsNullOrEmpty(td.StudentId)).ToList();

        var studentIds = tutorData.Where(td => !string.IsNullOrEmpty(td.StudentId))
            .Select(td => td.StudentId!);

        var coursesForStudents = (await colleagueSqlWrapper
                .GetEnrollmentsForStudents(studentIds.Distinct()))
            .GroupBy(fs => fs.ColleagueId)
            .ToDictionary(fs => fs.Key, fs => fs);

        foreach (var sessionData in tutorData)
        {
            _logger.LogDebug("Current student: {student}", sessionData.StudentId);

            if (sessionData.StudentId == null)
            {
                _logger.LogError("Student for Boost Attendance {eventId} is null", sessionData.IntegrationId);
            }

            if (sessionData.SessionType == ColleagueTutoringSessionData.SessionTypes.WritingLab)
            {
                // we don't match writing labs to enrollments
                continue;
            }

            sessionData.CoursesForSubject = courseMappings
                .Where(cm => cm.BrainfuseSubject.Trim() == sessionData.Subject?.Trim()).Select(cm => cm.McccCourse);

            var sessionDataCoursesForSubject = sessionData.CoursesForSubject.ToList();
            if (sessionDataCoursesForSubject.Count == 0)
            {
                // no matches for subject
                _logger.LogDebug("No courses for subject: {subject}", sessionData.Subject);

                // if there are no mappings for this subject, check if the subject matches (^(\w{3,4}) (\d{3,4}\w?)).*$ and use that as the course
                if (sessionData.Subject == null)
                {
                    sessionData.Course = "BrainFuse subject is null.";
                    continue;
                }

                var match = Regex.Match(sessionData.Subject, @"^(\w{3,4}) (\d{3,4}\w?)");
                if (match.Success)
                {
                    sessionDataCoursesForSubject.Add(match.Value);
                }
                else
                {
                    sessionData.Course = "BrainFuse subject has no mapped courses.";
                    continue;
                }
            }

            if (coursesForStudents.TryGetValue(sessionData.StudentId, out var coursesForStudent))
            {
                //coursesForStudent.Dump(student.StudentId);
                sessionData.CoursesForStudent = coursesForStudent.ToList();

                var enrolledCourseNames =
                    sessionData.CoursesForStudent.Select(cs => cs.CourseSubject + " " + cs.CourseNumber);
                //enrolledCourseNames.Dump();

                var matchingCourses = sessionDataCoursesForSubject?.Where(cs => enrolledCourseNames.Any(c => c == cs));

                var matchingCoursesList = matchingCourses?.ToList();
                if (matchingCoursesList != null)
                {
                    sessionData.Course = matchingCoursesList.Count == 0
                        ? "No Matches"
                        : string.Join(',', matchingCoursesList);
                }
            }
            else
            {
                sessionData.Course = "No Course Enrollments";
            }
        }

        if (_options.UpdateMode)
        {
            if (_options.ExportType == "CROA")
            {
                await WriteTutoringDataToCroa(tutorData);
            }
            else if (_options.ExportType == "CSV")
            {
                await WriteTutoringDataToCsv(tutorData);
            }
            if (_options.StarfishExport)
            {
                await WriteTutoringDataToStarfishFile();
            }
        }
        else
        {
            _logger.LogInformation("Update mode set to false, not writing to ODS Database");
        }

        _logger.LogInformation("Process is done.");
        Environment.Exit(0);
    }

    private async Task WriteTutoringDataToStarfishFile()
    {
        var reportLocation = _options.StarfishExportLocation + @"\brainfuse_meetings.csv";

        var tutorDataQuery = """
                             SELECT integration_id
                                  , source
                                  , student_id
                                  , start_dt as start_date
                                  , end_dt   as end_date
                                  , location
                                  , course
                                  , reason
                                  , provider_name
                                  , notes
                             FROM [dbo].[M45_VW_STARFISH_BRAINFUSE_MEETINGS];
                             """;
        
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        await using var connection = new SqlConnection(_options.ConnectionStrings.Croa);
        
        try
        {
            var croaTutorData = await connection.QueryAsync<ColleagueTutoringSessionData>(tutorDataQuery);
            
            var tutorData = croaTutorData.Select(t => new StarfishMeeting
            {
                IntegrationId = t.IntegrationId,
                StudentId = t.StudentId,
                Source = t.Source,
                StartDt = t.StartDate.ToString("yyyy-MM-dd hh:mm:ss"),
                EndDt = t.EndDate.ToString("yyyy-MM-dd hh:mm:ss"),
                Location = t.Location,
                Course = t.Course,
                Reason = t.Reason,
                ProviderName = t.Provider,
                Notes = t.Notes,
            });
            
            await using var writer = new StreamWriter(reportLocation);
            await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
            await csv.WriteRecordsAsync(tutorData);
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while writing tutoring data to Croa file");

            Environment.Exit(1);
        }
    }

    private async Task WriteTutoringDataToCsv(List<ColleagueTutoringSessionData> tutorData)
    {
        var reportLocation = _options.ExportLocation + @"\Tutor_Data.csv";

        await using var writer = new StreamWriter(reportLocation);
        await using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        await csv.WriteRecordsAsync(tutorData);
    }

    private async Task<List<ColleagueTutoringSessionData>?> GetBoostAttendance(BrainfuseApiWrapper brainFuse,
        IEnumerable<McccCourseMapping> courseMappings,
        ColleagueSqlWrapper colleagueSqlWrapper, string lookupStartDate, string lookupEndDate)
    {
        var boostAttendance = await brainFuse.GetBoostAttendance(lookupStartDate, lookupEndDate);

        if (boostAttendance == null)
        {
            return null;
        }

        var boostAttendanceData = boostAttendance.Select(e =>
            new ColleagueTutoringSessionData(e.SisId, $"boost_attendance_{e.EventId}_{e.SisId}", "Brainfuse")
            {
                SessionType = ColleagueTutoringSessionData.SessionTypes.BoostAttendance,
                StartDate = e.StartDate,
                EndDate = e.EndDate,
                Location = e.Location,
                Subject = e.SubjectName,
                CoursesForSubject = courseMappings.Where(cm => cm.BrainfuseSubject.Trim() == e.SubjectName?.Trim())
                    .Select(cm => cm.McccCourse).ToArray(),
                Reason = Regex.Replace(e.Reason, @"\s+", " ")
                    .Replace("Focus/reason for appointment(copy and paste assignment instructions if applicable): ",
                        ""),
                Provider = e.Provider,
                Attended = e.Attended,
                Notes = "Attended: " + (e.Attended ? "Yes" : "No")
            }).ToList();

        return boostAttendanceData;
    }

    private static List<McccCourseMapping> GetCourseMappings(string fileName)
    {
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        var records = new List<McccCourseMapping>();

        // read the excel file with the fileName using EPPLus
        using var package = new ExcelPackage(new FileInfo(fileName));
        var worksheet = package.Workbook.Worksheets[0];
        var rowCount = worksheet.Dimension.Rows;

        // start at 2 to skip the header, EPPlus is 1 indexed
        for (var row = 2; row <= rowCount; row++)
        {
            // if either data column is null, skip the incomplete record
            if (string.IsNullOrEmpty(worksheet.Cells[row, 2].Text) ||
                string.IsNullOrEmpty(worksheet.Cells[row, 3].Text))
            {
                continue;
            }

            records.Add(new McccCourseMapping(worksheet.Cells[row, 2].Text, worksheet.Cells[row, 3].Text));
        }

        return records;
    }

    private async Task WriteTutoringDataToCroa(List<ColleagueTutoringSessionData> tutoringSessionData)
    {
        DefaultTypeMap.MatchNamesWithUnderscores = true;
        await using var connection = new SqlConnection(_options.ConnectionStrings.Croa);

        foreach (var record in tutoringSessionData)
        {
            if (record.IntegrationId.Length > 50)
                _logger.LogInformation("IntegrationId record is too long: {IntegrationId}", record.IntegrationId);
            if (record.Source.Length > 50) _logger.LogInformation("Source record is too long: {Source}", record.Source);
            if (record.StudentId.Length > 10)
                _logger.LogInformation("StudentId record is too long: {StudentId}", record.StudentId);
            if (record.Location?.Length > 50)
                _logger.LogInformation("Location record is too long: {Location}", record.Location);
            if (record.Course?.Length > 200) _logger.LogDebug("Course record is too long: {Course}", record.Course);
            if (record.Reason?.Length > 1024)
            {
                _logger.LogInformation("Reason record is too long: {Reason}", record.Reason);
            }

            if (record.Provider?.Length > 200)
                _logger.LogInformation("Provider record is too long: {Provider}", record.Provider);
        }

        const string query = """
                             MERGE M45_STARFISH_BRAINFUSE_MEETINGS AS target
                             USING (VALUES (@IntegrationId, @Source, @StudentId, @StartDate, @EndDate, @Location, @Subject, @Course, @Reason, @Provider, @Notes, @Attended, @SessionType)) AS source (integration_id, source, student_id, start_dt, end_dt, location, subject, course, reason, provider_name, notes, attended, session_type)
                             ON (target.integration_id = source.integration_id)
                             WHEN MATCHED THEN
                                 UPDATE SET Course = source.Course, Attended = source.Attended
                             WHEN NOT MATCHED THEN
                                 INSERT 
                                 (integration_id, source, student_id, start_dt, end_dt, location, subject, course, reason, provider_name, notes, attended, session_type) 
                                 VALUES 
                                 (@IntegrationId, @Source, @StudentId, @StartDate, @EndDate, @Location, @Subject, @Course, @Reason, @Provider, @Notes, @Attended, @SessionType);
                             """;
        try
        {
            await connection.ExecuteAsync(query, tutoringSessionData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while writing tutoring data to Croa file");

            Environment.Exit(1);
        }
    }
}