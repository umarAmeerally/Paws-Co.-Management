# Paws-Co.-Management
year 2 software engineering module coursework

# Getting started
1. Clone the repo
2. Make sure to choose the **Master** branch as it contains updated codes.
3. Restore dependencies (Packages) and build
4. Run the project file.

# Storage mode
When the app starts you will be prompted:
Choose data source:
  📄  Load data from CSV
  🗄️  Load data from Database
•	CSV mode – Ideal for a quick demo.
Default path: C:/Users/<you>/Desktop/pet_management_data.csv (or supply your own).
•	Database mode – Reads from the PetCare SQL Server database configured in PetCareContext.
⚠ Delete operations are disabled in CSV mode to keep your original file intact.

# Database Setup (optional)
1.	Create an empty database, e.g. updateddbms.
2.	Update the connection string in PetCareContext.cs and program.cs or export DOTNET_CONNECTION_STRING as an environment variable.
3.	Run the EF Core migrations (none are included yet – use dotnet ef migrations add InitialCreate).
4.	Launch the app and pick Database mode – sample data will be loaded automatically.

# Using the App
The main menu is fully keyboard driven. Use ↑/↓ to move, Enter to select.
Action	
Display lists	
Add new entity	
Edit entity	
Delete entity	- (DB mode only)
Search – full text across ID / Name / Email etc.
Save all data	- bulk insert/update everything into SQL Server
Exit

# Check on SQL
Enter the following prompt to conduct a check on the SQL: 
Select * from Owners/Pets/Appointments. 
# Note: The recent ones will always be in the last row.

# Hope you enjoy our work!!



