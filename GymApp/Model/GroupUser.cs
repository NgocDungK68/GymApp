using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GymApp.Model
{
    [Table("Group_User")]
    public class GroupUser
    {
        [Key]
        public int id { get; set; }

        public string role_in_group { get; set; }
        public DateTime joined_at { get; set; }

        [Column("group_id")]
        public int GroupId { get; set; }
        public Group Group { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
