# Dane Logowania - System Zarządzania Biblioteką

## Logowanie przez Frontend

**Frontend URL:** `http://localhost:3000/login`

Po uruchomieniu frontendu (Next.js) i backendu (ASP.NET Core), możesz zalogować się przez stronę logowania w przeglądarce.

## Endpoint Logowania (API)

**URL:** `POST /api/Auth/login`

**Base URL:** 
- HTTP: `http://localhost:5205` (domyślny)
- HTTPS: `https://localhost:7246`

## Przykładowe Użytkownicy

System został zainicjalizowany z następującymi użytkownikami:

### 1. Administrator
- **Nazwa użytkownika:** `admin`
- **Hasło:** `Admin123!`
- **Rola:** Admin
- **Email:** admin@firma.pl
- **Uprawnienia:** Pełny dostęp do wszystkich funkcji systemu

### 2. Bibliotekarz
- **Nazwa użytkownika:** `bibliotekarz`
- **Hasło:** `Librarian123!`
- **Rola:** Librarian
- **Email:** piotr.wisniewski@firma.pl
- **Uprawnienia:** Zarządzanie książkami, wypożyczeniami, użytkownikami

### 3. Pracownik - Jan Kowalski
- **Nazwa użytkownika:** `jan.kowalski`
- **Hasło:** `Employee123!`
- **Rola:** Employee
- **Email:** jan.kowalski@firma.pl
- **Uprawnienia:** Przeglądanie książek, wypożyczanie

### 4. Pracownik - Anna Nowak
- **Nazwa użytkownika:** `anna.nowak`
- **Hasło:** `Employee123!`
- **Rola:** Employee
- **Email:** anna.nowak@firma.pl
- **Uprawnienia:** Przeglądanie książek, wypożyczanie

## Przykład Żądania HTTP

### cURL
```bash
curl -X POST "http://localhost:5205/api/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "Admin123!"
  }'
```

### JavaScript (Fetch API)
```javascript
fetch('http://localhost:5205/api/Auth/login', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    username: 'admin',
    password: 'Admin123!'
  })
})
.then(response => response.json())
.then(data => {
  console.log('Token:', data.token);
  // Zapisz token w localStorage lub state
  localStorage.setItem('token', data.token);
})
.catch(error => console.error('Błąd:', error));
```

### JavaScript (Axios)
```javascript
import axios from 'axios';

axios.post('http://localhost:5205/api/Auth/login', {
  username: 'admin',
  password: 'Admin123!'
})
.then(response => {
  const token = response.data.token;
  // Zapisz token
  localStorage.setItem('token', data.token);
  // Ustaw token w nagłówkach dla kolejnych żądań
  axios.defaults.headers.common['Authorization'] = `Bearer ${token}`;
})
.catch(error => console.error('Błąd:', error));
```

## Odpowiedź z Serwera

Po pomyślnym logowaniu otrzymasz odpowiedź w formacie:

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "email": "admin@firma.pl",
  "role": "Admin",
  "expiresAt": "2024-11-23T12:00:00Z"
}
```

## Używanie Tokenu JWT

Po otrzymaniu tokenu, musisz go wysyłać w nagłówku `Authorization` przy każdym żądaniu do chronionych endpointów:

```
Authorization: Bearer <twój_token>
```

### Przykład z cURL
```bash
curl -X GET "http://localhost:5205/api/Books" \
  -H "Authorization: Bearer <twój_token>"
```

### Przykład z JavaScript
```javascript
fetch('http://localhost:5205/api/Books', {
  headers: {
    'Authorization': `Bearer ${localStorage.getItem('token')}`
  }
})
.then(response => response.json())
.then(data => console.log(data));
```

## Swagger UI

Możesz również przetestować logowanie przez Swagger UI:
1. Uruchom aplikację
2. Otwórz w przeglądarce: `http://localhost:5205/swagger` lub `https://localhost:7246/swagger`
3. Znajdź endpoint `POST /api/Auth/login`
4. Kliknij "Try it out"
5. Wprowadź dane logowania (np. `admin` / `Admin123!`)
6. Kliknij "Execute"
7. Skopiuj otrzymany token
8. Kliknij przycisk "Authorize" w górnej części strony Swagger
9. Wprowadź token w formacie: `Bearer <twój_token>`
10. Teraz możesz testować wszystkie chronione endpointy

## Uwagi

- Token JWT wygasa po 1440 minutach (24 godziny) - sprawdź `appsettings.json` dla dokładnej konfiguracji
- Wszystkie endpointy oprócz `/api/Auth/login` i `/api/Auth/register` wymagają autoryzacji
- Endpointy do tworzenia, edycji i usuwania wymagają roli `Admin` lub `Librarian`
- Endpointy do przeglądania są dostępne dla wszystkich zalogowanych użytkowników

