# Prickle

**Prickle** is an interactive web platform for designing florariums (plant terrariums) in the browser. It combines a visual constructor — glass containers, living plants, substrate mixes, and decorative elements — with biology-aware guidance so beginners can avoid common mismatches (light, watering, humidity, and soil expectations). A Generative AI pipeline produces a photorealistic preview from the user’s layout and catalog imagery, while client-side PDF export turns the same project data into a shopping list and layered assembly instructions. The goal is a safer, more predictable path from idea to materials list.

---

## Key Features

- **Interactive Smart Constructor** — A visual, layered workspace where users can design their florariums from scratch by selecting glass containers, living plants, substrate mixes, and decorations from a comprehensive catalog.
- **Biological Compatibility Engine** — Acts as a smart assistant to prevent common beginner mistakes. The system automatically cross-references the selected plants' needs (light, water, humidity) and ensures the ecosystem is viable and aligns with the chosen soil formula.
- **Automated Material Calculations** — Eliminates guesswork by dynamically computing the exact volume of soil components (in liters) needed, based on the specific physical capacity of the chosen container and backend soil recipes.
- **AI-Powered Visualization** — Integrates Generative AI (powered by Gemini via OpenRouter) to create a photorealistic preview of the user's custom layout, allowing them to visualize the final composition before purchasing any materials.
- **One-Click PDF Blueprints** — Generates a comprehensive, downloadable guide directly in the browser. It provides a precise shopping list (including substrate percentages and quantities) along with step-by-step assembly instructions.
- **Community Gallery & Project Management** — Users can securely save their projects, edit them over time, and publish their successful designs to a public gallery to share with the community.
- **Robust Catalog Management** — A structured system for browsing and managing the database of plants, containers, decorations, and precise soil formulas.
- **Secure Access & Developer Tools** — Enterprise-grade authentication and authorization handled by Keycloak, coupled with interactive OpenAPI/Scalar documentation for seamless API exploration during development.

---

## Tech Stack & Technologies

### Frontend

- **React 19 & Vite 7** — Core UI framework and fast build tooling.
- **React Router 7** — Client-side SPA routing.
- **Axios** — HTTP client for API communication.
- **Keycloak JS** — Frontend integration for OpenID Connect authentication.
- **pdfmake & html2canvas** — Client-side PDF generation and canvas capturing for the florarium constructor.

### Backend

- **ASP.NET Core (.NET 10)** — High-performance backend framework utilizing Minimal APIs.
- **Entity Framework Core 10** — Primary Object-Relational Mapper (ORM).
- **Mediator & FluentValidation** — Request routing (CQRS pattern) and robust data validation.
- **Scalar** — Interactive OpenAPI documentation for development.

### Data & Infrastructure

- **.NET Aspire** — Application orchestration, service discovery, and local development environment management.
- **PostgreSQL** — Primary relational database.
- **Keycloak** — Centralized identity and access management (IAM) provider.
- **Redis** — Provisioned via Aspire for orchestration-ready caching and messaging.
- **OpenTelemetry** — Distributed tracing and metrics collection.

### AI integration

- **OpenRouter API** — Gateway for routing multimodal generative AI requests.
- **Google Gemini** — The underlying model used for photorealistic florarium preview generation.

### Testing

- **xUnit v3** — Core framework for unit testing.
- **NSubstitute & Bogus** — Libraries for mocking dependencies and generating robust fake test data.
- **Aspire Hosting Testing** — Framework for integration testing against real HTTP endpoints.

---

## System Architecture

- **System Orchestration** — The application relies on .NET Aspire to manage the entire development and hosting environment. It automatically provisions the PostgreSQL database, Keycloak identity server, backend API, and Vite frontend, while seamlessly handling service discovery and secure configuration injection without manual setup.
- **Client-Server Communication** — The React single-page application communicates with the backend through a RESTful API. Secure access is maintained by attaching JSON Web Tokens, provided by the Keycloak identity provider, to all protected requests.
- **Backend API Layer** — The ASP.NET Core backend exposes domain-driven endpoints protected by role-based authorization policies, strictly distinguishing between standard users and administrators. It also manages database integrity and schema updates automatically.
- **Client-Side Processing** — To optimize performance and minimize unnecessary server round-trips, significant business logic is executed directly in the browser. The frontend application independently handles biological compatibility evaluations, dynamic soil volume calculations, and the complete rendering of PDF blueprints using data aggregated from standard REST endpoints.
- **Asynchronous AI Pipeline** — Image generation requests are processed using a queue-based background worker architecture. When a user requests an AI preview, the API immediately acknowledges the request and offloads the actual external communication to a background service. This decouples the latency of generative models from the main web thread, allowing the UI to remain responsive and poll for updates.

---

## Development Process & Engineering Practices

