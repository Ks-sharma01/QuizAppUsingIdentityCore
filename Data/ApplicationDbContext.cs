using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QuizApplication.Models;
using QuizApplication.ViewModels;
using System.Reflection.Emit;

namespace QuizApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Question> Questions { get; set; }
        public DbSet<Answer> Answers { get; set; }
        //public DbSet<User> Users { get; set; }
        public DbSet<Result> Results { get; set; }
        public DbSet<Category> Category { get; set; }
        public DbSet<UserAnswer> UserAnswer { get; set; }
        public DbSet<AttemptedQuestion> AttemptedQuestions { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<QuestionAnswerDto>().HasNoKey();
            modelBuilder.Entity<Question>()
                .HasMany(q => q.Answers)
                .WithOne(a => a.Questions)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<AttemptedQuestion>()
       .HasOne(aq => aq.User)
       .WithMany()
       .HasForeignKey(aq => aq.Id)
       .OnDelete(DeleteBehavior.Restrict); // Prevents cascade path issue

            modelBuilder.Entity<AttemptedQuestion>()
                .HasOne(aq => aq.Question)
                .WithMany()
                .HasForeignKey(aq => aq.QuestionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UserAnswer>()
            .HasOne(ua => ua.Category)
            .WithMany()
            .HasForeignKey(ua => ua.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // Prevents cascade path issue

            modelBuilder.Entity<UserAnswer>()
    .HasOne(ua => ua.Question)
    .WithMany()
    .HasForeignKey(ua => ua.QuestionId)
    .OnDelete(DeleteBehavior.Restrict); // Prevents cascade path issue
        }
    } 
}
