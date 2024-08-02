using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Globalization;

namespace Flashcards.Models
{
    internal class StudySessionMenu
    {
        static string connectionString = "Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True";
        internal static void ManageStudySession()
        {
            Console.Clear();
            Console.WriteLine("Study Session Menu");
            Console.WriteLine("0. Return to Main Menu\n1. Add Session\n2. View Study Sessions by Stack\n3. Average Score Yearly Report\n4. Delete Study Session");
            string command = Console.ReadLine();

            switch (command) 
            {
                case "0":
                    Console.Clear();
                    Menu.MainMenu();
                    break;
                case "1":
                    StudySession();
                    break;
                case "2":
                    ViewStudySession();
                    break;
                case "3":
                    AverageYearlyReport();
                    break;
                case "4":
                    DeleteStudySession();
                    break;
                default:
                    Console.WriteLine("Incorrect Input. Please choose a number between 0 and 3");
                    Console.ReadLine();
                    ManageStudySession();
                    break;
            
            }
        }

        public static void AverageYearlyReport()
        {
            Console.WriteLine("Enter the year Format: yyyy. Press 0 to return to menu");
            string year = Console.ReadLine();

            if (year == "0") ManageStudySession();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Create the command to get the study sessions for the specified year
                var yearlyReport = connection.CreateCommand();
                yearlyReport.CommandText = @"
            SELECT * FROM StudySessions WHERE YEAR(Date) = @year
        ";
                yearlyReport.Parameters.AddWithValue("@year", year);

                SqlDataReader reader = yearlyReport.ExecuteReader();

                if (!reader.HasRows)
                {
                    Console.WriteLine("No sessions found for the specified year. Press any key to return to menu");
                    Console.ReadLine();
                    reader.Close();
                    connection.Close();
                    ManageStudySession();
                    return;
                }

                reader.Close();

                // Create the pivot query to summarize the data by month
                var viewSessionYearlyReport = connection.CreateCommand();
                viewSessionYearlyReport.CommandText = @"
            SELECT StackId,
                ISNULL([1], 0) AS Jan,
                ISNULL([2], 0) AS Feb,
                ISNULL([3], 0) AS Mar,
                ISNULL([4], 0) AS Apr,
                ISNULL([5], 0) AS May,
                ISNULL([6], 0) AS Jun,
                ISNULL([7], 0) AS Jul,
                ISNULL([8], 0) AS Aug,
                ISNULL([9], 0) AS Sep,
                ISNULL([10], 0) AS Oct,
                ISNULL([11], 0) AS Nov,
                ISNULL([12], 0) AS Dec
            FROM
            (
                SELECT StackId, MONTH(Date) AS Month, COUNT(*) AS SessionsCount
                FROM StudySessions
                WHERE YEAR(Date) = @year
                GROUP BY StackId, MONTH(Date)
            ) AS SourceTable
            PIVOT
            (
                SUM(SessionsCount)
                FOR Month IN ([1], [2], [3], [4], [5], [6], [7], [8], [9], [10], [11], [12])
            ) AS PivotTable
            ORDER BY StackId
        ";
                viewSessionYearlyReport.Parameters.AddWithValue("@year", year);

                SqlDataReader pivotReader = viewSessionYearlyReport.ExecuteReader();

                // Display the results
                Console.WriteLine("StackId\tJan\tFeb\tMar\tApr\tMay\tJun\tJul\tAug\tSep\tOct\tNov\tDec");
                while (pivotReader.Read())
                {
                    Console.WriteLine($"{pivotReader["StackId"]}\t{pivotReader["Jan"]}\t{pivotReader["Feb"]}\t{pivotReader["Mar"]}\t{pivotReader["Apr"]}\t{pivotReader["May"]}\t{pivotReader["Jun"]}\t{pivotReader["Jul"]}\t{pivotReader["Aug"]}\t{pivotReader["Sep"]}\t{pivotReader["Oct"]}\t{pivotReader["Nov"]}\t{pivotReader["Dec"]}");
                }

                pivotReader.Close();
                connection.Close();
            }

            Console.WriteLine("Press any key to return to the main menu.");
            Console.ReadLine();
            ManageStudySession();
        }


        public static void SessionNav()
        {
            bool isValid = true;

            while (isValid)
            {
                Console.WriteLine("No Study Sessions found. Would you like to try again");
                Console.WriteLine("1. Yes \n2. No");
                string command = Console.ReadLine();

                switch (command.ToLower())
                {
                    case "1":
                        AverageYearlyReport();
                        break;
                    case "2":
                        isValid = false;
                        ManageStudySession();
                        break;
                    default:
                        Console.WriteLine("You chose the wrong option. Chose 1 or 2");
                        break;
                }

            }
            
        }

        public static void DeleteStudySession()
        {
            Console.Clear();
            ViewAllStudySession();
            Console.WriteLine("Enter Session Id to Delete. Press 0 to retun to menu");
            string input = Console.ReadLine();
            int sessionId;

            // Validate if the input is an integer
            if (!int.TryParse(input, out sessionId))
            {
                Console.WriteLine("The number you entered is not an integer. Press any key to try again.");
                Console.ReadLine();
                DeleteStudySession();
                return;
            }

            // Check if the input is 0 to return to the menu
            if (sessionId == 0)
            {
                ManageStudySession();
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var deleteSession = connection.CreateCommand();
                deleteSession.CommandText = @"
                DELETE FROM StudySessions WHERE Id = @sessionId 
                ";
                deleteSession.Parameters.AddWithValue(@"sessionId", sessionId);

                deleteSession.ExecuteNonQuery();
                connection.Close();
            }

            Console.WriteLine($"Session Id: {sessionId} successfully deleted!");
            Console.ReadLine();
            ManageStudySession();
        }

