using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Flashcards.Views
{
    public partial class ManageFlashcardsPage : Page
    {
        private readonly string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";
        private int? editingFlashcardId = null;
        private bool isInitialized = false;

        public ManageFlashcardsPage()
        {
            InitializeComponent();
            
            // Wait for the page to fully load before initializing data
            this.Loaded += ManageFlashcardsPage_Loaded;
        }

        private void ManageFlashcardsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!isInitialized)
            {
                AddSampleFlashcards();
                LoadStacks();
                LoadFlashcards();
                isInitialized = true;
            }
        }

        private void AddSampleFlashcards()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var checkCmd = connection.CreateCommand();
                    checkCmd.CommandText = "SELECT COUNT(*) FROM FlashCards";
                    int count = (int)checkCmd.ExecuteScalar();

                    if (count < 20)
                    {
                        int animalsStackId = GetOrCreateStack(connection, "Animals");
                        int carsStackId = GetOrCreateStack(connection, "Vehicles");
                        int countriesStackId = GetOrCreateStack(connection, "Geography");
                        int mathStackId = GetOrCreateStack(connection, "Mathematics");
                        int scienceStackId = GetOrCreateStack(connection, "Science");

                        var sampleCards = new List<(int stackId, string category, string difficulty, string question, string answer)>
                        {
                            // Animals - 15 cards
                            (animalsStackId, "Animals", "Easy", "What is the largest land animal?", "elephant"),
                            (animalsStackId, "Animals", "Easy", "What animal is known as 'man's best friend'?", "dog"),
                            (animalsStackId, "Animals", "Easy", "What is a baby cat called?", "kitten"),
                            (animalsStackId, "Animals", "Easy", "What sound does a cow make?", "moo"),
                            (animalsStackId, "Animals", "Easy", "What animal says 'meow'?", "cat"),
                            (animalsStackId, "Animals", "Medium", "What is the fastest land animal?", "cheetah"),
                            (animalsStackId, "Animals", "Medium", "What is the tallest animal?", "giraffe"),
                            (animalsStackId, "Animals", "Medium", "What animal has black and white stripes?", "zebra"),
                            (animalsStackId, "Animals", "Medium", "What is the largest bird?", "ostrich"),
                            (animalsStackId, "Animals", "Medium", "What animal has a trunk?", "elephant"),
                            (animalsStackId, "Animals", "Hard", "What is the only mammal capable of true flight?", "bat"),
                            (animalsStackId, "Animals", "Hard", "What is the slowest land animal?", "sloth"),
                            (animalsStackId, "Animals", "Hard", "What animal has the longest lifespan?", "tortoise"),
                            (animalsStackId, "Animals", "Hard", "What is a group of lions called?", "pride"),
                            (animalsStackId, "Animals", "Hard", "What bird can fly backwards?", "hummingbird"),
                            
                            // Cars - 12 cards
                            (carsStackId, "Cars", "Easy", "What car brand has a prancing horse logo?", "Ferrari"),
                            (carsStackId, "Cars", "Easy", "What car brand has four rings?", "Audi"),
                            (carsStackId, "Cars", "Easy", "What car brand has a three-pointed star?", "Mercedes"),
                            (carsStackId, "Cars", "Easy", "What color is typically used for stop signs?", "red"),
                            (carsStackId, "Cars", "Medium", "What does SUV stand for? (no spaces)", "SportUtilityVehicle"),
                            (carsStackId, "Cars", "Medium", "What does BMW stand for? (no spaces)", "BayerischeMotorenWerke"),
                            (carsStackId, "Cars", "Medium", "What is the luxury brand of Toyota?", "Lexus"),
                            (carsStackId, "Cars", "Medium", "What Italian brand makes the Aventador?", "Lamborghini"),
                            (carsStackId, "Cars", "Hard", "What was the first mass-produced car?", "ModelT"),
                            (carsStackId, "Cars", "Hard", "What does ABS stand for in cars? (no spaces)", "AntilockBrakingSystem"),
                            (carsStackId, "Cars", "Hard", "What year was the first Ford Mustang released?", "1964"),
                            (carsStackId, "Cars", "Hard", "What is the fastest production car? (as of 2020)", "BugattiChiron"),
                            
                            // Countries/Geography - 18 cards
                            (countriesStackId, "Countries", "Easy", "What is the capital of France?", "Paris"),
                            (countriesStackId, "Countries", "Easy", "What is the largest country by area?", "Russia"),
                            (countriesStackId, "Countries", "Easy", "What is the capital of England?", "London"),
                            (countriesStackId, "Countries", "Easy", "What is the capital of Japan?", "Tokyo"),
                            (countriesStackId, "Countries", "Easy", "What is the capital of Italy?", "Rome"),
                            (countriesStackId, "Countries", "Easy", "What is the capital of Spain?", "Madrid"),
                            (countriesStackId, "Countries", "Medium", "What country has the most islands in the world?", "Sweden"),
                            (countriesStackId, "Countries", "Medium", "What is the capital of Australia?", "Canberra"),
                            (countriesStackId, "Countries", "Medium", "What is the largest desert?", "Sahara"),
                            (countriesStackId, "Countries", "Medium", "What is the longest river?", "Nile"),
                            (countriesStackId, "Countries", "Medium", "What is the tallest mountain?", "Everest"),
                            (countriesStackId, "Countries", "Medium", "What ocean is the largest?", "Pacific"),
                            (countriesStackId, "Countries", "Hard", "What is the smallest country in the world?", "VaticanCity"),
                            (countriesStackId, "Countries", "Hard", "What is the capital of Mongolia?", "Ulaanbaatar"),
                            (countriesStackId, "Countries", "Hard", "What country has the most time zones?", "France"),
                            (countriesStackId, "Countries", "Hard", "What is the deepest ocean trench?", "MarianaTrench"),
                            (countriesStackId, "Countries", "Hard", "What is the driest place on Earth?", "AtacamaDesert"),
                            (countriesStackId, "Countries", "Hard", "What country spans two continents?", "Turkey"),
                            
                            // Mathematics - 20 cards
                            (mathStackId, "Mathematics", "Easy", "What is 5 + 7?", "12"),
                            (mathStackId, "Mathematics", "Easy", "What is 10 x 10?", "100"),
                            (mathStackId, "Mathematics", "Easy", "What is 15 - 8?", "7"),
                            (mathStackId, "Mathematics", "Easy", "What is 20 / 4?", "5"),
                            (mathStackId, "Mathematics", "Easy", "What is 3 x 3?", "9"),
                            (mathStackId, "Mathematics", "Easy", "What is 50 / 10?", "5"),
                            (mathStackId, "Mathematics", "Easy", "What is 100 - 25?", "75"),
                            (mathStackId, "Mathematics", "Medium", "What is the square root of 144?", "12"),
                            (mathStackId, "Mathematics", "Medium", "What is 15% of 100?", "15"),
                            (mathStackId, "Mathematics", "Medium", "What is 12 x 12?", "144"),
                            (mathStackId, "Mathematics", "Medium", "What is the square root of 81?", "9"),
                            (mathStackId, "Mathematics", "Medium", "What is 2 to the power of 5?", "32"),
                            (mathStackId, "Mathematics", "Medium", "What is 25% of 200?", "50"),
                            (mathStackId, "Mathematics", "Hard", "What is the value of Pi to 2 decimal places?", "3.14"),
                            (mathStackId, "Mathematics", "Hard", "What is the square root of 169?", "13"),
                            (mathStackId, "Mathematics", "Hard", "What is 15 squared?", "225"),
                            (mathStackId, "Mathematics", "Hard", "What is the value of e to 2 decimal places?", "2.72"),
                            (mathStackId, "Mathematics", "Hard", "What is 20% of 500?", "100"),
                            (mathStackId, "Mathematics", "Hard", "What is 13 x 8?", "104"),
                            (mathStackId, "Mathematics", "Hard", "What is the cube root of 27?", "3"),
                            
                            // Science - 20 cards
                            (scienceStackId, "Science", "Easy", "What planet is known as the Red Planet?", "Mars"),
                            (scienceStackId, "Science", "Easy", "What is the center of an atom called?", "nucleus"),
                            (scienceStackId, "Science", "Easy", "What gas do plants produce?", "oxygen"),
                            (scienceStackId, "Science", "Easy", "What is frozen water called?", "ice"),
                            (scienceStackId, "Science", "Easy", "What is the hottest planet?", "Venus"),
                            (scienceStackId, "Science", "Easy", "What is the largest planet?", "Jupiter"),
                            (scienceStackId, "Science", "Easy", "What force keeps us on the ground?", "gravity"),
                            (scienceStackId, "Science", "Medium", "What is the chemical symbol for water? (no spaces)", "H2O"),
                            (scienceStackId, "Science", "Medium", "What gas do plants absorb from the atmosphere?", "carbondioxide"),
                            (scienceStackId, "Science", "Medium", "What is the smallest planet?", "Mercury"),
                            (scienceStackId, "Science", "Medium", "What is the chemical symbol for gold?", "Au"),
                            (scienceStackId, "Science", "Medium", "What is the speed of sound in m/s?", "343"),
                            (scienceStackId, "Science", "Medium", "How many bones are in the human body?", "206"),
                            (scienceStackId, "Science", "Medium", "What is the powerhouse of the cell?", "mitochondria"),
                            (scienceStackId, "Science", "Hard", "What is the speed of light in km/s?", "300000"),
                            (scienceStackId, "Science", "Hard", "What is the atomic number of carbon?", "6"),
                            (scienceStackId, "Science", "Hard", "What is the rarest blood type?", "ABnegative"),
                            (scienceStackId, "Science", "Hard", "What is the half-life of carbon-14 in years?", "5730"),
                            (scienceStackId, "Science", "Hard", "What is the chemical formula for table salt? (no spaces)", "NaCl"),
                            (scienceStackId, "Science", "Hard", "What is absolute zero in Celsius?", "-273")
                        };

                        foreach (var card in sampleCards)
                        {
                            try
                            {
                                var insertCmd = connection.CreateCommand();
                                insertCmd.CommandText = @"
                                    INSERT INTO FlashCards(StackId, Front, Back, Category, Difficulty) 
                                    VALUES (@stackId, @question, @answer, @category, @difficulty)";
                                insertCmd.Parameters.AddWithValue("@stackId", card.stackId);
                                insertCmd.Parameters.AddWithValue("@question", card.question);
                                insertCmd.Parameters.AddWithValue("@answer", card.answer);
                                insertCmd.Parameters.AddWithValue("@category", card.category);
                                insertCmd.Parameters.AddWithValue("@difficulty", card.difficulty);
                                insertCmd.ExecuteNonQuery();
                            }
                            catch { }
                        }
                    }
                }
            }
            catch { }
        }

        private int GetOrCreateStack(SqlConnection connection, string stackName)
        {
            var checkStack = connection.CreateCommand();
            checkStack.CommandText = "SELECT Id FROM Stacks WHERE StackName = @stackName";
            checkStack.Parameters.AddWithValue("@stackName", stackName);
            object result = checkStack.ExecuteScalar();

            if (result != null)
            {
                return (int)result;
            }

            var createStack = connection.CreateCommand();
            createStack.CommandText = "INSERT INTO Stacks(StackName) VALUES (@stackName); SELECT SCOPE_IDENTITY();";
            createStack.Parameters.AddWithValue("@stackName", stackName);
            return Convert.ToInt32(createStack.ExecuteScalar());
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

            if (cmbStacks != null)
                cmbStacks.ItemsSource = stacks;
            
            if (cmbFilterStacks != null)
                cmbFilterStacks.ItemsSource = stacks;
        }

        private void LoadFlashcards(int? stackId = null, string category = null, string difficulty = null)
        {
            // Check if DataGrid is initialized
            if (dgFlashcards == null)
            {
                return;
            }

            var flashcards = new List<FlashCard>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var flashcardCmd = connection.CreateCommand();
                
                string query = "SELECT Id, StackId, Front, Back, Category, Difficulty FROM FlashCards WHERE 1=1";
                
                if (stackId.HasValue)
                {
                    query += " AND StackId = @stackId";
                }
                
                if (!string.IsNullOrEmpty(category) && category != "All")
                {
                    query += " AND Category = @category";
                }
                
                if (!string.IsNullOrEmpty(difficulty) && difficulty != "All")
                {
                    query += " AND Difficulty = @difficulty";
                }
                
                flashcardCmd.CommandText = query;
                
                if (stackId.HasValue)
                {
                    flashcardCmd.Parameters.AddWithValue("@stackId", stackId.Value);
                }
                
                if (!string.IsNullOrEmpty(category) && category != "All")
                {
                    flashcardCmd.Parameters.AddWithValue("@category", category);
                }
                
                if (!string.IsNullOrEmpty(difficulty) && difficulty != "All")
                {
                    flashcardCmd.Parameters.AddWithValue("@difficulty", difficulty);
                }

                SqlDataReader reader = flashcardCmd.ExecuteReader();

                while (reader.Read())
                {
                    flashcards.Add(new FlashCard
                    {
                        Id = reader.GetInt32(0),
                        StackId = reader.GetInt32(1),
                        Front = reader.GetString(2),
                        Back = reader.GetString(3),
                        Category = reader.IsDBNull(4) ? "General" : reader.GetString(4),
                        Difficulty = reader.IsDBNull(5) ? "Medium" : reader.GetString(5)
                    });
                }

                reader.Close();
            }

            dgFlashcards.ItemsSource = flashcards;
        }

        private void BtnSaveFlashcard_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStacks.SelectedItem == null)
            {
                MessageBox.Show("Please select a stack.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtQuestion.Text) || string.IsNullOrWhiteSpace(txtAnswer.Text))
            {
                MessageBox.Show("Please enter both question and answer.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (txtAnswer.Text.Contains(" "))
            {
                var result = MessageBox.Show(
                    "The answer contains spaces. It's recommended to remove spaces for easier matching.\n\nContinue anyway?",
                    "Answer Contains Spaces",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.No)
                {
                    return;
                }
            }

            var selectedStack = (Stack)cmbStacks.SelectedItem;
            string category = (cmbCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? 
                             (string.IsNullOrWhiteSpace(cmbCategory.Text) ? "General" : cmbCategory.Text);
            string difficulty = (cmbDifficulty.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Medium";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                if (editingFlashcardId.HasValue)
                {
                    var updateFlashcard = connection.CreateCommand();
                    updateFlashcard.CommandText = @"
                        UPDATE FlashCards 
                        SET StackId = @stackId, Front = @question, Back = @answer, 
                            Category = @category, Difficulty = @difficulty
                        WHERE Id = @id";
                    updateFlashcard.Parameters.AddWithValue("@id", editingFlashcardId.Value);
                    updateFlashcard.Parameters.AddWithValue("@stackId", selectedStack.Id);
                    updateFlashcard.Parameters.AddWithValue("@question", txtQuestion.Text);
                    updateFlashcard.Parameters.AddWithValue("@answer", txtAnswer.Text);
                    updateFlashcard.Parameters.AddWithValue("@category", category);
                    updateFlashcard.Parameters.AddWithValue("@difficulty", difficulty);

                    updateFlashcard.ExecuteNonQuery();
                    MessageBox.Show("Flashcard updated successfully!", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var addFlashcard = connection.CreateCommand();
                    addFlashcard.CommandText = @"
                        INSERT INTO FlashCards(StackId, Front, Back, Category, Difficulty) 
                        VALUES (@stackId, @question, @answer, @category, @difficulty)";
                    addFlashcard.Parameters.AddWithValue("@stackId", selectedStack.Id);
                    addFlashcard.Parameters.AddWithValue("@question", txtQuestion.Text);
                    addFlashcard.Parameters.AddWithValue("@answer", txtAnswer.Text);
                    addFlashcard.Parameters.AddWithValue("@category", category);
                    addFlashcard.Parameters.AddWithValue("@difficulty", difficulty);

                    addFlashcard.ExecuteNonQuery();
                    MessageBox.Show($"Flashcard added successfully!\nCategory: {category}\nDifficulty: {difficulty}", "Success", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            ClearForm();
            ApplyCurrentFilters();
        }

        private void BtnViewFlashcard_Click(object sender, RoutedEventArgs e)
        {
            if (dgFlashcards.SelectedItem == null)
            {
                MessageBox.Show("Please select a flashcard to view.", "Selection Required", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedFlashcard = (FlashCard)dgFlashcards.SelectedItem;
            
            string message = $"Flashcard Details:\n\n" +
                           $"ID: {selectedFlashcard.Id}\n" +
                           $"Stack ID: {selectedFlashcard.StackId}\n" +
                           $"Category: {selectedFlashcard.Category}\n" +
                           $"Difficulty: {selectedFlashcard.Difficulty}\n\n" +
                           $"Question:\n{selectedFlashcard.Front}\n\n" +
                           $"Answer:\n{selectedFlashcard.Back}";

            MessageBox.Show(message, "Flashcard Details", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void BtnEditFlashcard_Click(object sender, RoutedEventArgs e)
        {
            if (dgFlashcards.SelectedItem == null)
            {
                MessageBox.Show("Please select a flashcard to edit.", "Selection Required", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedFlashcard = (FlashCard)dgFlashcards.SelectedItem;
            LoadFlashcardForEditing(selectedFlashcard);
        }

        private void DgFlashcards_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (dgFlashcards.SelectedItem != null)
            {
                BtnViewFlashcard_Click(sender, e);
            }
        }

        private void LoadFlashcardForEditing(FlashCard flashcard)
        {
            editingFlashcardId = flashcard.Id;

            cmbStacks.SelectedValue = flashcard.StackId;
            txtQuestion.Text = flashcard.Front;
            txtAnswer.Text = flashcard.Back;

            bool categoryFound = false;
            foreach (ComboBoxItem item in cmbCategory.Items)
            {
                if (item.Content.ToString() == flashcard.Category)
                {
                    cmbCategory.SelectedItem = item;
                    categoryFound = true;
                    break;
                }
            }
            if (!categoryFound)
            {
                cmbCategory.Text = flashcard.Category;
            }

            foreach (ComboBoxItem item in cmbDifficulty.Items)
            {
                if (item.Content.ToString() == flashcard.Difficulty)
                {
                    cmbDifficulty.SelectedItem = item;
                    break;
                }
            }

            txtFormTitle.Text = "Edit Flashcard";
            btnSaveFlashcard.Content = "Update Flashcard";
            btnCancelEdit.Visibility = Visibility.Visible;

            txtQuestion.Focus();
        }

        private void BtnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
        }

        private void ClearForm()
        {
            editingFlashcardId = null;
            txtQuestion.Clear();
            txtAnswer.Clear();
            cmbStacks.SelectedIndex = -1;
            cmbCategory.SelectedIndex = 10;
            cmbDifficulty.SelectedIndex = 1;

            txtFormTitle.Text = "Add New Flashcard";
            btnSaveFlashcard.Content = "Add Flashcard";
            btnCancelEdit.Visibility = Visibility.Collapsed;
        }

        private void BtnDeleteFlashcard_Click(object sender, RoutedEventArgs e)
        {
            if (dgFlashcards.SelectedItem == null)
            {
                MessageBox.Show("Please select a flashcard to delete.", "Selection Required", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedFlashcard = (FlashCard)dgFlashcards.SelectedItem;
            var result = MessageBox.Show(
                $"Are you sure you want to delete this flashcard?\n\nQuestion: {selectedFlashcard.Front}", 
                "Confirm Delete", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var deleteFlashcard = connection.CreateCommand();
                    deleteFlashcard.CommandText = "DELETE FROM FlashCards WHERE Id = @flashcardId";
                    deleteFlashcard.Parameters.AddWithValue("@flashcardId", selectedFlashcard.Id);

                    deleteFlashcard.ExecuteNonQuery();
                }

                MessageBox.Show("Flashcard deleted successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
                
                ClearForm();
                ApplyCurrentFilters();
            }
        }

        private void ApplyCurrentFilters()
        {
            if (cmbFilterStacks == null || cmbFilterCategory == null || cmbFilterDifficulty == null)
            {
                return;
            }

            int? stackId = cmbFilterStacks.SelectedValue as int?;
            string category = (cmbFilterCategory.SelectedItem as ComboBoxItem)?.Content.ToString();
            string difficulty = (cmbFilterDifficulty.SelectedItem as ComboBoxItem)?.Content.ToString();
            
            LoadFlashcards(stackId, category, difficulty);
        }

        private void CmbFilterStacks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            ApplyCurrentFilters();
        }

        private void CmbFilterCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            ApplyCurrentFilters();
        }

        private void CmbFilterDifficulty_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isInitialized) return;
            ApplyCurrentFilters();
        }

        private void BtnShowAll_Click(object sender, RoutedEventArgs e)
        {
            cmbFilterStacks.SelectedIndex = -1;
            cmbFilterCategory.SelectedIndex = 0;
            cmbFilterDifficulty.SelectedIndex = 0;
            LoadFlashcards();
        }
    }
}
