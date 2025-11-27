using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Flashcards.Views
{
    public partial class ViewSessionsPage : Page
    {
        private readonly string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";

        public ViewSessionsPage()
        {
            InitializeComponent();
            LoadSessions();
        }

        private void LoadSessions()
        {
            var sessions = new List<StudySession>();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var selectSessionsCmd = connection.CreateCommand();
                selectSessionsCmd.CommandText = "SELECT Id, StackId, Date, Score FROM StudySessions ORDER BY Date DESC";

                SqlDataReader reader = selectSessionsCmd.ExecuteReader();

                while (reader.Read())
                {
                    sessions.Add(new StudySession
                    {
                        Id = reader.GetInt32(0),
                        StackId = reader.GetInt32(1),
                        Date = reader.GetDateTime(2),
                        Score = reader.GetInt32(3)
                    });
                }

                reader.Close();
            }

            dgSessions.ItemsSource = sessions;
            UpdateStatistics(sessions);
        }

        private void UpdateStatistics(List<StudySession> sessions)
        {
            if (sessions.Count > 0)
            {
                txtTotalSessions.Text = sessions.Count.ToString();
                txtAverageScore.Text = sessions.Average(s => s.Score).ToString("F1");
                txtHighestScore.Text = sessions.Max(s => s.Score).ToString();
            }
            else
            {
                txtTotalSessions.Text = "0";
                txtAverageScore.Text = "0.0";
                txtHighestScore.Text = "0";
            }
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            LoadSessions();
        }

        private void BtnDeleteSession_Click(object sender, RoutedEventArgs e)
        {
            if (dgSessions.SelectedItem == null)
            {
                MessageBox.Show("Please select a session to delete.", "Selection Required",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedSession = (StudySession)dgSessions.SelectedItem;
            var result = MessageBox.Show($"Are you sure you want to delete this study session?",
                "Confirm Delete",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var deleteSession = connection.CreateCommand();
                    deleteSession.CommandText = "DELETE FROM StudySessions WHERE Id = @sessionId";
                    deleteSession.Parameters.AddWithValue("@sessionId", selectedSession.Id);

                    deleteSession.ExecuteNonQuery();
                }

                MessageBox.Show("Study session deleted successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadSessions();
            }
        }
    }
}
