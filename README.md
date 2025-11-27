# Flashcards WPF Application

A modern Windows desktop application for studying with flashcards, built with WPF and .NET 8.

## Features

### Stack Management
- Create new stacks to organize your flashcards by topic
- View all available stacks
- Delete stacks (removes all associated flashcards and study sessions)

### Complete Flashcard Management (CRUD)
- **Create** flashcards with questions and answers
- **Read/View** flashcard details in a popup dialog
- **Update/Edit** existing flashcards
- **Delete** flashcards with confirmation
- **Categorize flashcards** by topic:
  - Animals, Cars, Countries, Geography, History
  - Languages, Mathematics, Science, Sports, Technology
  - General (or create custom categories)
- **Set difficulty levels**: Easy, Medium, or Hard
- Filter flashcards by stack, category, and difficulty
- Double-click to view flashcard details
- Answer validation (warns if spaces detected)
- **Pre-loaded sample flashcards** for testing (85 total cards)

### Sample Flashcards Included
The app comes with 85 sample flashcards across multiple categories:
- **Animals** (15 cards): elephant, dog, cheetah, bat, giraffe, zebra, and more
- **Vehicles** (12 cards): Ferrari, Audi, BMW, Lamborghini, and automotive knowledge
- **Geography** (18 cards): Paris, Russia, Sweden, VaticanCity, mountains, oceans
- **Mathematics** (20 cards): Arithmetic, square roots, percentages, Pi
- **Science** (20 cards): Mars, H2O, planets, atoms, physics, chemistry

All sample answers follow the no-spaces rule!

### Study Sessions with Flexible Question Count
- Interactive study sessions with real-time feedback
- Select any stack to study
- **Choose number of questions**: 5, 10, 15, 20, or All
- **Random question selection** for variety
- **Filter by category and difficulty** before starting
- See category and difficulty for each question
- Difficulty color coding (Green=Easy, Orange=Medium, Red=Hard)
- Get immediate feedback on answers (correct/incorrect)
- View correct answers when wrong
- Automatic score tracking
- Beautiful visual interface with progress indicators
- Session summary shows applied filters

### Study History
- View all past study sessions
- See statistics including:
  - Total number of sessions
  - Average score
  - Highest score
- Delete individual study sessions

### Yearly Reports with PDF Export
- Generate reports showing study session counts by month
- Visual data grid showing sessions per stack
- Filter by year
- Easy-to-read pivot table format
- **Export reports to PDF** with professional formatting

## Key Features

### Full CRUD Operations on Flashcards
1. **Create**: Add new flashcards with validation
2. **Read**: View flashcard details
   - Click "View Details" button
   - Or double-click any flashcard in the grid
3. **Update**: Edit existing flashcards
   - Click "Edit Selected" button
   - Form switches to edit mode
   - Click "Cancel" to abort editing
4. **Delete**: Remove flashcards with confirmation

### Answer Validation
- The app warns if your answer contains spaces
- Recommended format: "VaticanCity" not "Vatican City"
- Makes matching answers easier during study sessions
- You can choose to continue if spaces are needed

### Flexible Study Sessions
- Choose how many questions to study: 5, 10, 15, 20, or All
- Questions are randomly selected for variety
- Smart card count display shows available vs. requested
- Adjusts automatically if requested count exceeds available

## Getting Started

1. Ensure SQL Server is installed and running
2. Build and run the application
3. **Database auto-setup**:
   - Tables are created automatically
   - 85 sample flashcards are added on first run
   - Stacks are auto-created (Animals, Vehicles, Geography, Mathematics, Science)
4. Start managing flashcards or begin studying!

## How to Use

### Creating Flashcards
1. Navigate to **Manage Flashcards**
2. Select a stack (or it will use existing ones)
3. Choose category and difficulty
4. Enter question and answer (no spaces in answer recommended)
5. Click **Add Flashcard**

### Viewing Flashcards
- **Method 1**: Select a flashcard and click **View Details**
- **Method 2**: Double-click any flashcard in the grid
- View shows: ID, Stack, Category, Difficulty, Question, Answer

### Editing Flashcards
1. Select a flashcard from the grid
2. Click **Edit Selected**
3. Form loads with flashcard data
4. Make your changes
5. Click **Update Flashcard** or **Cancel**

### Deleting Flashcards
1. Select a flashcard from the grid
2. Click **Delete Selected**
3. Confirm deletion
4. Flashcard is permanently removed

