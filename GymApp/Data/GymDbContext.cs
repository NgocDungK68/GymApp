using GymApp.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Data
{
    public class GymDbContext : DbContext
    {
        // ===== USER =====
        public DbSet<User> Users { get; set; }

        // ===== NUTRITION =====
        public DbSet<Nutrition> Nutritions { get; set; }
        public DbSet<Meal> Meals { get; set; }
        public DbSet<Food> Foods { get; set; }
        public DbSet<MealFood> MealFoods { get; set; }

        // ===== WORKOUT / ROUTINE =====
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Routine> Routines { get; set; }
        public DbSet<RoutineExercise> RoutineExercises { get; set; }
        public DbSet<UserRoutine> UserRoutines { get; set; }
        public DbSet<Workout> Workouts { get; set; }

        // ===== GROUP =====
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupUser> GroupUsers { get; set; }

        // ===== CHALLENGE =====
        public DbSet<Challenge> Challenges { get; set; }
        public DbSet<ChallengeUser> ChallengeUsers { get; set; }

        // ===== NOTIFICATION =====
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(
                "Server=DESKTOP-Q643FS1\\SQLEXPRESS;Database=GymOnlineDB;Trusted_Connection=True;TrustServerCertificate=True"
            );
        }
    }
}
