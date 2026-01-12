using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Workout")]
    public class Workout
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public int duration { get; set; }
        public string note { get; set; }
        public DateTime? date { get; set; }
        public double? calories_burned { get; set; }

        [Column("routine_id")]
        public int RoutineId { get; set; }

        [Column("nutrition_id")]
        public int NutritionId { get; set; }
        public Routine Routine { get; set; }
        public Nutrition Nutrition { get; set; }
    }
}
