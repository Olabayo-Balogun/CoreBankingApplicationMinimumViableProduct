namespace Application.Models.IndustryField.Response
{
    public class IndustryFieldResponse
    {
        public long Id { get; set; }
        public long IndustryId { get; set; }
        public string Name { get; set; }
        public string DataType { get; set; }
        public bool IsRequired { get; set; }
        public int Order { get; set; }
        public string? ToolTip { get; set; }
    }
}
