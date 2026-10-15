namespace Data;

[PathModel("tests")]
public class TestModel
{
    public int Id { get; set; }
    public string BridgeType { get; set; }
    public string SerialNumberBridge { get; set; }
    public string Operator { get; set; }
    public DateTime CreateDate { get; set; }
	public ICollection<StepModel> Steps { get; set; } = new List<StepModel>();
}
