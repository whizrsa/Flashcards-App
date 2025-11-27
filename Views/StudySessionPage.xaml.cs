using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Flashcards.Views
{
    public partial class StudySessionPage : Page
    {
        private readonly string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";
        private List<FlashCard> currentFlashcards;
        private List<FlashCard> allAvailableFlashcards; // Store all available flashcards
        private int currentCardIndex = 0;
        private int score = 0;
        private int selectedStackId;
        private string selectedCategory = "All";
        private string selectedDifficulty = "All";

        public StudySessionPage()
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

            lstStacks.ItemsSource = stacks;
            UpdateCardCount();
        }

        private void UpdateCardCount()
        {
            if (txtCardCount == null)
            {
                return;
            }

            int count = 0;
            
            if (lstStacks != null && lstStacks.SelectedItem != null)
            {
                var selectedStack = (Stack)lstStacks.SelectedItem;
                count = GetFilteredCardCount(selectedStack.Id);
            }
            else
            {
                count = GetTotalCardCount();
            }
            
            txtCardCount.Text = $"Total cards available: {count}";
            UpdateQuestionCountInfo(count);
        }

        private void UpdateQuestionCountInfo(int availableCards)
        {
            if (txtQuestionCountInfo == null || cmbQuestionCount == null)
            {
                return;
            }

            var selectedItem = cmbQuestionCount.SelectedItem as ComboBoxItem;
            if (selectedItem == null)
            {
                return;
            }

            string countStr = selectedItem.Content.ToString();
            
            if (countStr == "All")
            {
                txtQuestionCountInfo.Text = $"(will study all {availableCards} cards)";
            }
            else
            {
                int requestedCount = int.Parse(countStr);
                if (requestedCount > availableCards)
                {
                    txtQuestionCountInfo.Text = $"(only {availableCards} available)";
                    txtQuestionCountInfo.Foreground = new SolidColorBrush(Colors.Orange);
                }
                else
                {
                    txtQuestionCountInfo.Text = $"(from {availableCards} available)";
                    txtQuestionCountInfo.Foreground = new SolidColorBrush(Color.FromRgb(153, 153, 153));
                }
            }
        }

        private void LstStacks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateCardCount();
        }

        private int GetFilteredCardCount(int stackId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var countCmd = connection.CreateCommand();
                
                string query = "SELECT COUNT(*) FROM FlashCards WHERE StackId = @stackId";
                
                if (selectedCategory != "All")
                {
                    query += " AND Category = @category";
                }
                
                if (selectedDifficulty != "All")
                {
                    query += " AND Difficulty = @difficulty";
                }
                
                countCmd.CommandText = query;
                countCmd.Parameters.AddWithValue("@stackId", stackId);
                
                if (selectedCategory != "All")
                {
                    countCmd.Parameters.AddWithValue("@category", selectedCategory);
                }
                
                if (selectedDifficulty != "All")
                {
                    countCmd.Parameters.AddWithValue("@difficulty", selectedDifficulty);
                }
                
                return (int)countCmd.ExecuteScalar();
            }
        }

        private int GetTotalCardCount()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var countCmd = connection.CreateCommand();
                
                string query = "SELECT COUNT(*) FROM FlashCards WHERE 1=1";
                
                if (selectedCategory != "All")
                {
                    query += " AND Category = @category";
                }
                
                if (selectedDifficulty != "All")
                {
                    query += " AND Difficulty = @difficulty";
                }
                
                countCmd.CommandText = query;
                
                if (selectedCategory != "All")
                {
                    countCmd.Parameters.AddWithValue("@category", selectedCategory);
                }
                
                if (selectedDifficulty != "All")
                {
                    countCmd.Parameters.AddWithValue("@difficulty", selectedDifficulty);
                }
                
                return (int)countCmd.ExecuteScalar();
            }
        }

        private void CmbStudyFilter_Changed(object sender, SelectionChangedEventArgs e)
        {
            if (cmbStudyCategory == null || cmbStudyDifficulty == null || txtCardCount == null)
            {
                return;
            }

            selectedCategory = (cmbStudyCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All";
            selectedDifficulty = (cmbStudyDifficulty.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "All";
            UpdateCardCount();
        }

        private void BtnResetFilters_Click(object sender, RoutedEventArgs e)
        {
            if (cmbStudyCategory == null || cmbStudyDifficulty == null)
            {
                return;
            }

            cmbStudyCategory.SelectedIndex = 0;
            cmbStudyDifficulty.SelectedIndex = 0;
            selectedCategory = "All";
            selectedDifficulty = "All";
            UpdateCardCount();
        }

        private void LstStacks_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lstStacks.SelectedItem != null)
            {
                BtnStartStudy_Click(sender, e);
            }
        }

        private void BtnStartStudy_Click(object sender, RoutedEventArgs e)
        {
            if (lstStacks.SelectedItem == null)
            {
                MessageBox.Show("Please select a stack to study.", "Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedStack = (Stack)lstStacks.SelectedItem;
            selectedStackId = selectedStack.Id;

            // Load ALL available flashcards for selected stack with filters
            allAvailableFlashcards = new List<FlashCard>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var flashcardCmd = connection.CreateCommand();
                
                string query = "SELECT Id, StackId, Front, Back, Category, Difficulty FROM FlashCards WHERE StackId = @stackId";
                
                if (selectedCategory != "All")
                {
                    query += " AND Category = @category";
                }
                
                if (selectedDifficulty != "All")
                {
                    query += " AND Difficulty = @difficulty";
                }
                
                flashcardCmd.CommandText = query;
                flashcardCmd.Parameters.AddWithValue("@stackId", selectedStackId);
                
                if (selectedCategory != "All")
                {
                    flashcardCmd.Parameters.AddWithValue("@category", selectedCategory);
                }
                
                if (selectedDifficulty != "All")
                {
                    flashcardCmd.Parameters.AddWithValue("@difficulty", selectedDifficulty);
                }

                SqlDataReader reader = flashcardCmd.ExecuteReader();

                while (reader.Read())
                {
                    allAvailableFlashcards.Add(new FlashCard
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

            if (allAvailableFlashcards.Count == 0)
            {
                MessageBox.Show("No flashcards match the selected filters. Please adjust your filters or add flashcards.", "No Flashcards",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Determine how many questions to study
            var selectedCountItem = cmbQuestionCount.SelectedItem as ComboBoxItem;
            string countStr = selectedCountItem?.Content.ToString() ?? "All";
            
            int questionsToStudy;
            if (countStr == "All")
            {
                questionsToStudy = allAvailableFlashcards.Count;
            }
            else
            {
                questionsToStudy = int.Parse(countStr);
                if (questionsToStudy > allAvailableFlashcards.Count)
                {
                    questionsToStudy = allAvailableFlashcards.Count;
                }
            }

            // Randomly select the specified number of flashcards
            currentFlashcards = new List<FlashCard>();
            Random random = new Random();
            var shuffled = allAvailableFlashcards.OrderBy(x => random.Next()).ToList();
            currentFlashcards = shuffled.Take(questionsToStudy).ToList();

            // Initialize study session
            currentCardIndex = 0;
            score = 0;

            // Switch to flashcard view
            StackSelectionPanel.Visibility = Visibility.Collapsed;
            FlashcardPanel.Visibility = Visibility.Visible;

            ShowCurrentCard();
        }

        private void ShowCurrentCard()
        {
            if (currentCardIndex < currentFlashcards.Count)
            {
                var card = currentFlashcards[currentCardIndex];
                txtQuestion.Text = card.Front;
                txtUserAnswer.Text = string.Empty;
                txtProgress.Text = $"Question {currentCardIndex + 1} of {currentFlashcards.Count}";
                txtScore.Text = $"Score: {score}";
                txtCurrentCategory.Text = $"Category: {card.Category}";
                txtCurrentDifficulty.Text = $"Difficulty: {card.Difficulty}";
                
                // Color code difficulty
                switch (card.Difficulty.ToLower())
                {
                    case "easy":
                        txtCurrentDifficulty.Foreground = new SolidColorBrush(Colors.Green);
                        break;
                    case "medium":
                        txtCurrentDifficulty.Foreground = new SolidColorBrush(Colors.Orange);
                        break;
                    case "hard":
                        txtCurrentDifficulty.Foreground = new SolidColorBrush(Colors.Red);
                        break;
                    default:
                        txtCurrentDifficulty.Foreground = new SolidColorBrush(Colors.Gray);
                        break;
                }

                // Reset feedback panel
                FeedbackPanel.Visibility = Visibility.Collapsed;
                btnSubmitAnswer.Visibility = Visibility.Visible;
                btnNext.Visibility = Visibility.Collapsed;
                btnFinish.Visibility = Visibility.Collapsed;

                txtUserAnswer.Focus();
            }
        }

        private void TxtUserAnswer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter && btnSubmitAnswer.Visibility == Visibility.Visible)
            {
                BtnSubmitAnswer_Click(sender, e);
            }
            else if (e.Key == Key.Enter && btnNext.Visibility == Visibility.Visible)
            {
                BtnNext_Click(sender, e);
            }
        }

        private void BtnSubmitAnswer_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUserAnswer.Text))
            {
                MessageBox.Show("Please enter an answer.", "Answer Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var card = currentFlashcards[currentCardIndex];
            bool isCorrect = txtUserAnswer.Text.Trim().Equals(card.Back, StringComparison.OrdinalIgnoreCase);

            if (isCorrect)
            {
                score++;
                txtFeedback.Text = "CORRECT!";
                txtFeedback.Foreground = new SolidColorBrush(Colors.Green);
                txtCorrectAnswer.Text = string.Empty;
                FeedbackPanel.Background = new SolidColorBrush(Color.FromRgb(220, 255, 220));
            }
            else
            {
                txtFeedback.Text = "INCORRECT";
                txtFeedback.Foreground = new SolidColorBrush(Colors.Red);
                txtCorrectAnswer.Text = $"Correct answer: {card.Back}";
                FeedbackPanel.Background = new SolidColorBrush(Color.FromRgb(255, 220, 220));
            }

            FeedbackPanel.Visibility = Visibility.Visible;
            btnSubmitAnswer.Visibility = Visibility.Collapsed;

            if (currentCardIndex < currentFlashcards.Count - 1)
            {
                btnNext.Visibility = Visibility.Visible;
            }
            else
            {
                btnFinish.Visibility = Visibility.Visible;
            }

            txtScore.Text = $"Score: {score}";
        }

        private void BtnNext_Click(object sender, RoutedEventArgs e)
        {
            currentCardIndex++;
            ShowCurrentCard();
        }

        private void BtnFinish_Click(object sender, RoutedEventArgs e)
        {
            // Save study session to database
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var recordSessionCmd = connection.CreateCommand();
                recordSessionCmd.CommandText = @"
                    INSERT INTO StudySessions (StackId, Date, Month, Score)
                    VALUES (@stackId, @date, @month, @score)";

                recordSessionCmd.Parameters.AddWithValue("@stackId", selectedStackId);
                recordSessionCmd.Parameters.AddWithValue("@date", DateTime.Now);
                recordSessionCmd.Parameters.AddWithValue("@month", DateTime.Now.Month);
                recordSessionCmd.Parameters.AddWithValue("@score", score);

                recordSessionCmd.ExecuteNonQuery();
            }

            // Show results
            FlashcardPanel.Visibility = Visibility.Collapsed;
            ResultsPanel.Visibility = Visibility.Visible;

            txtFinalScore.Text = $"Your Score: {score}/{currentFlashcards.Count}";
            double percentage = (double)score / currentFlashcards.Count * 100;
            txtPercentage.Text = $"{percentage:F0}%";
            
            string filterInfo = $"Questions studied: {currentFlashcards.Count}";
            if (selectedCategory != "All" || selectedDifficulty != "All")
            {
                filterInfo += "\nFilters applied: ";
                if (selectedCategory != "All") filterInfo += $"Category: {selectedCategory} ";
                if (selectedDifficulty != "All") filterInfo += $"Difficulty: {selectedDifficulty}";
            }
            txtSessionSummary.Text = filterInfo;

            if (percentage >= 80)
            {
                txtPercentage.Foreground = new SolidColorBrush(Colors.Green);
            }
            else if (percentage >= 60)
            {
                txtPercentage.Foreground = new SolidColorBrush(Colors.Orange);
            }
            else
            {
                txtPercentage.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private void BtnNewSession_Click(object sender, RoutedEventArgs e)
        {
            // Reset everything
            ResultsPanel.Visibility = Visibility.Collapsed;
            StackSelectionPanel.Visibility = Visibility.Visible;
            currentCardIndex = 0;
            score = 0;
            currentFlashcards = null;
            allAvailableFlashcards = null;
            lstStacks.SelectedIndex = -1;
        }
    }
}
