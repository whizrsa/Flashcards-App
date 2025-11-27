using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flashcards.Models;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections;

namespace Flashcards
{
    internal class DbConnections
    {
        static string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";
        internal static void CreateDb()
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

        internal static void AddStack()
        {
            Console.Clear();
            Console.WriteLine("Please enter the name of the stack? Press 0 to return to main menu.");
            string stackName = Console.ReadLine();

            if (stackName == "0") Menu.MainMenu();

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    var stackTable = connection.CreateCommand();
                    stackTable.CommandText = @"INSERT INTO Stacks(StackName) VALUES (@stackName)";
                    stackTable.Parameters.AddWithValue("@stackName", stackName);

                    stackTable.ExecuteNonQuery();

                    connection.Close();

                }
            }
            catch (DbException e)
            {
                Console.WriteLine($"{stackName} already exists" + e);
                Console.WriteLine("Continue");
                Console.ReadLine();
            }
            Console.WriteLine($"{stackName} added. Press any key to continue");
            Console.ReadLine();
            Console.Clear();
            Menu.MainMenu();
        }

        internal static void DeleteStack()
        {
            Console.Clear();
            ViewStacks();
            Console.WriteLine("Please enter Stack Id to delete a Stack");
            string input = Console.ReadLine();

            if (input == "0") Menu.MainMenu();

            if (!int.TryParse(input, out int stackId))
            {
                Console.WriteLine("Invalid Stack Id. Please enter a valid integer.");
                Console.ReadLine();
                DeleteStack();
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Delete related study sessions first
                var deleteStudySessions = connection.CreateCommand();
                deleteStudySessions.CommandText = @"
                    DELETE FROM StudySessions WHERE StackId = @stackId
                    ";
                deleteStudySessions.Parameters.AddWithValue("@stackId", stackId);
                deleteStudySessions.ExecuteNonQuery();

                // Delete related flashcards
                var deleteFlashCards = connection.CreateCommand();
                deleteFlashCards.CommandText = @"
                    DELETE FROM FlashCards WHERE StackId = @stackId
                    ";
                deleteFlashCards.Parameters.AddWithValue("@stackId", stackId);
                deleteFlashCards.ExecuteNonQuery();

                // Delete the stack
                var deleteStack = connection.CreateCommand();
                deleteStack.CommandText = @"
                    DELETE FROM Stacks WHERE Id = @stackId
                    ";
                deleteStack.Parameters.AddWithValue("@stackId", stackId);
                deleteStack.ExecuteNonQuery();

                connection.Close();
            }

            Console.WriteLine("Stack and its related records deleted successfully. Press any key to continue.");
            Console.ReadLine();
            Menu.MainMenu();
        }



        internal static void ViewStacks()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var viewStacks = connection.CreateCommand();
                viewStacks.CommandText = @"
                SELECT Id, StackName FROM Stacks
            ";

                List<Stack> stacks = new List<Stack>();
                SqlDataReader reader = viewStacks.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        stacks.Add(new Stack
                        {
                            Id = reader.GetInt32(0),
                            Name = reader.GetString(1)
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No records found");
                }

                connection.Close();
                Console.WriteLine("=======================================");
                foreach (var stack in stacks)
                {
                    Console.WriteLine("\n");
                    Console.WriteLine($"Stack Name: {stack.Name} \nStack: {stack.Id}");
                }
                Console.WriteLine("=======================================\n\n");
            }
        }

        internal static void AddFlashCard()
        {
            Console.Clear();
            ViewStacks();

            Console.WriteLine("Select a Stack to add the flashcard to. Please enter the id or press 0 to exist");
            int stackId = Convert.ToInt32(Console.ReadLine());


            if (stackId.ToString() == "0") Menu.MainMenu();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var checkIdExists = connection.CreateCommand();
                checkIdExists.CommandText = @"
                SELECT COUNT(*) FROM Stacks WHERE Id = @stackId
                ";
                checkIdExists.Parameters.AddWithValue("@stackId", stackId);
                int checkQuery = Convert.ToInt32(checkIdExists.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"Stack Id: {stackId} does not exist.Press any key to continue");
                    Console.ReadLine();
                    connection.Close();
                    AddFlashCard();

                }

                Console.WriteLine("Enter the question.");
                string question = Console.ReadLine();

                Console.WriteLine("Enter the answer");
                string answer = Console.ReadLine();


                var addFlashToStack = connection.CreateCommand();
                addFlashToStack.CommandText = @"
                INSERT INTO FlashCards(StackId,Front,Back) VALUES
                (@stackId,@question,@answer)
                ";
                addFlashToStack.Parameters.AddWithValue("@stackId", stackId);
                addFlashToStack.Parameters.AddWithValue("@question", question);
                addFlashToStack.Parameters.AddWithValue("@answer", answer);

                addFlashToStack.ExecuteNonQuery();
                connection.Close();
            }

            Console.WriteLine("Flashcard added successfully. Click any button to continue");
            Console.ReadLine();
            Console.Clear();
            Menu.MainMenu();
        }

        internal static void DeleteFlashCard()
        {
            Console.WriteLine("What Flash card would you like to delete?");
            int flashCardId = Convert.ToInt32(Console.ReadLine());

            if (flashCardId.ToString() == "0") Menu.MainMenu();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var flashCardTb = connection.CreateCommand();
                flashCardTb.CommandText = @"
                SELECT COUNT(*) FROM FlashCards WHERE Id = @flashCardId
                
                ";
                flashCardTb.Parameters.AddWithValue("@flashCardId", flashCardId);
                int checkQuery = Convert.ToInt32(flashCardTb.ExecuteScalar());

                if (checkQuery == 0)
                {
                    Console.WriteLine($"FlashCard ID: {flashCardId} does not exists. Press any button to re-enter id");
                    Console.ReadLine();
                    connection.Close();
                    DeleteFlashCard();

                }

                var deleteFlashCardId = connection.CreateCommand();
                deleteFlashCardId.CommandText = @"
                    DELETE FROM FlashCards WHERE Id = @flashCardId
                    ";
                deleteFlashCardId.Parameters.AddWithValue("@flashCardId", flashCardId);

                flashCardTb.ExecuteNonQuery();
                deleteFlashCardId.ExecuteNonQuery();
                connection.Close();

                Console.WriteLine("Flash Card successfully deleted. Press any key to return to main menu");
                Console.ReadLine();
                Menu.MainMenu();

            }
        }


    }
 }