- **Modular Solution Architecture** — The project is organized using a modern structure that cleanly separates the hosting orchestration, backend layers, shared components, and test suites.
- **Clean Architecture Principles** — The backend strictly adheres to a clear separation of concerns, isolating core domain logic and application use cases from external infrastructure, databases, and the API layer.
- **Centralized Quality Management** — Dependency versions are managed centrally to guarantee build consistency. Static analysis tools and code style guardrails are enforced across the entire solution to maintain high engineering standards.
- **Automated Integration Testing** — The platform utilizes orchestration-aware testing environments to validate real HTTP endpoints, ensuring reliable interactions between the application and its underlying data stores.
- **Disciplined Version Control** — The team followed a strict Git workflow to maintain code stability. Every new feature, bug fix, or architectural change was developed in an isolated feature branch.
- **Mandatory Code Review** — All changes were integrated into the main branch strictly via Pull Requests. To ensure shared code ownership and prevent bugs, every Pull Request required a mandatory peer review and explicit approval from at least one other team member before merging.

---

## Prerequisites

- **.NET SDK 10 or higher** — Required for compiling and running the backend and orchestration layers.
- **.NET Aspire CLI** — The primary tool used to launch the entire solution.
- **Docker Desktop** — .NET Aspire relies on a local container runtime to automatically provision and orchestrate dependencies.
- **Node.js 20 or higher** — .NET Aspire utilizes the local Node environment to execute the Vite development server.
- **OpenRouter Account** — An active API key is required to utilize the generative AI visualization features.

---

## Usage Guide

### Installation

To get started, clone the repository to your local machine.

```bash
git clone https://github.com/ProjectsOfOurSuperTeam/Prickle
cd Prickle
```

### Run

Ensure Docker Desktop is running. Because the project utilizes .NET Aspire for complete local orchestration, you do not need to manually restore backend dependencies, install Node packages, or start the components separately. A single command will automatically build the solution, provision the required database and identity containers, and launch all associated services:

```bash
aspire run
```

When the application starts, .NET Aspire will open its orchestration dashboard in your browser. Behind the scenes, it automatically spins up Docker containers for PostgreSQL, pgAdmin, Redis, and Keycloak. The backend API applies any pending database migrations automatically, and both the API and Vite frontend are launched and securely wired together using service discovery.

From the Aspire dashboard, you can easily navigate to all essential services using the provided endpoint links:
- **The Main Application** — Access the fully functional React frontend.
- **Interactive API Docs** — Open the Scalar endpoint to explore and test the REST API.
- **Identity Management** — Access the Keycloak administration console.
- **Database Management** — Access pgAdmin to inspect the PostgreSQL database directly.
### Required Secrets

On the first launch, the Aspire AppHost may prompt you to securely enter the following parameters in your terminal:

- `GEMINI_API_KEY` — Your OpenRouter API key, required to enable the Generative AI florarium visualization feature.
- `KeycloakAdminPassword` — An initial administrator password for the local Keycloak identity instance.

These values are safely stored in local .NET User Secrets and are never committed to the repository.

### Typical User Journey

If you are evaluating the application, we recommend following this path to experience the core business logic:

1. **Authenticate** — Register or sign in via the secure Keycloak flow.
2. **Explore** — Browse the comprehensive catalog of plants, glass containers, and decorations.
3. **Design** — Open the Constructor workspace, mix and match items, and review the live biological compatibility feedback.
4. **Visualize** — Save your project and trigger the Generative AI integration to see a photorealistic preview of your florarium.
5. **Export** — Generate and download your personalized PDF blueprint, complete with precise soil calculations and assembly steps.

### Running Tests

To verify the system's integrity, you can run the automated unit and integration tests across the entire solution using the .NET CLI:

```bash
dotnet test Prickle.slnx
```

---

## Team & Contributors

| Name                  | Role | Key Contributions                                                                                                                  |
|-----------------------| --- |------------------------------------------------------------------------------------------------------------------------------------|
| **Andrii Shandryk**   | **Backend Lead** | Architected the ASP.NET Core API, configured .NET Aspire orchestration, and implemented Entity Framework Core data access.         |
| **Anastasiia Tuhai**  | **UI/UX Developer** | Designed the visual language, developed reusable React components, and ensured a responsive, accessible user interface.            |
| **Denys Shvachka**    | **Frontend Developer** | Developed the interactive Florarium Constructor UI, including drag-and-drop layering and state handling for canvas capturing.      |
| **Rinat Rafikov**     | **Frontend Integrations** | Implemented Keycloak JS for secure authentication and built the asynchronous polling UI for the GenAI visualization pipeline.      |
| **Ihor Kharyshyn**    | **Frontend Core Logic** | Programmed the client-side biological compatibility engine, dynamic soil calculations, and the automated PDF blueprint export.     |
| **Vladyslav Hibskyi** | **Frontend Architecture** |Bootstrapped the Vite/React 19 environment, configured React Router 7, and established core client-side state management patterns. |

National University of "Kyiv-Mohyla Academy", Faculty of Computer Sciences, 2026, Structure of software projects.
