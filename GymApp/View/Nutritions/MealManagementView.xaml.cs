using GymApp.Core;
using GymApp.Data;
using GymApp.Model;
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
    public partial class MealManagementView : UserControl
    {
        public ObservableCollection<MealRow> Meals { get; set; }
            = new ObservableCollection<MealRow>();

        private ICollectionView _mealView;

        public MealManagementView()
        {
            InitializeComponent();

            _mealView = CollectionViewSource.GetDefaultView(Meals);
            dgMeals.ItemsSource = _mealView;

            LoadMeals();
        }

        // ================= LOAD =================
        private void LoadMeals()
        {
            if (!UserSession.IsLoggedIn)
            {
                MessageBox.Show("Vui lòng đăng nhập để xem bữa ăn");
                return;
            }

            var list = GetMealsFromDb()
                .OrderByDescending(m => m.date)
                .ToList();

            Meals.Clear();
            int index = 1;

            foreach (var meal in list)
            {
                Meals.Add(new MealRow
                {
                    Index = index++,
                    Id = meal.id,
                    Name = meal.name,
                    MealType = meal.meal_type,
                    TotalCalories = CalculateCalories(meal),
                    Date = meal.date?.ToString("dd/MM/yyyy") ?? ""
                });
            }

            _mealView.Refresh();
            txtTotal.Text = Meals.Count.ToString();
        }

        // ================= CLICK NAME =================
        private void MealName_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb && tb.DataContext is MealRow row)
            {
                var mainWindow = Window.GetWindow(this) as MainWindow;
                if (mainWindow == null) return;

                mainWindow.MainContent.Children.Clear();
                mainWindow.MainContent.Children.Add(
                    new DetailMealView(row.Id)
                );
            }
        }

        // ================= ADD NEW =================
        private void BtnAddNew_Click(object sender, RoutedEventArgs e)
        {
            var addView = new AddMealView();
            addView.Owner = Window.GetWindow(this);
            addView.ShowDialog();

            LoadMeals();
        }

        // ================= SEARCH =================
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(keyword))
            {
                _mealView.Filter = null;
            }
            else
            {
                _mealView.Filter = obj =>
                {
                    if (obj is MealRow row)
                    {
                        return row.Name.ToLower().Contains(keyword)
                            || row.MealType.ToLower().Contains(keyword);
                    }
                    return false;
                };
            }

            _mealView.Refresh();
            txtTotal.Text = _mealView.Cast<object>().Count().ToString();
        }

        // ================= DB =================
        private List<Meal> GetMealsFromDb()
        {
            int currentUserId = UserSession.CurrentUser!.id;

            using var context = new GymDbContext();

            return context.Meals
                .AsNoTracking()
                .Include(m => m.MealFoods)
                .ThenInclude(mf => mf.Food)
                .Where(m => m.Nutrition.user_id == currentUserId)
                .ToList();
        }

        // ================= CALORIES =================
        private double CalculateCalories(Meal meal)
        {
            if (meal.MealFoods == null) return 0;

            return meal.MealFoods
                .Where(mf => mf.Food != null)
                .Sum(mf => mf.Food.calories);
        }
    }

    // ================= ROW MODEL =================
    public class MealRow
    {
        public int Index { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public string MealType { get; set; }
        public double TotalCalories { get; set; }
        public string Date { get; set; }
    }
}