### Starting a Study Session
1. Navigate to **Study Session**
2. Select a stack from the list
3. (Optional) Apply category and/or difficulty filters
4. Choose number of questions to study
5. Click **Start Study Session**
6. Answer each question and get immediate feedback
7. Review your final score

### Filtering
Use the filter dropdowns to find specific flashcards:
- Filter by Stack
- Filter by Category
- Filter by Difficulty
- Combine filters for precise searches
- Click "Show All" to reset

## Technical Details

- **Framework**: .NET 8 with WPF
- **Database**: SQL Server (LocalDB or SQL Express)
- **Pattern**: Code-behind with data binding
- **UI**: Modern Material Design-inspired interface
- **PDF Generation**: QuestPDF library
- **Features**: Full CRUD operations, filtering, validation, random question selection

## Database Schema

### Stacks Table:
- Id (Primary Key)
- StackName (Unique)

### FlashCards Table:
- Id (Primary Key)
- StackId (Foreign Key to Stacks)
- Front (Question)
- Back (Answer - recommended no spaces)
- Category (e.g., Animals, Mathematics, Countries)
- Difficulty (Easy, Medium, Hard)

### StudySessions Table:
- Id (Primary Key)
- StackId (Foreign Key to Stacks)
- Date (DateTime)
- Month (Integer)
- Score (Integer)

## Sample Data Format

All sample flashcards follow best practices:
- Questions are clear and relevant to category
- Answers have NO spaces (e.g., "VaticanCity" not "Vatican City")
- Difficulty levels are appropriate
- Categories match the content

Examples:
- Question: "What is the largest country by area?" Answer: "Russia"
- Question: "What does SUV stand for?" Answer: "SportUtilityVehicle"
- Question: "What is the chemical symbol for water?" Answer: "H2O"

## Benefits

### For Users
- **Complete control**: Create, view, edit, delete flashcards
- **Visual feedback**: See exactly what's in each flashcard
- **Easy editing**: No need to delete and recreate
- **Validation**: Warns about potential matching issues
- **Sample data**: Start learning immediately with 85 pre-loaded cards
- **Flexible sessions**: Choose session length based on available time

### For Learning
- **Organized by topic**: Categories keep content structured
- **Progressive difficulty**: Easy, Medium, Hard levels
- **Quick review**: View details without editing
- **Safe deletion**: Confirmation prevents accidents
- **Variety**: Random question selection keeps learning fresh

## Database Connection

The application connects to SQL Server:
```
Data Source=DESKTOP-UI42SK7\\SQLEXPRESS;Initial Catalog=Flashcards;Integrated Security=True
```

Update the connection string in these files if needed:
- `DbConnections.cs`
- All page code-behind files in the `Views` folder

## Dependencies

- .NET 8 Windows Desktop Runtime
- SQL Server (Express or LocalDB)
- QuestPDF 2024.3.0 (Community License)
- System.Data.SqlClient

## Tips

1. **Answer Format**: Avoid spaces in answers for easier matching
   - Good: "NewYork", "UnitedStates", "H2O"
   - Problematic: "New York", "United States"

2. **Viewing**: Double-click is fastest way to view flashcard details

3. **Editing**: The form stays in edit mode until you save or cancel

4. **Sample Cards**: Run the app once to get 85 sample flashcards automatically

5. **Categories**: Use the editable combo box to create custom categories

6. **Study Sessions**: Start with 5 questions for quick practice, increase as you improve

## System Requirements

- Windows 10 or later
- .NET 8 Runtime
- SQL Server 2016 or later (Express Edition is sufficient)
- Minimum 4GB RAM
- 100MB free disk space

## Installation

1. Clone the repository
2. Open the solution in Visual Studio 2022 or later
3. Restore NuGet packages
4. Update the connection string in the code files to match your SQL Server instance
5. Build and run the application

## Future Enhancements

- Import/export flashcards (CSV, JSON)
- Bulk edit operations
- Flashcard templates
- Image support for questions/answers
- Audio pronunciation for language cards
- Spaced repetition algorithm
- Mobile companion app
- Cloud synchronization
- Multi-user support

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## Copyright and License

Copyright (c) 2024 Flashcards WPF Application

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Third-Party Licenses

### QuestPDF
This application uses QuestPDF for PDF generation. QuestPDF is licensed under the Community License for non-commercial use. For commercial use, please obtain a commercial license from https://www.questpdf.com/license/.

## Disclaimer

This software is provided for educational purposes. The authors and contributors are not responsible for any data loss or damage that may occur from using this application. Always maintain backups of important data.

## Support

For issues, questions, or contributions, please visit the project repository on GitHub.

---

Built with .NET 8 and WPF
