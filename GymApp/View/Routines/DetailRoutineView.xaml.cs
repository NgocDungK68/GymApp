using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace GymApp.View.Routines
{
    public partial class DetailRoutineView : UserControl
    {
        private readonly int _routineId;
        private ObservableCollection<RoutineExerciseItem> _exerciseList = new();
        private List<RoutineExerciseItem> _originalExercises = new();

        private bool _isEditing = false;

        private string _originalName = "";
        private string _originalDescription = "";
        private string _originalLevel = "";

        public DetailRoutineView(int routineId)
        {
            InitializeComponent();
            _routineId = routineId;

            dgExercises.ItemsSource = _exerciseList;

            LoadRoutine();
        }

        // ================= LOAD =================
        private void LoadRoutine()
        {
            using var context = new GymDbContext();

            // ===== 1. LOAD ROUTINE =====
            var routine = context.Routines
                .AsNoTracking()
                .FirstOrDefault(r => r.id == _routineId);

            if (routine == null)
            {
                MessageBox.Show("Không tìm thấy lịch tập");
                return;
            }

            txtRoutineName.Text = routine.name;
            txtDescription.Text = routine.description;
            txtLevel.Text = routine.level;
            txtLevel.Text = routine.level;
            cbLevel.SelectedItem = cbLevel.Items
                .Cast<ComboBoxItem>()
                .FirstOrDefault(i => i.Content.ToString() == routine.level);


            _originalName = routine.name;
            _originalDescription = routine.description;
            _originalLevel = routine.level;

            // ===== 2. LOAD CREATED BY (USER_ROUTINE → USER) =====
            string createdBy = "System";

            if (UserSession.CurrentUser != null)
            {
                var userRoutine = context.UserRoutines
                    .AsNoTracking()
                    .FirstOrDefault(ur =>
                        ur.RoutineId == _routineId &&
                        ur.UserId == UserSession.CurrentUser.id);

                if (userRoutine?.owner_id != null)
                {
                    var owner = context.Users
                        .AsNoTracking()
                        .FirstOrDefault(u => u.id == userRoutine.owner_id);

                    if (owner != null)
                        createdBy = owner.name;
                }
            }

            txtCreatedBy.Text = createdBy;

            // ===== 3. LOAD ROUTINE EXERCISES =====
            var routineExercises = context.RoutineExercises
                .Include(re => re.Exercise)
                .AsNoTracking()
                .Where(re => re.RoutineId == _routineId)
                .ToList();

            _exerciseList.Clear();
            int index = 1;

            foreach (var re in routineExercises)
            {
                _exerciseList.Add(new RoutineExerciseItem
                {
                    Index = index++,
                    ExerciseId = re.ExerciseId,
                    ExerciseName = re.Exercise?.name ?? "(Không xác định)",
                    Sets = re.sets
                });
            }

            _originalExercises = _exerciseList
                .Select(x => new RoutineExerciseItem
                {
                    ExerciseId = x.ExerciseId,
                    Sets = x.Sets
                })
                .ToList();
        }

        // ================= BUTTONS =================

        private void BtnStartWorkout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Chức năng bắt đầu buổi tập sẽ phát triển sau 💪");
        }

        private void BtnAddExercise_Click(object sender, RoutedEventArgs e)
        {
            int? selectedExerciseId = null;

            var exerciseView = new ExerciseManagementView();
            var hostWindow = Window.GetWindow(this);

            var selectExerciseWindow = new Window
            {
                Title = "Chọn bài tập",
                Width = 800,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = exerciseView,
                Owner = hostWindow
            };

            // lắng nghe khi user bấm "Tạo lịch"
            exerciseView.ExerciseSelected += id =>
            {
                selectedExerciseId = id;
            };

            if (selectExerciseWindow.ShowDialog() == true && selectedExerciseId.HasValue)
            {
                // HỨNG ĐƯỢC exerciseId TẠI ĐÂY
                AddExerciseToListById(selectedExerciseId.Value);
            }
        }   

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            GoBackToList();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!IsOwner())
            {
                MessageBox.Show("Bạn không có quyền chỉnh sửa lịch tập này");
                return;
            }

            if (_isEditing) return;

            SetEditMode();
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!IsOwner())
            {
                MessageBox.Show("Bạn không có quyền xóa lịch tập này");
                return;
            }

            if (MessageBox.Show("Bạn có chắc muốn xóa lịch tập này?",
                "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                != MessageBoxResult.Yes)
                return;

            using var context = new GymDbContext();

            var routine = context.Routines
                .Include(r => r.RoutineExercises)
                .Include(r => r.UserRoutines)
                .FirstOrDefault(r => r.id == _routineId);

            if (routine != null)
            {
                context.RoutineExercises.RemoveRange(routine.RoutineExercises);
                context.UserRoutines.RemoveRange(routine.UserRoutines);
                context.Routines.Remove(routine);
                context.SaveChanges();
            }

            MessageBox.Show("Đã xóa lịch tập");
            GoBackToList();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string newName = txtRoutineName.Text.Trim();
            string newDescription = txtDescription.Text.Trim();
            string newLevel = (cbLevel.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show("Tên lịch tập không được để trống");
                return;
            }

            bool routineInfoChanged =
                newName != _originalName ||
                newDescription != _originalDescription ||
                newLevel != _originalLevel;

            bool exerciseListChanged = HasExerciseListChanged();

            // Không thay đổi
            if (!routineInfoChanged && !exerciseListChanged)
            {
                MessageBox.Show("Dữ liệu không thay đổi");
                return;
            }

            using var context = new GymDbContext();
            var routine = context.Routines
                .Include(r => r.RoutineExercises)
                .FirstOrDefault(r => r.id == _routineId);

            if (routine == null) return;

            // ===== UPDATE ROUTINE =====
            routine.name = newName;
            routine.description = newDescription;
            routine.level = newLevel;

            // ===== UPDATE ROUTINE_EXERCISE =====
            if (exerciseListChanged)
            {
                // Xóa cũ
                context.RoutineExercises.RemoveRange(routine.RoutineExercises);

                // Thêm mới
                foreach (var item in _exerciseList)
                {
                    context.RoutineExercises.Add(new RoutineExercise
                    {
                        RoutineId = _routineId,
                        ExerciseId = item.ExerciseId,
                        sets = item.Sets
                    });
                }
            }

            context.SaveChanges();

            MessageBox.Show("Cập nhật lịch tập thành công");
            GoBackToList();
        }

        // ================= HELPERS =================

        private void GoBackToList()
        {
            var mainWindow = Window.GetWindow(this) as MainWindow;
            if (mainWindow == null) return;

            mainWindow.MainContent.Children.Clear();
            mainWindow.MainContent.Children.Add(
                new RoutineManagementView()
            );
        }

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // chỉ cho nhập số
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        private bool IsOwner()
        {
            if (UserSession.CurrentUser == null) return false;

            using var context = new GymDbContext();

            return context.UserRoutines.Any(ur =>
                ur.RoutineId == _routineId &&
                ur.owner_id == UserSession.CurrentUser.id);
        }

        private void SetEditMode()
        {
            _isEditing = true;

            txtRoutineName.IsReadOnly = false;
            txtDescription.IsReadOnly = false;

            // Level: view → edit
            bdLevelView.Visibility = Visibility.Collapsed;
            cbLevel.Visibility = Visibility.Visible;

            btnSave.Visibility = Visibility.Visible;
            btnAddExercise.Visibility = Visibility.Visible;
        }

        private void AddExerciseToListById(int exerciseId)
        {
            using var context = new GymDbContext();

            var exercise = context.Exercises
                                  .AsNoTracking()
                                  .FirstOrDefault(e => e.id == exerciseId);

            if (exercise == null)
            {
                MessageBox.Show("Không tìm thấy bài tập!",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            _exerciseList.Add(new RoutineExerciseItem
            {
                Index = _exerciseList.Count + 1,
                ExerciseId = exercise.id,
                ExerciseName = exercise.name,
                Sets = 1
            });
        }

        private bool HasExerciseListChanged()
        {
            if (_exerciseList.Count != _originalExercises.Count)
                return true;

            for (int i = 0; i < _exerciseList.Count; i++)
            {
                if (_exerciseList[i].ExerciseId != _originalExercises[i].ExerciseId)
                    return true;

                if (_exerciseList[i].Sets != _originalExercises[i].Sets)
                    return true;
            }

            return false;
        }
    }
}
