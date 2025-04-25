![Banner](/banner.png)

# Janus - Distributed Version Control System
Janus is a secure, self-hosted Distributed Version Control System (DVCS) designed for enterprises. It eliminates reliance on external cloud services, allowing organisations to maintain full control over sensitive codebases within internal networks. Built with a Command Line Interface (CLI) and a Dockerised web interface, Janus prioritizes security, extensibility, and compliance.

### Key Features
- On-Premise Deployment: Host repositories internally for GDPR/Data Protection Act compliance.
- Plugin Architecture: Extend functionality with custom plugins.
- Granular Access Control: Native RBAC and revocable Personal Access Tokens (PATs).
- Dockerised Microservices: Containerised frontend, backend, and database.
- Cross-Platform CLI: Fast, intuitive command-line tool for Windows, macOS, and Linux.
- Audit Logging: Comprehensive logs for compliance and traceability.


## Technologies

### Dockerised Architecture

- Frontend: A responsive, user-friendly web interface providing secure access to repository data and management tools.
- Backend: RESTful endpoints supporting CLI and web interactions, handling authentication, audit logging, and data processing.
- Database: Secure storage of user accounts, repository metadata, and audit logs.

### Local CLI

- The Command Line Interface (CLI) is the core of Janus’s local operations and supports essential DVCS workflows:
- Repository initialization (creates a hidden .janus/ directory for version control).
- File staging and commit history logging.
- Branch management, merging, and conflict detection.
- Pushing and pulling changes to/from the internal remote repository.

### Plugin Framework

- The plugin architecture enabling developers to add custom commandsto the CLI.
- Adapt Janus to evolving enterprise workflows without modifying core code.

### Component Architecture

An overview of how the parts interact:
![System Architecure Diagram](/Documentation/Diagrams/System_Architecture_Diagram.png)

## Video
[![Janus Video](https://img.youtube.com/vi/VIDEO_ID/0.jpg)](https://youtu.be/VIDEO_ID)

## Installation

### Prerequisites
- Docker Engine 20.10+
- Docker Compose 2.15+

### Docker Setup
```bash
git clone https://github.com/benjaminsanderswyatt/COMP3000-JanusVersionControl.git
cd COMP3000-JanusVersionControl/Fullstack

# Build and start containers
./up-internal.sh
```

### CLI Installation
1. Download the latest release from the releases page.
2. Follow the installaton instructions inside the downloaded package.


### Documentation
[CLI Command Reference](/Documentation/CLI_DOCUMENTATION.md)

[Plugin Developent Guide](/Documentation/PLUGIN_DEVELOPMENT_GUIDE.md)

[Endpoints](/Documentation/API_ENDPOINTS.md)

[Project Poster](/Documentation/Poster/10808929.jpg)

[Project Report](/Documentation/Report/Report.pdf)


### License
This project is developed for academic purposes at the University of Plymouth. All rights reserved unless otherwise specified in component licenses.
