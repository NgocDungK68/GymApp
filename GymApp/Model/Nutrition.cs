using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Nutrition")]
    public class Nutrition
    {
        [Key]
        public int id { get; set; }

        public string note { get; set; }
        public double goal_calories { get; set; }
        public DateTime date { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }

        public ICollection<Meal> Meals { get; set; }
    }
}
