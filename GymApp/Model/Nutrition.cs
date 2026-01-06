using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    public class Nutrition
    {
        [Key]
        public int id { get; set; }

        public string note { get; set; }
        public string goal_macros { get; set; }
        public DateTime date { get; set; }

        public int user_id { get; set; }
        public User User { get; set; }

        public ICollection<Meal> Meals { get; set; }
    }
}
