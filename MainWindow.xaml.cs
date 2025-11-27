using System.Windows;
using Flashcards.Views;

namespace Flashcards
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new WelcomePage());
        }

        private void BtnManageStacks_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ManageStacksPage());
        }

        private void BtnManageFlashcards_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ManageFlashcardsPage());
        }

        private void BtnStudySession_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new StudySessionPage());
        }

        private void BtnViewSessions_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new ViewSessionsPage());
        }

        private void BtnYearlyReport_Click(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new YearlyReportPage());
        }

        private void BtnExit_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", 
                "Exit Application", 
                MessageBoxButton.YesNo, 
                MessageBoxImage.Question);
            
            if (result == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
    }
}
