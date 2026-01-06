using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Exercise")]
    public class Exercise
    {
        [Key]
        public int id { get; set; }

        public string name { get; set; }
        public string muscles { get; set; }
        public string about { get; set; }

        public int? owner_id { get; set; }

        [Required]
        public bool is_system { get; set; } = true;

        public ICollection<RoutineExercise> RoutineExercises { get; set; }
    }
}
