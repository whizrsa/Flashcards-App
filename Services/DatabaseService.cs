using System.Data.SqlClient;

namespace Flashcards.Services
{
    internal class DatabaseService
    {
        private static readonly string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";

        internal static void InitializeDatabase()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                var createStackTable = connection.CreateCommand();
                createStackTable.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'Stacks' AND xtype ='U')
                BEGIN
                    CREATE TABLE Stacks (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            StackName NVARCHAR(100) NOT NULL UNIQUE
                    );
                END
                ";

                var createFlashCardTable = connection.CreateCommand();
                createFlashCardTable.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FlashCards' AND xtype='U')
                BEGIN
                    CREATE TABLE FlashCards (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            StackId INT NOT NULL,
                            Front NVARCHAR(255) NOT NULL,
                            Back NVARCHAR(255) NOT NULL,
                            Category NVARCHAR(50) NULL,
                            Difficulty NVARCHAR(20) NULL,
                            FOREIGN KEY (StackId) REFERENCES Stacks(Id)
                     );
                END
                ";

                var createStudySession = connection.CreateCommand();
                createStudySession.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='StudySessions' AND xtype='U')
                BEGIN
                    CREATE TABLE StudySessions (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        StackId INT NOT NULL,
                        Date DATETIME NOT NULL,
                        Month INT NOT NULL,
                        Score INT NOT NULL,
                        FOREIGN KEY (StackId) REFERENCES Stacks(Id)
                    );
                END
                ";

                createStackTable.ExecuteNonQuery();
                createFlashCardTable.ExecuteNonQuery();
                createStudySession.ExecuteNonQuery();

                // Add Category and Difficulty columns to FlashCards if they don't exist
                var alterFlashCardsTable = connection.CreateCommand();
                alterFlashCardsTable.CommandText = @"
                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[FlashCards]') AND name = 'Category')
                BEGIN
                    ALTER TABLE FlashCards ADD Category NVARCHAR(50) NULL;
                END

                IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[FlashCards]') AND name = 'Difficulty')
                BEGIN
                    ALTER TABLE FlashCards ADD Difficulty NVARCHAR(20) NULL;
                END
                ";
                alterFlashCardsTable.ExecuteNonQuery();

                // Update existing records to have default values
                var updateDefaults = connection.CreateCommand();
                updateDefaults.CommandText = @"
                UPDATE FlashCards 
                SET Category = 'General', Difficulty = 'Medium' 
                WHERE Category IS NULL OR Difficulty IS NULL
                ";
                updateDefaults.ExecuteNonQuery();

                connection.Close();
            }
        }
    }
}
