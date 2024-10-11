# TalentSpot Project

## Overview
The TalentSpot project is a job listing application designed for employers to register, publish job listings, and view them. The application includes features for managing users, job posts, work types, benefits, and forbidden words.

## Technologies Used
- **Backend:** .NET 6, C#
- **Database:** PostgreSQL
- **Caching:** Redis
- **Containerization:** Docker
- **Testing:** xUnit
- **Authentication:** JWT

## Getting Started

### Prerequisites
- .NET 6 SDK
- Docker
- PostgreSQL

### Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/talentspot.git
   cd talentspot
Configure Docker Ensure you have Docker installed and running. Then, create and start the PostgreSQL container:

bash
Copy code
docker-compose up -d
Update Connection Strings Open appsettings.json in the TalentSpot.API project and ensure the connection string points to your PostgreSQL instance:

json
Copy code
"ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=talentspot;Username=postgres;Password=postgres"
}
Apply Migrations Run the following command to apply migrations and seed data to the database:

bash
Copy code
dotnet ef database update --project TalentSpot.Infrastructure/TalentSpot.Infrastructure.csproj --startup-project TalentSpot.API/TalentSpot.API.csproj
Run the Application Start the application:

bash
Copy code
dotnet run --project TalentSpot.API/TalentSpot.API.csproj
Postman Collection
A Postman collection for testing the API endpoints is included in the repository. You can import it into Postman to test the application easily.

API Endpoints
User Registration: POST /api/users/register
User Login: POST /api/users/login
Create Job: POST /api/jobs
Get Jobs: GET /api/jobs
Update Job: PUT /api/jobs/{id}
Delete Job: DELETE /api/jobs/{id}
Work Types: Manage work types with the respective endpoints.
Benefits: Manage benefits with the respective endpoints.
Forbidden Words: Manage forbidden words with the respective endpoints.
Contributing
Contributions are welcome! Please create a pull request with a description of your changes.

License
This project is licensed under the MIT License.