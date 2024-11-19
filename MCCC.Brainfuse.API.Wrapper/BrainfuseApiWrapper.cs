using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Text.Json;
using MCCC.Brainfuse.API.Wrapper.Models;

namespace MCCC.Brainfuse.API.Wrapper;

public class BrainfuseApiWrapper
{
    private readonly string _apiKey;
    private readonly HttpClient _httpClient;
    private readonly int _threadCount;
    private readonly int _pageSize;
    private const string AuthenticationEndPoint = "token";

    private const string LiveHelpEndPoint =
        "account/10148/reports/usersDetails/liveHelp?startDate={0}&endDate={1}&itemsPerPage={2}&page={3}&includeContent=true";

    private const string BoostAttendanceEndPoint =
        "account/10148/reports/boost/attendance?startDate={0}&endDate={1}&itemsPerPage={2}&page={3}&includeContent=true";

    private const string WritingLabEndPoint =
        "account/10148/reports/usersDetails/writingLab?startDate={0}&endDate={1}&itemsPerPage={2}&page={3}&includeContent=true";

    private const string EventInfoEndPoint =
        "https://www.brainfuse.com/jsp/calendar/json/e?id={0}&viewStyle=json&include=attendance&w=s";

    /// <summary>
    /// Construct a Brainfuse API Wrapper
    /// </summary>
    /// <param name="baseUrl"></param>
    /// <param name="apiKey"></param>
    /// <param name="useFiddler">Attach a proxy on 127.0.0.1:8888 when true</param>
    /// <param name="threadCount">Defaults to 8</param>
    /// <param name="pageSize">Defaults to 25</param>
    public BrainfuseApiWrapper(string baseUrl, string apiKey, bool useFiddler, int threadCount = 8, int pageSize = 25)
    {
        _apiKey = apiKey;

        _threadCount = threadCount;
        _pageSize = pageSize;

        // Create an HttpClientHandler and set the proxy
        var handler = new HttpClientHandler
        {
            Proxy = new WebProxy("http://127.0.0.1:8888"),
            UseProxy = useFiddler
        };

        _httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
    }

