using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flashcards
{
    internal class Menu
    {
        internal static void MainMenu()
        {
            bool runMenu = true;
            Console.WriteLine("Select a function");
            Console.WriteLine("0. Exit \n1. Add a Stack\n2. Delete a Stack\n3. Add a Flash Card\n4. Delete a Flash Card\n5. Study Session");
            string command = Console.ReadLine();

            switch (command)
            {
                case "0":
                    Console.WriteLine("Goodbye!!");
                    runMenu = false;
                    Environment.Exit(0);
                    break;
                case "1":
                    DbConnections.AddStack();
                    break;
                case "2":
                    DbConnections.DeleteStack();
                    break;
                case "3":
                    DbConnections.AddFlashCard();
                    break;
                case "4":
                    DbConnections.DeleteFlashCard();
                    break;
                case "5":
                    StudySessionMenu.ManageStudySession();
                    break;
                default:
                    Console.WriteLine("Invalid Input. Please choose between 0 and 7!");
                    MainMenu();
                    break;
            }
        }
    }
}
