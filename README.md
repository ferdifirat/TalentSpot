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
- .NET 8 SDK
- Docker
- PostgreSQL

### Setup

1. **Clone the Repository**
   ```bash
   git clone https://github.com/yourusername/talentspot.git
   cd talentspot
2. **Configure Docker** 
Ensure you have Docker installed and running. Then, create and start the PostgreSQL container:
   ```bash
   docker-compose up -d