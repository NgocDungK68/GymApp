using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("User")]
    public class User
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        [Required]
        public string password { get; set; }

        [Required]
        public int age { get; set; }

        public string? about { get; set; }

        public string? avatar { get; set; }

        [Required]
        public double height { get; set; }

        [Required]
        public double weight { get; set; }

        [Required]
        public string gender { get; set; }

        // Navigation
        public ICollection<Nutrition> Nutritions { get; set; }
        public ICollection<UserRoutine> UserRoutines { get; set; }
        public ICollection<GroupUser> GroupUsers { get; set; }
        public ICollection<Notification> Notifications { get; set; }
        public ICollection<ChallengeUser> ChallengeUsers { get; set; }
    }
}

