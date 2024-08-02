using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flashcards
{
    internal class FlashCard
    {
        public int Id { get;  set; }
        public int StackId { get;  set; }
        public string Front { get;  set; }
        public string Back { get;  set; }    

        /*
        public FlashCard(int id, string stackId, string front, string back)
        {
            Id = id;
            StackId = stackId;
            Front = front;
            Back = back;
        }

        */
    }
}
