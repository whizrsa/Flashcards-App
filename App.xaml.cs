using System.Windows;
using Flashcards.Services;

namespace Flashcards
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Initialize database
            DatabaseService.InitializeDatabase();
        }
    }
}