        internal static void ViewAllStudySession()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var selectSessionsCmd = connection.CreateCommand();
                selectSessionsCmd.CommandText = @"
                SELECT Id, StackId, Date, Score FROM StudySessions
                ";

                List<StudySession> sessions = new List<StudySession>();
                SqlDataReader reader = selectSessionsCmd.ExecuteReader();

                if (reader.HasRows)
                {
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
                }
                else
                {
                    Console.WriteLine("Record Empty.Press any Key to return to start of program");
                    Console.ReadLine();
                    connection.Close();
                    ViewStudySession();
                }

                reader.Close();
                connection.Close();
                Console.WriteLine("======================================\n");
                Console.WriteLine("Sessions\n");
                foreach (var session in sessions)
                {
                    Console.WriteLine($"Session Id:\t{session.Id}\nStack Id:\t{session.StackId}\nDate:\t{session.Date}\nScore:\t{session.Score}\n\n");
                }
                Console.WriteLine("======================================\n");

            }
        }

        private static void ViewStudySession()
        {
            using(SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var selectSessionsCmd = connection.CreateCommand();
                selectSessionsCmd.CommandText = @"
                SELECT Date,Score FROM StudySessions
                ";

                List<StudySession> sessions = new List<StudySession>();
                SqlDataReader reader = selectSessionsCmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        sessions.Add(new StudySession
                        {
                            Date = reader.GetDateTime(0),
                            Score = reader.GetInt32(1)
                        });
                    }
                }else
                {
                    Console.WriteLine("Record Empty.Press any Key to return to start of program");
                    Console.ReadLine();
                    connection.Close();
                    ViewStudySession();
                }

                reader.Close();
                connection.Close();
                Console.WriteLine("======================================\n");
                Console.WriteLine("Session Date\t\t\tScore");
                foreach (var session in sessions)
                {
                    Console.WriteLine($"{session.Date}\t\t{session.Score}");
                }
                Console.WriteLine("======================================\n");


                Console.WriteLine("The Average Score: " + sessions.Average(s => s.Score));
                Console.WriteLine("Press any key to return to main menu");
                Console.ReadLine();
                Console.Clear();
                Menu.MainMenu();
            }
        }

        internal static void StudySession()
        {
            Console.Clear();
            DbConnections.ViewStacks();
            Console.WriteLine("Please enter a stack name or enter 0 to return to main menu");
            string stackName = Console.ReadLine();

            Console.Clear();
            if (stackName == "0") Menu.MainMenu();

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var stackNameTb = connection.CreateCommand();
                stackNameTb.CommandText = @"
                SELECT Id FROM Stacks WHERE StackName = @stackName
                ";
                stackNameTb.Parameters.AddWithValue("@stackName", stackName);
                object stackIdObj = stackNameTb.ExecuteScalar();

                if (stackIdObj == null)
                {
                    Console.WriteLine($"Stack name {stackName} does not exist! Press any key to continue");
                    Console.ReadLine();
                    connection.Close();
                    Menu.MainMenu();
                    return;
                }

                int stackId = (int)stackIdObj;

                var flashcardCmd = connection.CreateCommand();
                flashcardCmd.CommandText = @"
                SELECT Id, Front, Back FROM FlashCards WHERE StackId = @stackId
                ";
                flashcardCmd.Parameters.AddWithValue("@stackId", stackId);

                SqlDataReader reader = flashcardCmd.ExecuteReader();
                List<FlashCard> flashcards = new List<FlashCard>();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        flashcards.Add(new FlashCard
                        {
                            Id = reader.GetInt32(0),
                            StackId = stackId,
                            Front = reader.GetString(1),
                            Back = reader.GetString(2)
                        });
                    }
                }
                else
                {
                    Console.WriteLine("No flashcards found for this stack. Press any key to continue");
                    connection.Close();
                    Console.ReadLine();
                    StudySession();
                    return;
                }

                connection.Close();
                SessionInput(flashcards, stackId);
            }
        }

        internal static void SessionInput(List<FlashCard> flashcards, int stackId)
        {
            int score = 0;
            foreach (var flashcard in flashcards)
            {
                Console.Clear();
                Console.WriteLine($"Flashcard ID: {flashcard.Id}");
                Console.WriteLine($"Question: {flashcard.Front}");
                Console.Write("Your answer: ");
                string answer = Console.ReadLine();

                if (answer.Equals(flashcard.Back, StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Correct!");
                    score++;
                }
                else
                {
                    Console.WriteLine($"Incorrect. The correct answer is: {flashcard.Back}");
                }

                Console.WriteLine("Press any key to continue");
                Console.ReadLine();
                Console.Clear();
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var recordSessionCmd = connection.CreateCommand();
                recordSessionCmd.CommandText = @"
                INSERT INTO StudySessions (StackId, Date, Month, Score)
                VALUES (@stackId, @date, @month, @score)
                ";

                recordSessionCmd.Parameters.AddWithValue("@stackId", stackId);
                recordSessionCmd.Parameters.AddWithValue("@date", DateTime.Now);
                recordSessionCmd.Parameters.AddWithValue("@month", DateTime.Now.Month);
                recordSessionCmd.Parameters.AddWithValue("@score", score);

                recordSessionCmd.ExecuteNonQuery();
                connection.Close();
            }

            Console.WriteLine("Study Session completed. Press any key to return to main menu.");
            Console.ReadLine();
            Menu.MainMenu();
        }
    }
}
