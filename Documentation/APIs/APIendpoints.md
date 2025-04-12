# API Documentation

## CLI APIs
*Authenticated with `CLIPolicy`, Rate Limited with `CLIRateLimit`*

---

### AccessTokenController
**Base Route**: `api/AccessToken`

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/RevokePAT` | POST | Revoke a PAT (uses `Authorization` header) | Header: `Bearer <PAT>` | `200 OK` or `400 BadRequest` |
| `/Authenticate`| POST | Validate PAT and get username | Body: `{ Email: string }` | `200 OK` with username or `401` |

---

### RepoController (CLI)
**Base Route**: `api/cli/Repo`

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/janus/{owner}/{repoName}` | GET | Clone repository metadata | `owner`, `repoName` | `200 OK` with `RepoDto` or `404` |
| `/batchfiles/{owner}/{repoName}` | POST | Batch fetch file contents by hashes | Body: `List<string> fileHashes` | Multipart response with files |
| `/janus/{owner}/{repoName}/fetch` | POST | Fetch new commits since last sync | Body: `Dictionary<string, string> latestBranchHashes` | `200 OK` with updated `RepoDto` |
| `/janus/{owner}/{repoName}/head` | GET | Get latest commit hashes for all branches | - | `200 OK` with `RemoteHeadDto` |
| `/janus/{owner}/{repoName}/push` | POST | Push commits/files to repository (multipart) | Form: Metadata (JSON) + files | `200 OK` or error |

---

## Frontend APIs
*Authenticated with `FrontendPolicy`, Rate Limited with `FrontendRateLimit`*

---

### AccountController
**Base Route**: `api/web/Account`

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/ChangeProfilePicture` | POST | Upload profile picture | Form: `image` file | `200 OK` with URL or `400` |
| `/Delete` | DELETE | Delete user account | - | `200 OK` |

---

### CommitController
**Base Route**: `api/web/Commit`

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/{owner}/{repoName}/{branch}/commits`| GET | Get paginated commit | `startHash`, `limit=20` | `200 OK` with commits + next cursor |

---

### ContributorsController
**Base Route**: `api/web/Contributors`

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/{owner}/{repoName}/contributors`| GET | List collaborators | - | `200 OK` with contributor list |
| `/{owner}/{repoName}/invite` | POST | Invite collaborator | Body: `{ InviteeUsername, AccessLevel }` | `200 OK` or `403 Forbidden` |
| `/{owner}/{repoName}/leave` | DELETE | Leave repository | - | `200 OK` |

---

### DiscoverController
**Base Route**: `api/web/Discover**

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/Repositories` | GET | Discover public repos (weekly randomized) | `page=1` | `200 OK` with paginated repos |

---

### RepoController (Frontend)
**Base Route**: `api/web/Repo**

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/Init` | POST | Create new repo | Body: `{ RepoName, RepoDescription, IsPrivate }`| `200 OK` or `400` |
| `/file/{owner}/{repoName}/{fileHash}` | GET | Get file content | `fileHash` | `200 OK` with file bytes or `404` |
| `/repository-list` | GET | List owned repos | - | `200 OK` with repo list |
| `/{owner}/{repoName}` | GET | Get repo metadata | - | `200 OK` with branch/owner details |
| `/{owner}/{repoName}/{branch}` | GET | Get branch data (latest commit, README, tree) | - | `200 OK` with commit/tree data |

---

### RepoInvitesController
**Base Route**: `api/web/RepoInvites**

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/` | GET | List pending invites | - | `200 OK` with invites |
| `/{inviteId}/accept` | POST | Accept invite | `inviteId`| `200 OK` |
| `/{inviteId}/decline` | POST | Decline invite | `inviteId` | `200 OK` |

---

### RepoSettingsController
**Base Route**: `api/web/RepoSettings**

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/{owner}/{repoName}/description` | PUT | Update repo description | Body: `{ Description }` | `200 OK` |
| `/{owner}/{repoName}/visibility` | PUT | Toggle public/private | Body: `{ IsPrivate }` | `200 OK` |
| `/{owner}/{repoName}` | DELETE | Delete repo | - | `200 OK` |

---

### UsersController
**Base Route**: `api/web/Users**

| Endpoint | Method | Description | Parameters | Response |
|---|---|---|---|---|
| `/Register` | POST | User registration | Body: `{ Username, Email, Password }` | `201 Created` or `400` |
| `/Login` | POST | Login (JWT + refresh token cookie) | Body: `{ Email, Password }` | `200 OK` with JWT |
| `/Refresh` | POST | Refresh JWT | - | `200 OK` with new JWT |
| `/Logout` | POST | Logout (invalidate refresh token) | - | `200 OK` |

---

## Shared Notes
- **Authentication**:
  - CLI: Personal Access Tokens (PAT) via `CLIPolicy`
  - Frontend: JWT via `FrontendPolicy`
- **Rate Limits**:
  - CLI: `CLIRateLimit`
  - Frontend: `FrontendRateLimit` 
- **CORS**:
  - CLI: CLI clients (`CLIPolicy`)
  - Frontend: Web app (`FrontendPolicy`)