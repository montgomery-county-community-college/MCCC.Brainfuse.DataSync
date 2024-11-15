namespace MCCC.Brainfuse.DataSync.Models;

public class McccCourseMapping(string mcccCourse, string brainfuseSubject)
{
    public string McccCourse { get; set; } = mcccCourse;
    public string BrainfuseSubject { get; set; } = brainfuseSubject;
}