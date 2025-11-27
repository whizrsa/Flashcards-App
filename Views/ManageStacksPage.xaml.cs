using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

namespace Flashcards.Views
{
    public partial class ManageStacksPage : Page
    {
        private readonly string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";

        public ManageStacksPage()
        {
            InitializeComponent();
            LoadStacks();
        }

        private void LoadStacks()
        {
            var stacks = new List<Stack>();
            
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var viewStacks = connection.CreateCommand();
                viewStacks.CommandText = "SELECT Id, StackName FROM Stacks";

                SqlDataReader reader = viewStacks.ExecuteReader();

                while (reader.Read())
                {
                    stacks.Add(new Stack
                    {
                        Id = reader.GetInt32(0),
                        Name = reader.GetString(1)
                    });
                }

                reader.Close();
            }

            dgStacks.ItemsSource = stacks;
        }

        private void BtnAddStack_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStackName.Text))
            {
                MessageBox.Show("Please enter a stack name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var stackTable = connection.CreateCommand();
                    stackTable.CommandText = "INSERT INTO Stacks(StackName) VALUES (@stackName)";
                    stackTable.Parameters.AddWithValue("@stackName", txtStackName.Text);

                    stackTable.ExecuteNonQuery();
                }

                MessageBox.Show($"Stack '{txtStackName.Text}' added successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                txtStackName.Clear();
                LoadStacks();
            }
            catch (SqlException ex)
            {
                MessageBox.Show($"Stack already exists or error occurred: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnDeleteStack_Click(object sender, RoutedEventArgs e)
        {
            if (dgStacks.SelectedItem == null)
            {
                MessageBox.Show("Please select a stack to delete.", "Selection Required", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedStack = (Stack)dgStacks.SelectedItem;
            var result = MessageBox.Show($"Are you sure you want to delete '{selectedStack.Name}'?\n\nThis will also delete all related flashcards and study sessions.", 
                "Confirm Delete", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Delete related study sessions
                    var deleteStudySessions = connection.CreateCommand();
                    deleteStudySessions.CommandText = "DELETE FROM StudySessions WHERE StackId = @stackId";
                    deleteStudySessions.Parameters.AddWithValue("@stackId", selectedStack.Id);
                    deleteStudySessions.ExecuteNonQuery();

                    // Delete related flashcards
                    var deleteFlashCards = connection.CreateCommand();
                    deleteFlashCards.CommandText = "DELETE FROM FlashCards WHERE StackId = @stackId";
                    deleteFlashCards.Parameters.AddWithValue("@stackId", selectedStack.Id);
                    deleteFlashCards.ExecuteNonQuery();

                    // Delete the stack
                    var deleteStack = connection.CreateCommand();
                    deleteStack.CommandText = "DELETE FROM Stacks WHERE Id = @stackId";
                    deleteStack.Parameters.AddWithValue("@stackId", selectedStack.Id);
                    deleteStack.ExecuteNonQuery();
                }

                MessageBox.Show("Stack deleted successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadStacks();
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadStacks();
        }
    }
}
