<div align="center">

# 🌾 AgriTech IoT Platform

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3.12-FF6600?logo=rabbitmq)](https://www.rabbitmq.com/)
[![Docker](https://img.shields.io/badge/Docker-24.0-2496ED?logo=docker)](https://www.docker.com/)
[![Azure](https://img.shields.io/badge/Azure-0078D4?logo=microsoftazure)](https://azure.microsoft.com/)

<!-- ========== 状态徽章 ========== -->
[![CI Build & Test](https://github.com/yourusername/AgriTech/actions/workflows/ci.yml/badge.svg)](https://github.com/yourusername/AgriTech/actions/workflows/ci.yml)
[![Code Coverage](https://img.shields.io/badge/coverage-85%25-brightgreen)](https://github.com/yourusername/AgriTech/actions)
[![License](https://img.shields.io/badge/license-MIT-blue)](LICENSE)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen)](.github/pull_request_template.md)

</div>

---

## 📖 Table of Contents

- [🌾 AgriTech IoT Platform](#-agritech-iot-platform)
  - [📖 Table of Contents](#-table-of-contents)
  - [📋 Overview](#-overview)
  - [🏗️ Architecture](#️-architecture)
  - [🚀 Quick Start](#-quick-start)
    - [Option 1: Docker Compose (Recommended)](#option-1-docker-compose-recommended)
    - [Option 2: Local Development](#option-2-local-development)
  - [📊 Database Schema](#-database-schema)
  - [🧪 Testing](#-testing)
  - [📂 Project Structure](#-project-structure)
  - [🔧 Configuration](#-configuration)
    - [Environment Variables](#environment-variables)
    - [Configuration Files](#configuration-files)
  - [📈 CI/CD Pipeline](#-cicd-pipeline)
  - [📝 API Documentation](#-api-documentation)
  - [🛠️ Technology Stack](#️-technology-stack)
  - [🤝 Contributing](#-contributing)
  - [📄 License](#-license)

---

## 📋 Overview

AgriTech is an **IoT platform for smart agriculture** that collects, processes, and analyzes sensor data from farms. It uses a clean architecture with Domain-Driven Design (DDD) principles, event-driven communication via RabbitMQ, and PostgreSQL for persistence.

### Key Features

- ✅ **Sensor Management**: Register, activate, and monitor IoT sensors
- ✅ **Real-time Data Processing**: Process sensor readings with event-driven architecture
- ✅ **Intelligent Alerting**: Multi-strategy alert engine (threshold, rate-of-change, consecutive)
- ✅ **Batch Processing**: Efficient bulk data processing with configurable batch sizes
- ✅ **Clean Architecture**: Domain, Application, Infrastructure, and Presentation layers
- ✅ **Health Checks**: Built-in health monitoring for all services
- ✅ **Docker Ready**: Full containerization with Docker Compose
- ✅ **CI/CD Pipeline**: Automated build, test, and deployment with GitHub Actions

---

## 🏗️ Architecture
┌─────────────────────────────────────────────────────────────────────────────┐
│ Presentation Layer │
│ (WebAPI / Swagger UI) │
└─────────────────────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ Application Layer │
│ (Commands, Queries, Handlers, Events, Validators) │
└─────────────────────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ Domain Layer │
│ (Entities, Value Objects, Domain Events, Interfaces) │
└─────────────────────────────────────────────────────────────────────────────┘
│
▼
┌─────────────────────────────────────────────────────────────────────────────┐
│ Infrastructure Layer │
│ (Persistence, Messaging, Repositories, Unit of Work) │
└─────────────────────────────────────────────────────────────────────────────┘


### Event Flow
┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────┐
│ Sensor │────▶│ WebAPI │────▶│ RabbitMQ │────▶│ Worker │
│ Device │ │ │ │ │ │ │
└──────────┘ └──────────┘ └──────────┘ └──────────┘
│
▼
┌──────────┐
│PostgreSQL│
│ │
└──────────┘


---

## 🚀 Quick Start

### Option 1: Docker Compose (Recommended)

```bash
# 1. Clone the repository
git clone https://github.com/yourusername/AgriTech.git
cd AgriTech

# 2. Copy environment configuration
cp .env.example .env
# Edit .env with your configuration (or use defaults for local development)

# 3. Start all services
docker-compose up -d

# 4. Verify services are running
docker-compose ps

# 5. Access the API
curl http://localhost:5000/health
# Open browser: http://localhost:5000/swagger
```
## Database Schema
```bash
-- Core tables
Farms
  ├── Id (UUID) PK
  ├── Name (VARCHAR)
  ├── Location (PostGIS Point)
  ├── Acreage (DECIMAL)
  ├── CreatedAt (TIMESTAMP)
  ├── UpdatedAt (TIMESTAMP)
  └── IsDeleted (BOOLEAN)

Sensors
  ├── Id (UUID) PK
  ├── FarmId (UUID) FK → Farms.Id
  ├── Name (VARCHAR)
  ├── TemperatureThreshold (DECIMAL)
  ├── Location (PostGIS Point)
  ├── Status (ENUM: Active/Inactive)
  ├── CreatedAt (TIMESTAMP)
  ├── UpdatedAt (TIMESTAMP)
  └── IsDeleted (BOOLEAN)

Readings
  ├── Id (UUID) PK
  ├── SensorId (UUID) FK → Sensors.Id
  ├── Temperature (DECIMAL)
  ├── Humidity (DECIMAL)
  ├── Timestamp (TIMESTAMP)
  └── CreatedAt (TIMESTAMP)
```

## Testing
```bash
# Run all tests
dotnet test

# Run unit tests only
dotnet test --filter "Category=Unit"

# Run integration tests only
dotnet test --filter "Category=Integration"

# Run with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate coverage report
dotnet tool install --global dotnet-reportgenerator-globaltool
reportgenerator -reports:"./TestResults/**/coverage.opencover.xml" -targetdir:"./CoverageReport"
```

## Project Structure
```bash
AgriTech/
├── .github/
│   ├── workflows/
│   │   └── ci.yml                 # GitHub Actions CI pipeline
│   └── pull_request_template.md   # PR template
├── src/
│   ├── Domain/                    # Domain Layer
│   │   ├── Entities/              # Domain entities
│   │   ├── ValueObjects/          # Value objects
│   │   ├── Events/                # Domain events
│   │   └── Common/                # Common domain interfaces
│   ├── Application/               # Application Layer
│   │   ├── Features/              # Features organized by bounded context
│   │   ├── Handlers/              # Event handlers
│   │   └── Common/                # Common application interfaces
│   ├── Infrastructure/            # Infrastructure Layer
│   │   ├── Persistence/           # PostgreSQL / EF Core
│   │   │   ├── Configurations/    # Entity configurations
│   │   │   ├── Repositories/      # Repository implementations
│   │   │   └── Migrations/        # EF Core migrations
│   │   └── Messaging/             # RabbitMQ / MassTransit
│   │       └── Consumers/         # Message consumers
│   ├── WebAPI/                    # Presentation Layer
│   │   ├── Controllers/           # API endpoints
│   │   ├── Models/                # Request/Response DTOs
│   │   ├── Filters/               # Action filters
│   │   ├── Exceptions/            # Exception handlers
│   │   └── Dockerfile             # WebAPI container
│   └── AgriTech.Worker/           # Background Worker
│       ├── Consumers/             # Message consumers
│       ├── Health/                # Health checks
│       ├── Infrastructure/        # Worker-specific infrastructure
│       └── Dockerfile             # Worker container
├── tests/
│   ├── AgriTech.Domain.UnitTests/
│   ├── AgriTech.Application.UnitTests/
│   └── AgriTech.IntegrationTests/
├── docker-compose.yml             # Multi-container orchestration
├── .env.example                   # Environment variables template
├── .gitignore                     # Git ignore rules
├── AgriTech.sln                   # Solution file
└── README.md                      # This file
```
[![CI Build & Test](https://github.com/Luna-81/AgriTech/actions/workflows/ci.yml/badge.svg)](https://github.com/Luna-81/AgriTech/actions/workflows/ci.yml)
[![Code Coverage](https://img.shields.io/badge/coverage-85%25-brightgreen)](https://github.com/Luna-81/AgriTech/actions)
