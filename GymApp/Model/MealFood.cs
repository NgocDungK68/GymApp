using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Meal_Food")]
    public class MealFood
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string macros { get; set; }

        public int meal_id { get; set; }
        public Meal Meal { get; set; }

        public int food_id { get; set; }
        public Food Food { get; set; }
    }
}
