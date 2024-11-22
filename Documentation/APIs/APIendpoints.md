
## APIs

#### **User Register**

```http
  POST /users/register
```

| Parameter | Type     | Description                |
| :-------- | :------- | :------------------------- |
| `username` | `string` | **Required** The username of the user |
| `email` | `string` | **Required** The email address of the user |
| `password` | `string` | **Required** The password for the user |

##### Request Body:
```json
{
  "username": "exampleUser",
  "email": "user@example.com",
  "password": "examplePassword"
}
```
##### Response:
```json
{
  "message": "User registered successfully"
}
```



#### **User Login**

```http
  POST /users/login
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `email` | `string` | **Required** The email of the user |
| `password` | `string` | **Required** The password of the user |

##### Request Body:
```json
{
  "email": "example@user.com",
  "password": "examplePassword"
}
```
##### Response:
```json
{
  "access_token": "jwtToken"
}
```



#### **Create Repository**

```http
  POST /repositories/create
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `repoName` | `string` | **Required** Name of the repository |
| `visibility` | `string` | **Optional** Visability of the repo (public or private) |

##### Request Body:
```json
{
  "repoName": "exampleName",
  "visability": "private"
}
```
##### Response:
```json
{
  "repoId": 1,
  "message": "Repository created successfully"
}
```





#### **List Repositories of User**

```http
  GET /repositories/{userId}
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `userId` | `int` | **Required** The ID of the user |

##### Request Example:
```http
GET /repositories/123
```

##### Response:
```json
[
  {
    "repoId": 0,
    "repoName": "exampleName",
    "visibility": "private",
    "created": "2024-01-01T14:00:00"
  },
  {
    "repoId": 1,
    "repoName": "anotherName",
    "visibility": "public",
    "created": "2024-01-01T14:00:00"
  }
]
```





#### **Add Collaborator to Repository**

```http
  GET /repositories/{repoId}/collaborators
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `repoId` | `int` | **Required** ID of the repository |
| `userId` | `int` | **Required** ID of the collaborator |
| `role` | `string` | **Required** Role of the collaborator |

##### Request Example:
```json
{
  "userId": 123,
  "role": "viewer"
}
```

##### Response:
```json
{
  "message": "Collaborator added successfully"
}
```





#### **Create a Branch**

```http
  GET /repositories/{repoId}/branches
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `repoId` | `int` | **Required** ID of the repository |
| `branchName` | `string` | **Required** Name of the new branch |

##### Request Example:
```json
{
  "branchName": "exampleName"
}
```

##### Response:
```json
{
  "branchId": 2,
  "message": "Branch created successfully"
}
```




#### **Create a Commit**

```http
  POST /repositories/{repoId}/(branchId)/commits
```

| Parameter | Type     | Description                       |
| :-------- | :------- | :-------------------------------- |
| `repoId` | `int` | **Required** ID of the repository |
| `branchId` | `int` | **Required** ID of the branch |
| `message` | `string` | **Required** Commit message |
| `files` | `array` | **Required** Array of files that changed |

##### Request Example:
```json
{
  "message": "Example message",
  "files": [
    { "filePath": "testFile.txt", "content": "Hello world!" },
    { "filePath": "folder/testFile2.txt", "content": "Another file" }
  ]
}
```

##### Response:
```json
{
  "commitId": 3,
  "message": "Commit created successfully"
}
```




