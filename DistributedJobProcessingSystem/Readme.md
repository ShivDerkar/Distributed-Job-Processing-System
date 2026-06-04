# Distributed Job Processing System

A distributed background job processing platform built with C#, ASP.NET Core, PostgreSQL, Redis, Docker, and .NET Worker Services.

## Project Goal

The system allows users to create background jobs through an API. Jobs are stored in PostgreSQL, queued in Redis, and processed asynchronously by independent worker services.

## Architecture

User request flows through the system like this:

1. User creates a job using the API.
2. API stores the job in PostgreSQL.
3. API pushes the job ID into Redis.
4. Worker service picks the job ID from Redis.
5. Worker processes the job.
6. Worker updates job status and logs in PostgreSQL.

## Tech Stack

- C#
- ASP.NET Core
- PostgreSQL
- Redis
- Docker
- Docker Compose
- Entity Framework Core
- .NET Worker Service
- React

## Planned Features

- User registration and login
- JWT authentication
- Create jobs
- View job status
- Cancel jobs
- Job logs
- Redis job queue
- Multiple workers
- Retry mechanism
- Failed job tracking
- Worker heartbeat
- Admin dashboard
- Dockerized setup