namespace PRN232.LMS.Services.Models;

public sealed record SubjectModel(int SubjectId, string SubjectCode, string SubjectName, int Credit);

public sealed record SubjectUpsertModel(string SubjectCode, string SubjectName, int Credit);
