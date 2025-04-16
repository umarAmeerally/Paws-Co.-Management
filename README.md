# Paws-Co.-Management
year 2 software engineering module coursework

# Getting started
Clone the repo
Make sure to choose the **Master** branch as it contains updated codes.
Restore dependencies (Packages) and build
Run the project file.

# Storage mode
When the app starts you will be prompted: 
Choose data source:
📄 Load data from CSV - • CSV mode – Ideal for a quick demo. Default path: C:/Users//Desktop/pet_management_data.csv (or supply your own).
🗄️ Load data from Database - • Database mode – Reads from the PetCare SQL Server database configured in PetCareContext. 
⚠ Delete operations are disabled in CSV mode to keep your original file intact.

# Database Setup (optional)
Create an empty database, e.g. updateddbms.
Update the connection string in PetCareContext.cs and program.cs or export DOTNET_CONNECTION_STRING as an environment variable.
Run the EF Core migrations (none are included yet – use dotnet ef migrations add InitialCreate).
Launch the app and pick Database mode – sample data will be loaded automatically.

# Using the App
The main menu is fully keyboard driven. Use ↑/↓ to move, Enter to select. Action:
Display lists 
Add new entity 
Edit entity 
Delete entity - (DB mode only) 
Search – full text across ID / Name / Email etc. 
Save all data - bulk insert/update everything into SQL Server
Exit

# Check on SQL
Enter the following prompt to conduct a check on the SQL:
Select * from Owners/Pets/Appointments.

# Note: The recent ones will always be in the last row.
Hope you enjoy our work!!
