# English AI - Personalized English Learning Platform

> Personalize the way you learn and practice English with AI

## 📖 Overview

English AI is a comprehensive platform that combines artificial intelligence with language learning to provide personalized English learning experiences. The platform includes features like AI-powered chatbot conversations, dictionary lookups, quizzes, and assignments to help users improve their English skills.

## 🏗️ Architecture

This project consists of two main components:

- **Backend API** (`EngAce/`): .NET 8 Web API providing AI-powered language learning services
- **Frontend** (`english-mentor-buddy/`): React + TypeScript application with modern UI components

## 🚀 Prerequisites

Before running this project, make sure you have the following installed:

### For Backend (.NET API)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Visual Studio 2022 or Visual Studio Code with C# extension

### For Frontend (React App)
- [Node.js](https://nodejs.org/) (version 18 or later)
- [Bun](https://bun.sh/) (recommended) or npm/yar

## 🛠️ Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/HoangDat-3009/english-ai.git
cd english-ai
```

### 2. Backend  (.NET API)

create new .sln file 
dotnet new sln -n EngAce
dotnet sln EngAce.sln add Entities/Entities.csproj
dotnet sln EngAce.sln add Events/Events.csproj
dotnet sln EngAce.sln add Helper/Helper.csproj
dotnet sln EngAce.sln add EngAce.Api/EngAce.Api.csproj



### 3. Frontend  (React App)

create node_modules
->npm i
run fe 
-> npm run dev 


## 📁 Project Structure

```
english-ai/
├── EngAce/                          # Backend (.NET API)
│   ├── EngAce.Api/                  # Main API project
│   │   ├── Controllers/             # API Controllers
│   │   ├── DTO/                     # Data Transfer Objects
│   │   └── Program.cs               # Application entry point
│   ├── Entities/                    # Domain entities
│   ├── Events/                      # Event handling
│   └── Helper/                      # Utility classes
├── english-mentor-buddy/            # Frontend (React + TypeScript)
│   ├── src/
│   │   ├── components/              # Reusable UI components
│   │   ├── pages/                   # Page components
│   │   ├── services/                # API services
│   │   ├── hooks/                   # Custom React hooks
│   │   └── lib/                     # Utility functions
│   └── public/                      # Static assets
└── README.md                        # This file
```

## 🌟 Features

- **AI-Powered Chat**: Interactive conversations with AI to practice English
- **Smart Dictionary**: Advanced dictionary with pronunciation and examples
- **Personalized Quizzes**: AI-generated quizzes based on your learning level
- **Assignment System**: Structured learning assignments
- **Progress Tracking**: Monitor your learning progress
- **Health Monitoring**: Application health checks and monitoring

## 🔧 Configuration

### Backend Configuration

The API uses `appsettings.json` and `appsettings.Development.json` for configuration. Key settings include:

- Application Insights for monitoring
- Logging configuration
- CORS settings
- API endpoints

### Frontend Configuration

The React app uses environment variables for configuration. Create a `.env` file in the `english-mentor-buddy` directory:

```env
VITE_API_BASE_URL=https://localhost:5001
VITE_APP_TITLE=English AI
```

## 🧪 Testing

### Backend Tests

```bash
cd EngAce
dotnet test
```

### Frontend Tests

```bash
cd english-mentor-buddy
bun test  # or npm test
```

## 📦 Building for Production

### Backend

```bash
cd EngAce/EngAce.Api
dotnet publish -c Release -o ./publish
```

### Frontend

```bash
cd english-mentor-buddy
bun run build  # or npm run build
```

The built files will be in the `dist` directory.

## 🚀 Deployment

### Backend Deployment

The API is configured for Azure deployment with Application Insights integration. You can deploy using:

- Azure App Service
- Docker containers
- IIS on Windows Server

### Frontend Deployment

The frontend can be deployed to:

- GitHub Pages (configured): `bun run deploy`
- Vercel
- Netlify
- Azure Static Web Apps

## 🤝 Contributing



1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💻 Author

team project 1 with love

Made with ❤️ for English learners worldwide
