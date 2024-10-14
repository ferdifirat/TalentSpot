# TalentSpot Project

## Overview
The TalentSpot project is a job listing application designed for employers to register, publish job listings, and view them. The application includes features for managing users, job posts, work types, benefits, and forbidden words.

## Technologies Used
- **Backend:** .NET 8, C#
- **Database:** PostgreSQL
- **Caching:** Redis
- **Containerization:** Docker
- **Testing:** xUnit
- **Authentication:** JWT

## Getting Started

### Prerequisites
Before you begin, ensure you have the following installed:
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/)

### Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/ferdifirat/TalentSpot.git
   cd TalentSpot` 

2.  **Database Migrations** To apply database migrations, run the following command:

>      dotnet ef database update --project TalentSpot.Infrastructure/TalentSpot.Infrastructure.csproj
>     --startup-project TalentSpot.API/TalentSpot.API.csproj`

    
3.  **Configure Docker**  
    To run the entire application using Docker, ensure you have Docker installed and then follow these steps:
    
    -   Navigate to the project root where your `docker-compose.yml` file is located.
    -   Run the following command to start up the services:
    
    `docker-compose up -d` 

    

### Docker Services

-   **API**: Available at [http://localhost:8081](http://localhost:8081)
-   **PostgreSQL**: Database service, available at `localhost:5432`
-   **Redis**: Cache service, available at `localhost:6379`
-   **Elasticsearch**: Full-text search engine, available at `localhost:9200`

### Database Connection String

The default connection string for PostgreSQL is configured in `appsettings.json` as follows:
`"DefaultConnection": "Host=postgres;Database=talentspot1;Username=postgres;Password=postgres"`

## Testing

Unit tests are handled with xUnit. You can run tests using the following command:

    `dotnet test` 

## Postman Collections

The Postman collection for testing the API endpoints is located in the `TalentSpot.Postman-Collection` folder. Import this collection into Postman to test the available API routes.

