using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
using GymApp.View.Nutritions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace GymApp.View.Nutritions
{
    public partial class FoodManagementView : UserControl
    {
        public ObservableCollection<FoodRow> Foods { get; set; }
            = new ObservableCollection<FoodRow>();

        private ICollectionView _foodView;

        public FoodManagementView()
        {
            InitializeComponent();

            // Tạo CollectionView từ Foods
            _foodView = CollectionViewSource.GetDefaultView(Foods);
            dgFoods.ItemsSource = _foodView;

            LoadFoods();
        }

        private void LoadFoods()
        {
            // 🔒 BẮT BUỘC đăng nhập
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem danh sách thức ăn");
                return;
            }

            var list = GetFoodsFromDb();

            Foods.Clear();
            int index = 1;

            foreach (var food in list.OrderBy(f => f.name))
            {
                Foods.Add(new FoodRow
                {
                    Index = index++,
                    Id = food.id,
                    Name = food.name,
                    Calories = food.calories,
                    CreatedBy = food.is_system ? "System" : "Bạn"
                });
            }

            _foodView.Refresh();
            txtTotal.Text = Foods.Count.ToString();
        }

        // ================= CLICK TÊN THỨC ĂN =================
        private void FoodName_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is FoodRow row)
            {
                // Tìm MainWindow
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow == null) return;

                // Xóa màn hình hiện tại
                mainWindow.MainContent.Children.Clear();

                // Load màn hình chi tiết (bạn tự implement)
                mainWindow.MainContent.Children.Add(
                    new DetailFoodView(row.Id)
                );
            }
        }

        // ================= NÚT THÊM MỚI =================
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

            var addWindow = new AddFoodView();
            addWindow.Owner = Window.GetWindow(this);

            // mở dạng modal
            addWindow.ShowDialog();

            // reload danh sách
            LoadFoods();
        }

        // ================= SEARCH =================
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_foodView == null) return;

            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                _foodView.Filter = null;
            }
            else
            {
                _foodView.Filter = obj =>
                {
                    if (obj is FoodRow row && !string.IsNullOrEmpty(row.Name))
                    {
                        return row.Name.ToLower().StartsWith(keyword);
                    }
                    return false;
                };
            }

            _foodView.Refresh();
            txtTotal.Text = _foodView.Cast<object>().Count().ToString();
        }

        // ================= DB =================
        private List<Food> GetFoodsFromDb()
        {
            int currentUserId = UserSession.CurrentUser!.id;

            using var context = new GymDbContext();

            return context.Foods
                .AsNoTracking()
                .Where(f =>
                    f.is_system == true ||
                    f.owner_id == currentUserId
                )
                .ToList();
        }
    }

    // ================= ROW MODEL =================
    public class FoodRow
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public double Calories { get; set; }
        public string CreatedBy { get; set; }
    }
}
