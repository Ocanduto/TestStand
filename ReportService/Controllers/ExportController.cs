using ClosedXML.Excel;

using DocumentFormat.OpenXml.Wordprocessing;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using OfficeIMO.Word;

using ReportService.DataContexts;
using ReportService.Pages;

using System.Reflection.Metadata;
using System.Text;

namespace ReportService.Controllers;

[Route("/api/[controller]/[action]")]
public class ExportController(KreativDBContext context, IWebHostEnvironment environments) : ControllerBase
{
    const string docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
    const string xlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [HttpGet("{id}")]
    public async Task<IActionResult> Excel(int id)
    {
        var values = await GetValues(context, id);

        var contents = environments.WebRootFileProvider.GetDirectoryContents("templates");
        var template = contents.SingleOrDefault(q => q.Name.Equals($"template.{nameof(xlsx)}", StringComparison.OrdinalIgnoreCase));

        var stream = new MemoryStream();
        using var targetStream = new MemoryStream();
        using (var sourceStream = System.IO.File.Open(template.PhysicalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            byte[] buffer = new byte[4096]; // 4KB буфер
            int bytesRead;

            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                targetStream.Write(buffer, 0, bytesRead);
            }
        }

        using var workbook = new XLWorkbook(targetStream);

        foreach (var worksheet in workbook.Worksheets)
        {
            foreach (var cell in worksheet.Cells())
            {
                foreach (var value in values)
                {
                    var cellValue = cell.ToString("V");
                    if (cellValue.Contains(@$"{{{{{value.Key}}}}}"))
                    {
                        cellValue = cellValue.Replace(@$"{{{{{value.Key}}}}}", value.Value, StringComparison.OrdinalIgnoreCase);
                        cell.SetValue(cellValue);
                    }
                }
            }
        }

        workbook.SaveAs(stream);
        stream.Position = 0;

        return File(stream, xlsx, $"Report_{id}.{nameof(xlsx)}");
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Word(int id)
    {
        var values = await GetValues(context, id);

        var contents = environments.WebRootFileProvider.GetDirectoryContents("templates");
        var template = contents.SingleOrDefault(q => q.Name.Equals($"template.{nameof(docx)}", StringComparison.OrdinalIgnoreCase));

        var stream = new MemoryStream();
        using var targetStream = new MemoryStream();
        using (var sourceStream = System.IO.File.Open(template.PhysicalPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
        {
            byte[] buffer = new byte[4096]; // 4KB буфер
            int bytesRead;

            while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                targetStream.Write(buffer, 0, bytesRead);
            }
        }

        using var document = WordDocument.Load(targetStream);

        foreach (var value in values)
        {
            document.FindAndReplace(@$"{{{{{value.Key}}}}}", value.Value, StringComparison.OrdinalIgnoreCase);
        }

        document.Save(stream);

        return File(stream, docx, $"Report_{id}.{nameof(docx)}");
    }

    private static async Task<Dictionary<string, string>> GetValues(KreativDBContext context, int id)
    {
        var item = await context.TestCollection
                    .AsNoTracking()
                    .SingleAsync(q => q.Id == id);

        var values = new Dictionary<string, string>
        {
            { nameof(item.Id).ToUpper(), item.Id.ToString() },
            { nameof(item.TestType).ToUpper(), item.TestType },
            { nameof(item.SerialNumberBridge).ToUpper(), item.SerialNumberBridge },
            { nameof(item.SerialNumberHydro).ToUpper(), item.SerialNumberHydro },
            { nameof(item.SerialNumberValve).ToUpper(), item.SerialNumberValve },
            { nameof(item.BrandNumber).ToUpper(), item.BrandNumber },
            { nameof(item.Operator).ToUpper(), item.Operator },
            { "CREATEDATE.DATE", item.CreateDate.ToLocalTime().ToString("dd.MM.yyyy") },
            { "CREATEDATE.TIME", item.CreateDate.ToLocalTime().ToString("HH:mm:ss") }
        };

        var steps = await context.StepsCollection
            .AsNoTracking()
            .Where(q => q.TestId == id && q.Result)
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
            .ToListAsync();

        values.Add("STEP.11", steps.Any(q => q.StepType == 11) ? "Выполнено" : "Не выполнено");
        values.Add("STEP.20", steps.Any(q => q.StepType == 20) ? "Выполнено" : "Не выполнено");
        values.Add("STEP.30", steps.Any(q => q.StepType == 30) ? "Выполнено" : "Не выполнено");

        return values;
    }
}
