namespace Data;

[PathModel("steps")]
public class StepModel
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public int StepType { get; set; }
    public bool Result { get; set; }
    public string? Info { get; set; }
    public DateTime CreateDate { get; set; }
}

public class TextModel
{
    public int Id { get; set; }
    public string Text { get; set; }
}