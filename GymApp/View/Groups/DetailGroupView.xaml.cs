using GymApp.Data;
using GymApp.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;

namespace GymApp.View.Groups
{
    public partial class DetailGroupView : UserControl
    {
        public int GroupId { get; private set; }

        // Collection để binding vào ItemsControl
        public ObservableCollection<ChallengeItem> Challenges { get; set; }

        // Tên nhóm
        public string GroupName { get; set; }

        public DetailGroupView(int groupId)
        {
            InitializeComponent();

            GroupId = groupId;
            Challenges = new ObservableCollection<ChallengeItem>();
            this.DataContext = this;

            LoadGroupName();
            LoadChallenges();
        }

        // ================= LOAD GROUP NAME =================
        private void LoadGroupName()
        {
            using var context = new GymDbContext();

            var group = context.Groups
                               .AsNoTracking()
                               .FirstOrDefault(g => g.id == GroupId);

            if (group != null)
            {
                GroupName = group.name;
            }
            else
            {
                GroupName = $"Nhóm #{GroupId}";
            }
        }

        // ================= LOAD CHALLENGES =================
        private void LoadChallenges()
        {
            using var context = new GymDbContext();

            var challenges = context.Challenges
                                    .AsNoTracking()
                                    .Where(c => c.GroupId == GroupId)
                                    .OrderBy(c => c.start_date)
                                    .ToList();

            Challenges.Clear();

            // Nếu DB chưa có, tạo dữ liệu test
            if (!challenges.Any())
            {
                for (int i = 1; i <= 6; i++)
                {
                    Challenges.Add(new ChallengeItem
                    {
                        id = i,
                        name = $"Thử thách {i} nhóm {GroupId}",
                        description = $"Mô tả thử thách {i}, nội dung chi tiết...",
                        challenge_type = i % 2 == 0 ? "Tăng cơ" : "Giảm mỡ",
                        challenge_target = i % 2 == 0 ? 20 : 5,
                        start_date = DateTime.Today.AddDays(-i),
                        end_date = DateTime.Today.AddDays(i + 3)
                    });
                }
            }
            else
            {
                foreach (var c in challenges)
                {
                    Challenges.Add(new ChallengeItem
                    {
                        id = c.id,
                        name = c.name,
                        description = c.description,
                        challenge_type = c.challenge_type,
                        challenge_target = c.challenge_target,
                        start_date = c.start_date,
                        end_date = c.end_date
                    });
                }
            }
        }
    }

    // ================= CHALLENGE ITEM VIEWMODEL =================
    public class ChallengeItem
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string challenge_type { get; set; }
        public double? challenge_target { get; set; }
        public DateTime? start_date { get; set; }
        public DateTime? end_date { get; set; }
    }
}
