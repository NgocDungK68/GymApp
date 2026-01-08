using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using GymApp.View.Routines;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GymApp.View.Exercises
{
    public partial class DetailExerciseView : UserControl
    {
        private readonly int _exerciseId;
        private Exercise? _exercise;

        // Lưu dữ liệu gốc để so sánh thay đổi
        private string _originalName = "";
        private string _originalMuscles = "";
        private string _originalDescription = "";

        private bool _isEditing = false;

        public event Action<int>? ExerciseSelected;

        public DetailExerciseView(int exerciseId)
        {
            InitializeComponent();
            _exerciseId = exerciseId;

            LoadExercise();
            SetReadOnlyMode();
        }

        // ================= LOAD DATA =================
        private void LoadExercise()
        {
            using var context = new GymDbContext();

            _exercise = context.Exercises
                               .AsNoTracking()
                               .FirstOrDefault(e => e.id == _exerciseId);

            if (_exercise == null)
            {
                MessageBox.Show("Không tìm thấy bài tập");
                return;
            }

            txtExerciseName.Text = _exercise.name;
            txtMuscles.Text = _exercise.muscles;
            txtDescription.Text = _exercise.about;
            txtCreatedBy.Text = _exercise.is_system ? "System" : "Bạn";

            // Lưu bản gốc
            _originalName = _exercise.name;
            _originalMuscles = _exercise.muscles;
            _originalDescription = _exercise.about;
        }

        // ================= BUTTON EVENTS =================

        // Tạo lịch tập
        private void BtnCreateSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (_exercise == null) return;

            var hostWindow = Window.GetWindow(this);

            // ===============================
            // CASE 1: Đang nằm trong Window trung gian (ShowDialog)
            // ===============================
            if (hostWindow != null && hostWindow.Owner != null)
            {
                // bắn event trả exerciseId
                ExerciseSelected?.Invoke(_exerciseId);

                // đóng window trung gian
                hostWindow.DialogResult = true;
                hostWindow.Close();
                return;
            }

            // ===============================
            // CASE 2: Đang nằm trong MainWindow
            // ===============================
            if (hostWindow is MainWindow mainWindow)
            {
                var addWindow = new AddRoutineView(_exerciseId);
                addWindow.Owner = Window.GetWindow(this);

                // mở dạng modal
                addWindow.ShowDialog();
            }
        }


        // SỬA
        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!IsOwner())
            {
                MessageBox.Show("Bạn không có quyền chỉnh sửa bài tập này");
                return;
            }

            if (_isEditing) return;

            SetEditMode();
        }

        // XÓA
        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!IsOwner())
            {
                MessageBox.Show("Bạn không có quyền xóa bài tập này");
                return;
            }

            if (MessageBox.Show("Bạn có chắc muốn xóa bài tập này?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                != MessageBoxResult.Yes)
                return;

            using var context = new GymDbContext();
            var ex = context.Exercises.FirstOrDefault(e => e.id == _exerciseId);

            if (ex != null)
            {
                context.Exercises.Remove(ex);
                context.SaveChanges();
            }

            MessageBox.Show("Đã xóa bài tập");

            // Đóng view (tuỳ cách bạn host UserControl)
            this.Visibility = Visibility.Collapsed;
        }

        // LƯU
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newName = txtExerciseName.Text.Trim();
            string newMuscles = txtMuscles.Text.Trim();
            string newDesc = txtDescription.Text.Trim();

            // Validate
            if (string.IsNullOrWhiteSpace(newName) ||
                string.IsNullOrWhiteSpace(newMuscles))
            {
                MessageBox.Show("Tên bài tập và nhóm cơ không được để trống");
                return;
            }

            // Không thay đổi
            if (newName == _originalName &&
                newMuscles == _originalMuscles &&
                newDesc == _originalDescription)
            {
                MessageBox.Show("Dữ liệu không thay đổi");
                return;
            }

            using var context = new GymDbContext();
            var ex = context.Exercises.FirstOrDefault(e => e.id == _exerciseId);

            if (ex == null) return;

            ex.name = newName;
            ex.muscles = newMuscles;
            ex.about = newDesc;

            context.SaveChanges();

            MessageBox.Show("Cập nhật thành công");

            // quay về màn hình danh sách bài tập
            GoBackToList();
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GoBackToList();
        }

        // ================= HELPERS =================

        private bool IsOwner()
        {
            return _exercise != null &&
                   !_exercise.is_system &&
                   _exercise.owner_id == UserSession.CurrentUser?.id;
        }

        private void SetEditMode()
        {
            _isEditing = true;

            txtExerciseName.IsReadOnly = false;
            txtMuscles.IsReadOnly = false;
            txtDescription.IsReadOnly = false;

            btnSave.Visibility = Visibility.Visible;
        }

        private void SetReadOnlyMode()
        {
            _isEditing = false;

            txtExerciseName.IsReadOnly = true;
            txtMuscles.IsReadOnly = true;
            txtDescription.IsReadOnly = true;

            btnSave.Visibility = Visibility.Collapsed;
        }

        private void GoBackToList()
        {
            var hostWindow = Window.GetWindow(this);
            if (hostWindow == null) return;

            // ===============================
            // CASE 1: Đang nằm trong MainWindow
            // ===============================
            if (hostWindow is MainWindow mainWindow)
            {
                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(
                    new ExerciseManagementView()
                );
                return;
            }

            // ===============================
            // CASE 2: Đang nằm trong Window trung gian
            // ===============================
            hostWindow.Content = new ExerciseManagementView();
        }

    }
}
