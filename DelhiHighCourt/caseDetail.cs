public class caseDetail
{
    public int Id { get; set; } // Keeping Id as non-nullable, usually this is a required primary key.
    public string? Filename { get; set; }
    public string? Court { get; set; }
    public string? Abbr { get; set; }
    public string? CaseNo { get; set; }
    public DateTime? Dated { get; set; } // Making DateTime nullable
    public string? CaseName { get; set; }
    public string? Counsel { get; set; }
    public string? Overrule { get; set; }
    public string? OveruleBy { get; set; }
    public string? Citation { get; set; }
    public string? Coram { get; set; }
    public string? Act { get; set; }
    public string? Bench { get; set; }
    public string? Result { get; set; }
    public string? Headnotes { get; set; }
    public string? CaseReferred { get; set; }
    public string? Ssd { get; set; }
    public bool? Reportable { get; set; } // Making bool nullable
    public string? PdfLink { get; set; }
    public string? Type { get; set; }
    public int? CoramCount { get; set; } // Making int nullable
    public string? Petitioner { get; set; }
    public string? Respondent { get; set; }
    public string? BlaCitation { get; set; }
    public string? QrLink { get; set; }
}