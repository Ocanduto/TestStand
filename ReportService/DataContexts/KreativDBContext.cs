using Data;

using Microsoft.EntityFrameworkCore;

namespace ReportService.DataContexts;
public class KreativDBContext(DbContextOptions<KreativDBContext> settings) : DbContext(settings)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder
            .Entity<TestModel>()
            .HasKey(q => q.Id);

        builder
            .Entity<StepModel>()
            .HasKey(q => q.Id);

        builder
            .Entity<TextModel>()
            .HasKey(q => q.Id);
    }

    /// <summary>
    /// Коллекция тестов
    /// </summary>
    public DbSet<TestModel> TestCollection { get; set; }

    /// <summary>
    /// Коллекция шагов
    /// </summary>
    public DbSet<StepModel> StepsCollection { get; set; }

    /// <summary>
    /// Текстовые переменные
    /// </summary>
    public DbSet<TextModel> TextsCollection { get; set; }
}
