using Data;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

using ReportService.DataContexts;

namespace ReportService.Pages;

public class ReportModel(KreativDBContext context) : PageModel
{
    public int Id { get; set; }
    public TestModel? Item { get; set; }
    public CollectionViewModel<StepViewModel> Data { get; set; }

    public async Task OnGetAsync(int id, int pageIndex = 1)
    {
        Id = id;
        Item = await context.TestCollection.FirstOrDefaultAsync(q => q.Id == id);

        int pageSize = 10;

        var source = context.StepsCollection
            .AsNoTracking()
            .Where(q => q.TestId == id);

        var count = await source
            .CountAsync();

        var items = await source
            .OrderBy(q => q.CreateDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Join(context.TextsCollection,
            a => a.StepType,
            b => b.Id,
            (a, b) => new StepViewModel
            {
                Id = a.Id,
                TestId = a.TestId,
                CreateDate = a.CreateDate,
                Info = a.Info,
                Result = a.Result,
                StepType = a.StepType,
                StepText = b.Text
            })
            .Where(q => q.StepType == 1 || q.StepType == 20 || q.StepType == 30 || q.StepType == 4 || q.StepType == 6 || q.Info == "Авария")
            .ToListAsync();

        Data = new CollectionViewModel<StepViewModel>(items, count, pageIndex, pageSize);
    }
}

public class StepViewModel
{
    public int Id { get; set; }
    public int TestId { get; set; }
    public int StepType { get; set; }
    public string StepText { get; set; }
    public bool Result { get; set; }
    public string? Info { get; set; }
    public DateTime CreateDate { get; set; }
}