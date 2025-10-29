namespace Domain.DTO;

public class EmailTemplateDto : AuditableEntityDto
{
    public string TemplateName { get; set; }
    public string Channel { get; set; }
    public string Template { get; set; }
    public CancellationToken CancellationToken { get; set; }
}

