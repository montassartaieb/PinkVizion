# 🗺️ ROADMAP TECHNIQUE - PINKVISION

## 📋 Vue d'ensemble

PinkVision est une plateforme e-santé intelligente pour le diagnostic assisté du cancer du sein.

**Approche :** Frontend Angular + Backend .NET d'abord, puis intégration IA (FastAPI) plus tard.

---

## 🎯 Phase 1 : Fondations (Semaines 1-3)

### Sprint 1.1 - Infrastructure & Auth (Semaine 1)
- [ ] Configuration Docker Compose
- [ ] Setup Keycloak pour l'authentification
- [ ] Création du projet Angular (Frontend)
- [ ] Création des solutions .NET (Backend)
- [ ] Configuration PostgreSQL & MongoDB
- [ ] Setup API Gateway (Ocelot ou YARP)

### Sprint 1.2 - Auth Service (Semaine 2)
- [ ] Endpoints d'inscription (Patient, Médecin)
- [ ] Endpoints de connexion (JWT via Keycloak)
- [ ] Gestion des rôles (Admin, Médecin, Patient)
- [ ] Middleware d'autorisation
- [ ] Tests unitaires Auth

### Sprint 1.3 - Patient Service (Semaine 3)
- [ ] CRUD profil patient
- [ ] Historique médical basique
- [ ] Liaison avec Auth Service
- [ ] Tests d'intégration

---

## 🎯 Phase 2 : Core Features (Semaines 4-6)

### Sprint 2.1 - Imaging Service (Semaine 4)
- [ ] Upload d'images mammographie
- [ ] Stockage sécurisé (Azure Blob / MinIO)
- [ ] Métadonnées images
- [ ] Validation des formats (DICOM, PNG, JPG)
- [ ] **Mock IA endpoint** (retourne résultat simulé)

### Sprint 2.2 - Medical Record Service (Semaine 5)
- [ ] Création/modification dossiers médicaux
- [ ] Historique des analyses
- [ ] Notes du médecin
- [ ] Liaison Patient ↔ Médecin

### Sprint 2.3 - Appointment Service (Semaine 6)
- [ ] CRUD rendez-vous
- [ ] Planning médecin
- [ ] Disponibilités
- [ ] Confirmation/annulation

---

## 🎯 Phase 3 : Notifications & Dashboard (Semaines 7-8)

### Sprint 3.1 - Notification Service (Semaine 7)
- [ ] Notifications email (SMTP)
- [ ] Notifications in-app (WebSocket/SignalR)
- [ ] Alertes résultats disponibles
- [ ] Rappels rendez-vous

### Sprint 3.2 - Dashboard Service (Semaine 8)
- [ ] Statistiques globales
- [ ] Indicateurs par médecin
- [ ] Visualisation des tendances
- [ ] Export rapports

---

## 🎯 Phase 4 : Frontend Angular (Semaines 9-11)

### Sprint 4.1 - Pages Auth & Navigation (Semaine 9)
- [ ] Login / Register
- [ ] Layout principal
- [ ] Routing avec guards
- [ ] Intercepteurs HTTP

### Sprint 4.2 - Interfaces Patient (Semaine 10)
- [ ] Dashboard patient
- [ ] Upload mammographie
- [ ] Historique analyses
- [ ] Prise de rendez-vous

### Sprint 4.3 - Interfaces Médecin (Semaine 11)
- [ ] Liste patients
- [ ] Analyse images (avec mock IA)
- [ ] Validation diagnostics
- [ ] Gestion planning

---

## 🎯 Phase 5 : Intégration IA (Semaines 12-14)

### Sprint 5.1 - AI Diagnosis Service (Semaine 12)
- [ ] Setup FastAPI
- [ ] Chargement modèle PyTorch (CNN)
- [ ] Endpoint /predict
- [ ] Génération heatmap (Grad-CAM)

### Sprint 5.2 - Chatbot Service (Semaine 13)
- [ ] Setup RAG avec FAISS
- [ ] Intégration LLM
- [ ] Endpoints conversation
- [ ] Base de connaissances médicales

### Sprint 5.3 - Intégration Finale (Semaine 14)
- [ ] Connexion AI Service ↔ Backend .NET
- [ ] Tests end-to-end
- [ ] Optimisation performances
- [ ] Documentation API

---

## 🎯 Phase 6 : Production Ready (Semaines 15-16)

### Sprint 6.1 - Sécurité & Conformité
- [ ] Audit sécurité
- [ ] Chiffrement données sensibles
- [ ] Logs d'audit
- [ ] Conformité RGPD

### Sprint 6.2 - Monitoring & Déploiement
- [ ] Setup Prometheus/Grafana
- [ ] Alertes automatiques
- [ ] CI/CD pipeline
- [ ] Documentation déploiement

---

## 📊 Jalons Clés

| Jalon | Date | Description |
|-------|------|-------------|
| M1 | Semaine 3 | Auth + Patient opérationnels |
| M2 | Semaine 6 | Core backend complet |
| M3 | Semaine 8 | Notifications + Dashboard |
| M4 | Semaine 11 | Frontend Angular complet |
| M5 | Semaine 14 | IA intégrée |
| M6 | Semaine 16 | Production ready |

---

## 🛠️ Stack Technique Confirmée

| Couche | Technologie |
|--------|-------------|
| Frontend | Angular 17+ |
| Backend | .NET 8 (ASP.NET Core) |
| IA | Python 3.11 + FastAPI + PyTorch |
| Auth | Keycloak |
| DB Relationnelle | PostgreSQL 15 |
| DB Documents | MongoDB 7 |
| Vector DB | FAISS |
| Message Queue | RabbitMQ |
| API Gateway | YARP / Ocelot |
| Conteneurs | Docker + Docker Compose |
| Monitoring | Prometheus + Grafana |
