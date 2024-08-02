using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flashcards
{
    internal class StudySession
    {
        public int Id {  get; set; }
        public int StackId { get; set; }
        public DateTime Date { get; set; }
        public int month { get; set; }
        public int Score { get; set; }
    }
}
