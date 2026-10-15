namespace Data;

[PathModel("sensors")]
public class SensorModel
{
	public int Id { get; set; }
	public int StepId { get; set; }
	public string SensorName { get; set; }
	public double Value { get; set; }
	public DateTime RecordDate { get; set; }
	public DateTime CreateDate { get; set; }
	public StepModel Step { get; set; }
}