using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GymApp.View.Routines
{
    public partial class RoutineManagementView : UserControl
    {
        public ObservableCollection<RoutineRow> Routines { get; set; }
            = new ObservableCollection<RoutineRow>();

        private ICollectionView _routineView;

        public RoutineManagementView()
        {
            InitializeComponent();

            _routineView = CollectionViewSource.GetDefaultView(Routines);
            dgRoutines.ItemsSource = _routineView;

            LoadRoutines();
        }

        // ================= LOAD DATA =================
        private void LoadRoutines()
        {
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem danh sách lịch tập");
                return;
            }

            var list = GetRoutinesFromDb();

            Routines.Clear();
            int index = 1;

            foreach (var r in list)
            {
                r.Index = index++;
                Routines.Add(r);
            }

            _routineView.Refresh();
            txtTotal.Text = Routines.Count.ToString();
        }

        // ================= CLICK TÊN LỊCH TẬP =================
        private void RoutineName_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is RoutineRow row)
            {
                MessageBox.Show(
                    $"Mở chi tiết lịch tập:\n\n{row.Name}",
                    "Routine Detail"
                );

                // Sau này:
                /*
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow == null) return;

                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(
                    new RoutineDetailView(row.Id)
                );
                */
            }
        }

        // ================= SEARCH =================
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_routineView == null) return;

            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                _routineView.Filter = null;
            }
            else
            {
                _routineView.Filter = obj =>
                {
                    if (obj is RoutineRow row && !string.IsNullOrEmpty(row.Name))
                    {
                        return row.Name.ToLower().StartsWith(keyword);
                    }
                    return false;
                };
            }

            _routineView.Refresh();
            txtTotal.Text = _routineView.Cast<object>().Count().ToString();
        }

        // ================= ADD NEW =================
        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Bạn chưa đăng nhập!",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var addWindow = new AddRoutineView();
            addWindow.Owner = Window.GetWindow(this);

            // mở dạng modal
            addWindow.ShowDialog();

            // reload lại danh sách sau khi đóng window
            LoadRoutines();
        }


        // ================= DB =================
        private List<RoutineRow> GetRoutinesFromDb()
        {
            int currentUserId = UserSession.CurrentUser!.id;

            using var context = new GymDbContext();

            // 1️⃣ Lấy User_Routine của user đang đăng nhập
            var userRoutines = context.UserRoutines
                .Where(ur => ur.user_id == currentUserId)
                .Select(ur => new
                {
                    ur.routine_id,
                    ur.owner_id
                })
                .ToList();

            var result = userRoutines.Select(ur =>
            {
                // 2️⃣ Routine
                var routine = context.Routines
                    .First(r => r.id == ur.routine_id);

                // 3️⃣ Người tạo (owner)
                string createdBy = "";
                if (ur.owner_id.HasValue)
                {
                    createdBy = context.Users
                        .Where(u => u.id == ur.owner_id.Value)
                        .Select(u => u.name)
                        .FirstOrDefault() ?? "";
                }

                // 4️⃣ Tổng số sets
                int totalSets = context.RoutineExercises
                    .Where(re => re.routine_id == routine.id)
                    .Sum(re => (int?)re.sets) ?? 0;

                return new RoutineRow
                {
                    Id = routine.id,
                    Name = routine.name,
                    Level = routine.level,
                    TotalSets = totalSets,
                    CreatedBy = createdBy
                };
            })
            .OrderBy(r => r.Name)
            .ToList();

            return result;
        }
    }

    // ================= ROW MODEL =================
    public class RoutineRow
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Level { get; set; }
        public int TotalSets { get; set; }
        public string CreatedBy { get; set; }
    }
}
