using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using GymApp.View.Exercises;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GymApp.View
{
    public partial class ExerciseManagementView : UserControl
    {
        public ObservableCollection<ExerciseRow> Exercises { get; set; }
            = new ObservableCollection<ExerciseRow>();
        private ICollectionView _exerciseView;

        public event Action<int>? ExerciseSelected;

        public ExerciseManagementView()
        {
            InitializeComponent();

            // Tạo CollectionView từ Exercises
            _exerciseView = CollectionViewSource.GetDefaultView(Exercises);
            dgExercises.ItemsSource = _exerciseView;

            LoadExercises();
        }

        private void LoadExercises()
        {
            // 🔒 BẮT BUỘC đăng nhập
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem danh sách bài tập");
                return;
            }

            var list = GetExercisesFromDb();

            Exercises.Clear();
            int index = 1;

            foreach (var ex in list.OrderBy(x => x.name))
            {
                Exercises.Add(new ExerciseRow
                {
                    Index = index++,
                    Id = ex.id,
                    Name = ex.name,
                    Muscles = ex.muscles,
                    CreatedBy = ex.is_system ? "System" : "Bạn"
                });
            }

            _exerciseView.Refresh();
            txtTotal.Text = Exercises.Count.ToString();
        }


        // Click tên bài tập
        private void ExerciseName_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is not TextBlock tb || tb.DataContext is not ExerciseRow row)
                return;

            // Window chứa UserControl hiện tại
            var hostWindow = Window.GetWindow(this);

            // ===============================
            // CASE 1: Đang chạy trong MainWindow
            // ===============================
            if (hostWindow is MainWindow mainWindow)
            {
                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(
                    new DetailExerciseView(row.Id)
                );
                return;
            }

            // ===============================
            // CASE 2: Đang chạy trong Window trung gian
            // ===============================
            var detailView = new DetailExerciseView(row.Id);
            detailView.ExerciseSelected += id =>
            {
                ExerciseSelected?.Invoke(id);
            };

            hostWindow.Content = detailView;
        }

        // Nút thêm mới bài tập
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

            var addWindow = new AddExerciseView();
            addWindow.Owner = Window.GetWindow(this);

            // mở dạng modal
            addWindow.ShowDialog();

            // reload lại danh sách sau khi đóng window
            LoadExercises();
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_exerciseView == null) return;

            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                _exerciseView.Filter = null; // hiện tất cả
            }
            else
            {
                _exerciseView.Filter = obj =>
                {
                    if (obj is ExerciseRow row && !string.IsNullOrEmpty(row.Name))
                    {
                        // BẮT ĐẦU BẰNG CHUỖI NHẬP
                        return row.Name.ToLower().StartsWith(keyword);
                    }
                    return false;
                };
            }

            _exerciseView.Refresh();
            txtTotal.Text = _exerciseView.Cast<object>().Count().ToString();
        }

        private List<Exercise> GetExercisesFromDb()
        {
            int currentUserId = UserSession.CurrentUser!.id;

            using var context = new GymDbContext();

            // Lấy toàn bộ bài tập của mình và hệ thống
            return context.Exercises
                .AsNoTracking()
                .Where(e =>
                    e.is_system == true ||
                    e.owner_id == currentUserId
                )
                .ToList();
        }
    }

    public class ExerciseRow
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string Muscles { get; set; }
        public string CreatedBy { get; set; }
    }
}
