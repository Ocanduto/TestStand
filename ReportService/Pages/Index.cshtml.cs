using Data;

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using ReportService.DataContexts;

namespace ReportService.Pages;

public class IndexModel(ILogger<IndexModel> logger, KreativDBContext context) : PageModel
{
    public CollectionViewModel<TestViewModel> Data { get; set; }

    public async Task OnGetAsync(int pageIndex = 1)
    {
        int pageSize = 15;

        var source = context.TestCollection
            .AsNoTracking();

        var AllSteps = context.StepsCollection
                .AsNoTracking()
            .ToList();

        var count = await source
            .CountAsync();

        var items = await source
            .OrderByDescending(q => q.CreateDate)
            .Skip((pageIndex - 1) * pageSize)
            .Take(pageSize)
            .Select(q => new TestViewModel
            {
                Id = q.Id,
                CreateDate = q.CreateDate,
                SerialNumberBridge = q.SerialNumberBridge,
                Operator = q.Operator,
                BridgeType = q.BridgeType,
                State = 0
            })
            .ToListAsync();

        var StepNumbers = new int[]{ 11, 1, 20, 2, 30, 3, 4, 5, 6 };

        foreach (var item in items)
        {
            var StepList = new List<Step>();

            for (int i = 0; i < 9; i++)
            {
                var Step = new Step
                {
                    StepNumber = StepNumbers[i],
                    Status = false,
                    Have = false
                };
                StepList.Add(Step);
            }
            var List = new List<int>();

            var steps = AllSteps.Where(q => q.TestId == item.Id)
                            .ToList();

            foreach (var step in steps)
            {
                var CurrentStep = StepList.FirstOrDefault(q => q.StepNumber == step.StepType);
                if (CurrentStep!=null)
                {
                    //false => true = true/false?
                    CurrentStep.Status = step.Result;

                    CurrentStep.Have = true;
                }
            }

            if (!StepList.Any(q => q.Have == false) && !StepList.Any(q => q.Status == false))
            {
                item.State = 1;
            }
        }

        Data = new CollectionViewModel<TestViewModel>(items, count, pageIndex, pageSize);
    }
}

public class TestViewModel
{
    public int Id { get; set; }
    public string SerialNumberBridge { get; set; }
    public string BridgeType { get; set; }
    public string Operator { get; set; }
    public int State { get; set; }
    public DateTime CreateDate { get; set; }
}

public class Step
{
    public int StepNumber { get; set; }

    public bool Status { get; set; }

    public bool Have { get; set; }
}