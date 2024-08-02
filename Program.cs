using System.Data.SqlClient;
namespace Flashcards
{
    internal class Program
    {
        static string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";
        static void Main(string[] args)
        {
            DbConnections.CreateDb();
            Menu.MainMenu();
        }
    }
}
