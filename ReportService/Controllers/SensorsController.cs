using Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ReportService.DataContexts;

namespace ReportService.Controllers;

[Route("/api/[controller]")]
public class SensorsController(KreativDBContext context) : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> Get([FromQuery] StepsRequestArgs args)
	{
		var skip = (args.Page - 1) * args.Count;

		var items = await context
			.SensorsCollection
			.AsNoTracking()
			//.Where(q => q.TestId == args.TestId)
			.Skip(skip)
			.Take(args.Count)
			.ToListAsync();

		return Ok(items);
	}

	[HttpGet("{id}")]
	public async Task<IActionResult> Get(int id)
	{
		var item = await context
			.SensorsCollection
			.AsNoTracking()
			.FirstOrDefaultAsync(q => q.Id == id);

		return Ok(item);
	}

	[HttpPost]
	public async Task<IActionResult> Create([FromBody] SensorModel model)
	{
		try
		{
			var entity = context.Attach(model);

			entity.State = model.Id > 0 ? EntityState.Modified : EntityState.Added;

			context.SaveChanges();
		}
		catch (Exception ex)
		{

		}

		return Ok(model.Id);
	}
}
