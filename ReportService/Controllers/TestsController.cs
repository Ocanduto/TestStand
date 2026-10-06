using Data;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using ReportService.DataContexts;

namespace ReportService.Controllers;

[Route("/api/[controller]")]
public class TestsController(KreativDBContext context) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] RequestArgs args)
    {
        var skip = (args.Page - 1) * args.Count;

        var items = await context
            .TestCollection
            .AsNoTracking()
            .Skip(skip)
            .Take(args.Count)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await context
            .TestCollection
            .AsNoTracking()
            .FirstOrDefaultAsync(q => q.Id == id);

        return Ok(item);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TestModel model)
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
