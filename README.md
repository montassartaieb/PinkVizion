# 🩺 PinkVision - Plateforme E-Santé Intelligente

## 📋 Description

PinkVision est une plateforme e-santé intelligente destinée à l'aide au diagnostic du cancer du sein à partir de mammographies. Elle combine un backend microservices .NET 8, un frontend Angular 17+, et des services IA FastAPI.

## 🏗️ Architecture

```
PinkVision/
├── src/
│   ├── ApiGateway/           # YARP Gateway
│   ├── Services/
│   │   ├── Auth/             # Service d'authentification JWT
│   │   ├── Patient/          # Gestion des patients
│   │   ├── Doctor/           # Gestion des médecins
│   │   ├── Imaging/          # Upload et analyse mammographies
│   │   ├── MedicalRecord/    # Dossiers médicaux
│   │   ├── Appointment/      # Rendez-vous médicaux
│   │   ├── Notification/     # Notifications email/in-app
│   │   └── Dashboard/        # Statistiques et rapports
│   └── Shared/               # Librairies partagées
├── frontend/                 # Angular 17+ SPA
├── ia-service/               # Services IA FastAPI (existants)
│   ├── mammographic-app/     # Classification + Heatmap
│   └── chatbot_pinkvision/   # Chatbot médical
├── docker/                   # Configurations Docker
└── docs/                     # Documentation
```

## 🛠️ Stack Technique

| Couche | Technologie |
|--------|-------------|
| Frontend | Angular 17+ |
| Backend | ASP.NET Core .NET 8 |
| API Gateway | YARP |
| Auth | JWT natif (sans Keycloak) |
| IA | FastAPI + PyTorch (existant) |
| DB Relationnelle | PostgreSQL 15 |
| DB Documents | MongoDB 7 |
| Message Broker | RabbitMQ |
| Conteneurs | Docker + Docker Compose |
| Monitoring | Prometheus + Grafana |

## 🚀 Démarrage Rapide

### Prérequis
- Docker & Docker Compose
- .NET 8 SDK
- Node.js 18+ & npm
- Python 3.11 (pour les services IA)

### Lancement avec Docker Compose

```bash
# Démarrer tous les services
docker-compose up -d

# Vérifier les logs
docker-compose logs -f
```

### URLs des Services

| Service | URL |
|---------|-----|
| API Gateway | http://localhost:5000 |
| Frontend Angular | http://localhost:4200 |
| IA Mammographie | http://localhost:8001 |
| Chatbot | http://localhost:8002 |
| RabbitMQ Management | http://localhost:15672 |
| Grafana | http://localhost:3000 |
| Prometheus | http://localhost:9090 |

## 👥 Rôles Utilisateurs

- **ADMIN**: Gestion globale de la plateforme
- **MEDECIN**: Analyse des mammographies, gestion des patients
- **PATIENT**: Consultation de son dossier, prise de RDV

## 📖 Documentation

- [Roadmap Technique](docs/01-ROADMAP.md)
- [Contrats API](docs/02-API-CONTRACTS.md)
- [Schémas de Données](docs/03-DATA-SCHEMAS.md)
- [Sécurité](docs/04-SECURITY.md)
- [Plan de Tests](docs/05-TESTING.md)

## 📜 License

Projet académique - 5ème année SESAME
