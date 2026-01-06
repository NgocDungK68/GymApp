using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    public class Meal
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string meal_type { get; set; }
        public string description { get; set; }
        public string total_macros { get; set; }
        public DateTime? date { get; set; }

        public int nutrition_id { get; set; }
        public Nutrition Nutrition { get; set; }

        public ICollection<MealFood> MealFoods { get; set; }
    }
}