    private async Task EnsureLogin()
    {
        if (_httpClient.DefaultRequestHeaders.Authorization == null)
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("apiKey", _apiKey)
            });
            var response = await _httpClient.PostAsync(AuthenticationEndPoint, content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                // parse responseString as a JSON object and store the token parameter
                var jsonResponse = JsonSerializer.Deserialize<BrainfuseTokenResponse>(responseString);

                if (jsonResponse != null)
                    _httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(jsonResponse.Token ??
                                                                              throw new InvalidOperationException(
                                                                                  "Authentication failed"));
            }
        }
        // IDK, check the token?
    }

    public async Task<List<BrainfuseLiveSessionElement>?> GetLiveSessions(string searchStartDate,
        string? searchEndDate = null)
    {
        await EnsureLogin();

        return await GetData<BrainfuseLiveSessionElement>(LiveHelpEndPoint, "LiveSession", searchStartDate,
            searchEndDate);
    }

    public async Task<List<BrainfuseEvent>?> GetBoostAttendance(string searchStartDate, string? searchEndDate = null)
    {
        await EnsureLogin();

        var events = await GetData<BrainfuseEvent>(BoostAttendanceEndPoint, "events", searchStartDate, searchEndDate);

        events = events?.DistinctBy(e => e.EventId).ToList();

        //events.ForEach(async e => e.EventInfo = await GetEventInfo(e.EventId));

        if (events == null) return null;

        var throttler = new SemaphoreSlim(_threadCount);
        var tasks = new List<Task>();
        var returnData = new BlockingCollection<BrainfuseEvent>();

        foreach (var e in events)
        {
            await throttler.WaitAsync();
            tasks.Add(
                Task.Run(
                    async () =>
                    {
                        try
                        {
                            var eventInfo = await GetEventInfo(e.EventId);
                            if (eventInfo != null)
                            {
                                e.EndDate = eventInfo.End;
                                e.Location = eventInfo.Attributes?.FirstOrDefault(a => a.Name == "Location")
                                    ?.DisplayValue;
                                e.LocationValue = eventInfo.Attributes?.FirstOrDefault(a => a.Name == "Location_Value")
                                    ?.DisplayValue;
                                e.Reason = eventInfo.Notes;
                                e.Provider =
                                    eventInfo.BrainfuseModerator?.Username != null &&
                                    eventInfo.BrainfuseModerator.Username.EndsWith("@mc3.edu")
                                        ? "MCCC"
                                        : "BrainFuse";
                                var attended = eventInfo.AttendeeList?.FirstOrDefault(a => a.ModeratorModerator == 0)
                                    ?.Attendance;
                                e.Attended = attended is "ATTENDED";
                            }

                            // null location means a walk-up appointment, which means attendance is always true
                            if (e.LocationValue == null)
                            {
                                e.Location = "In Person";
                                e.Attended = true;
                            }

                            returnData.Add(e);
                        }
                        catch (Exception exception)
                        {
                            Console.WriteLine(exception);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }
                )
            );
        }
        
        await Task.WhenAll(tasks);

        return returnData.ToList();
    }

    public async Task<List<BrainfuseWritingLab>?> GetWritingLabAttendance(string searchStartDate,
        string? searchEndDate = null)
    {
        await EnsureLogin();
        return await GetData<BrainfuseWritingLab>(WritingLabEndPoint, "WritingLab", searchStartDate, searchEndDate);
    }

    private async Task<BrainfuseEventInfo?> GetEventInfo(long eventId)
    {
        await EnsureLogin();
        var initialEndPoint = string.Format(EventInfoEndPoint, eventId);
        var data = await _httpClient.GetAsync(initialEndPoint);
        data.EnsureSuccessStatusCode();
        var responseText = await data.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<BrainfuseEventInfo>(responseText);
    }

    private async Task<List<T>?> GetData<T>(string endPointTemplate, string dynamicPropertyName, string searchStartDate,
        string? searchEndDate = null)
    {
        if (searchEndDate == null)
        {
            searchEndDate = searchStartDate;
        }
        
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new BrainfuseDynamicParameterNamingPolicy(dynamicPropertyName),
            WriteIndented = true
        };

        var initialEndPoint = string.Format(endPointTemplate, searchStartDate, searchEndDate, 1, 1);
        Console.WriteLine(
            $"Searching {initialEndPoint} to {dynamicPropertyName} for {searchStartDate} to {searchEndDate}");
        var initialResponse = await _httpClient.GetAsync(initialEndPoint);

        if (initialResponse.IsSuccessStatusCode)
        {
            var initialResponseString = await initialResponse.Content.ReadAsStringAsync();

            var data = JsonSerializer.Deserialize<BrainfuseApiCall<T>>(initialResponseString);

            var numberOfResults = data?.BrainfusePages?.NumOfPages;
            var sessionId = data?.BrainfusePages?.SessionId;

            var apiCalls = new List<string>();

            for (var i = 0; i < numberOfResults; i += _pageSize)
            {
                apiCalls.Add(string.Format(endPointTemplate, searchStartDate, searchEndDate, _pageSize,
                    i / _pageSize + 1) + $"&__sessionId={sessionId}");
            }

            var throttler = new SemaphoreSlim(_threadCount);
            var allTasks = new List<Task>();
            var returnData = new BlockingCollection<T>();
            foreach (var apiCall in apiCalls)
            {
                await throttler.WaitAsync();
                allTasks.Add(
                    Task.Run(async () =>
                    {
                        try
                        {
                            var responseMessage = await _httpClient.GetAsync(apiCall);
                            responseMessage.EnsureSuccessStatusCode();
                            var responseText = await responseMessage.Content.ReadAsStringAsync();
                            //responseText.Dump();
                            var response = JsonSerializer.Deserialize<BrainfuseApiCall<T>>(responseText, options);

                            if (response?.Data != null)
                            {
                                foreach (var item in response.Data)
                                {
                                    returnData.Add(item);
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                        finally
                        {
                            throttler.Release();
                        }
                    }));
            }

            await Task.WhenAll(allTasks);

            return returnData.ToList();
        }

        Console.WriteLine($"Error: {initialResponse.StatusCode}");
        return null;
    }
}