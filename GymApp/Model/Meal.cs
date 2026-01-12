using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Meal")]
    public class Meal
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string meal_type { get; set; }
        public string description { get; set; }
        public double total_calories { get; set; }
        public DateTime? date { get; set; }

        [Column("nutrition_id")]
        public int NutritionId { get; set; }
        public Nutrition Nutrition { get; set; }

        public ICollection<MealFood> MealFoods { get; set; }
    }
}
