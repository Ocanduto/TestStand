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

		builder.Entity<StepModel>()
	       .HasOne(o => o.Test)
	       .WithMany(c => c.Steps)
	       .HasForeignKey(o => o.TestId)
	       .OnDelete(DeleteBehavior.Restrict);

		builder
            .Entity<TextModel>()
            .HasKey(q => q.Id);

		builder
			.Entity<SensorModel>()
			.HasKey(q => q.Id);

		builder.Entity<SensorModel>()
			   .HasOne(s => s.Step)               
			   .WithMany(st => st.Sensors)         
			   .HasForeignKey(s => s.StepId)       
			   .OnDelete(DeleteBehavior.Restrict); 
	}

    /// <summary>
    /// Коллекция тестов
    /// </summary>
    public DbSet<TestModel> TestCollection { get; set; }

    /// <summary>
    /// Коллекция шагов
    /// </summary>
    public DbSet<StepModel> StepsCollection { get; set; }

	public DbSet<SensorModel> SensorsCollection { get; set; }

	/// <summary>
	/// Текстовые переменные
	/// </summary>
	public DbSet<TextModel> TextsCollection { get; set; }
}
