namespace Data;

[PathModel("tests")]
public class TestModel
{
    public int Id { get; set; }
    public string TestType { get; set; }
    public string SerialNumberBridge { get; set; }
    public string SerialNumberHydro { get; set; }
    public string SerialNumberValve { get; set; }
    public string BrandNumber { get; set; }
    public string Operator { get; set; }
    public DateTime CreateDate { get; set; }
}
