# Projectname

Inventory-Order-Management-Api

## Features

- RESTful API
- Swagger/OpenAPI-Documentation
- JWT-Authentification
- Role Based Access Control (RBAC)

## Technologies

- .NET 10 Sdk
- Entity Framework Core
- ASP.NET Core Identity
- Swagger / OpenAPI
- Sqlite in Development Environment

## How to run the API on your local machine

### 1. Installation

```bash
git clone https://github.com/msass89/InventoryOrderManagementApi.git
cd API
```

### 2. Restore packages:

```bash
dotnet restore
```

### 3. Configuration

`appsettings.json`

```json
{
  "Jwt": {
    "Issuer": "...",
    "Audience": "...",
    "Key": "..."
  }
}
```

Alternatively it's recommended to set the JWT key by setting a user secret or an environment variable.

### 4. Run database migrations

```bash
dotnet ef database update
```

### 5. Start Application

```bash
dotnet run
```

## Swagger UI

The swagger UI is set to the root and is starting automatically when running the Api.


## API-Endpoints

| Method | Route | Description |
|---------|-------|--------------|
| POST | /api/auth/register | Register a new user |
| POST | /api/auth/login | Login as a new user |
| GET | /api/inventory | Return all items in the inventory |
| GET | /api/inventory/{id} | Return item by Id |
| POST | /api/inventory | Create new Item |
| DELETE | /api/inventory/{id} | Delete item by Id |
| GET | /api/orders |  Return all orders |
| GET | /api/orders/{id} |  Return order by Id |
| POST | /api/orders |  Create an orders |
| DELETE | /api/orders/{id} | Delete an order by Id |

## Authentification and Authorization

This API is secured by JWT authentification and RBAC-based authorizaiton.
Roles are 'Admin', 'SalesAgent', 'InventoryAgent', 'Customer'.
For accessing the endpoints register as a new user by providing a (fake) email adress, a password and the role.
The API is responding with a Bearer token. You can copy this token and paste in into the input field appearing when clicking on 'Authorize' on the upper right corner of the Swagger UI.
Now you are able to access the secured endpoints according to the user role.

## Projectstructure

```
Controller/
Data/
  Seed/
Migrations/
Models/
  DTO/
  Entities/
  Enums/

Program.cs
```

## Licence

MIT