using Microsoft.EntityFrameworkCore;
using RaceDay.Models;
using Route = RaceDay.Models.Route;

namespace RaceDay.Data
{
    public class RaceDayDbContext : DbContext
    {
        public RaceDayDbContext(DbContextOptions<RaceDayDbContext> options) : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Participant> Participants => Set<Participant>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Route> Routes => Set<Route>();
        public DbSet<Weather> Weather { get; set; }
        public DbSet<Weather> WeatherForecasts => Set<Weather>();
        public DbSet<Registration> Registrations => Set<Registration>();
        public DbSet<Result> Results => Set<Result>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Weather>().ToTable("Weather");

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organiser)
                .WithMany(u => u.OrganisedEvents)
                .HasForeignKey(e => e.OrganiserID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Participant>()
                .HasOne(p => p.User)
                .WithOne(u => u.Participant)
                .HasForeignKey<Participant>(p => p.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Participant)
                .WithMany(p => p.Registrations)
                .HasForeignKey(r => r.ParticipantID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Category)
                .WithMany(c => c.Registrations)
                .HasForeignKey(r => r.CategoryID)
                .OnDelete(DeleteBehavior.Restrict);

            // A participant can only enter a given category once
            modelBuilder.Entity<Registration>()
                .HasIndex(r => new { r.ParticipantID, r.CategoryID }).IsUnique();

            modelBuilder.Entity<Result>()
                .HasOne(r => r.Registration)
                .WithOne(g => g.Result)
                .HasForeignKey<Result>(r => r.RegistrationID);
        }
    }
}