using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    public class Food
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string name { get; set; }

        public string serving_unit { get; set; }
        public double? serving_size { get; set; }

        public string main_macros { get; set; }
        public string other_macros { get; set; }
        public string micronutrients { get; set; }

        public int owner_id { get; set; }    

        [Required]
        public bool is_system { get; set; } = true;

        public ICollection<MealFood> MealFoods { get; set; }
    }
}
