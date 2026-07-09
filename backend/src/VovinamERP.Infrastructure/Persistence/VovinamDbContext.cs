using Microsoft.EntityFrameworkCore;
using VovinamERP.Application.Common.Interfaces;
using VovinamERP.Domain.Belts;
using VovinamERP.Domain.Finance;
using VovinamERP.Domain.Guardians;
using VovinamERP.Domain.Instructors;
using VovinamERP.Domain.Organizations;
using VovinamERP.Domain.Persons;
using VovinamERP.Domain.Students;
using VovinamERP.Domain.Tenants;
using VovinamERP.Domain.Training;

namespace VovinamERP.Infrastructure.Persistence;

public sealed class VovinamDbContext : DbContext, IUnitOfWork
{
    public VovinamDbContext(DbContextOptions<VovinamDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Person> Persons => Set<Person>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Guardian> Guardians => Set<Guardian>();
    public DbSet<StudentGuardian> StudentGuardians => Set<StudentGuardian>();
    public DbSet<BeltRank> BeltRanks => Set<BeltRank>();
    public DbSet<TrainingClass> TrainingClasses => Set<TrainingClass>();
    public DbSet<TrainingSession> TrainingSessions => Set<TrainingSession>();
    public DbSet<StudentClassEnrollment> StudentClassEnrollments => Set<StudentClassEnrollment>();
    public DbSet<SessionInstructor> SessionInstructors => Set<SessionInstructor>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<AttendanceDetail> AttendanceDetails => Set<AttendanceDetail>();
    public DbSet<TuitionInvoice> TuitionInvoices => Set<TuitionInvoice>();
    public DbSet<TuitionPayment> TuitionPayments => Set<TuitionPayment>();
    public DbSet<Receipt> Receipts => Set<Receipt>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VovinamDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
