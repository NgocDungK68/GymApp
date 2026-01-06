using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    public class Workout
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public int duration { get; set; }
        public string note { get; set; }
        public DateTime? date { get; set; }
        public double? calories_burned { get; set; }

        public int routine_id { get; set; }
        public Routine Routine { get; set; }
    }
}
