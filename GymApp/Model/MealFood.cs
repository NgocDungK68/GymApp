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
        public double calories { get; set; }

        [Column("meal_id")]

        public int MealId { get; set; }
        public Meal Meal { get; set; }

        [Column("food_id")]

        public int FoodId { get; set; }
        public Food Food { get; set; }
    }
}
