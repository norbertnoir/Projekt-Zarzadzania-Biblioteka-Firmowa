# System Zarządzania Biblioteką Firmy - Frontend

Frontend aplikacji do zarządzania firmową biblioteką, zbudowany z wykorzystaniem Next.js, TypeScript i shadcn-ui.

## Funkcjonalności

- 📚 **Zarządzanie książkami** - przeglądanie, dodawanie, edycja i usuwanie książek
- 👥 **Zarządzanie użytkownikami** - zarządzanie użytkownikami biblioteki
- 📖 **Wypożyczenia** - tworzenie i zarządzanie wypożyczeniami książek
- 🔍 **Wyszukiwanie** - wyszukiwanie książek i użytkowników
- 📊 **Statystyki** - przegląd podstawowych statystyk biblioteki

## Wymagania

- Node.js 18+ 
- npm lub yarn

## Instalacja

1. Zainstaluj zależności:

```bash
npm install
```

lub

```bash
yarn install
```

## Konfiguracja

Utwórz plik `.env.local` w katalogu głównym projektu:

```env
NEXT_PUBLIC_API_URL=https://localhost:7246
```

## Uruchomienie

Uruchom serwer deweloperski:

```bash
npm run dev
```

lub

```bash
yarn dev
```

Aplikacja będzie dostępna pod adresem [http://localhost:3000](http://localhost:3000)

## Budowanie

Aby zbudować aplikację produkcyjną:

```bash
npm run build
npm start
```

## Struktura projektu

```
├── app/                    # Strony aplikacji (Next.js App Router)
│   ├── books/             # Strony związane z książkami
│   ├── loans/             # Strony związane z wypożyczeniami
│   ├── users/             # Strony związane z użytkownikami
│   ├── layout.tsx         # Główny layout
│   └── page.tsx           # Strona główna
├── components/            # Komponenty React
│   ├── ui/               # Komponenty shadcn-ui
│   └── navigation.tsx    # Komponent nawigacji
├── lib/                  # Biblioteki i narzędzia
│   ├── api/             # Klienty API
│   ├── api.ts           # Konfiguracja axios
│   └── utils.ts         # Funkcje pomocnicze
├── types/                # Definicje typów TypeScript
└── hooks/               # React hooks
```

## Technologie

- **Next.js 14** - Framework React z App Router
- **TypeScript** - Typowanie statyczne
- **Tailwind CSS** - Stylowanie
- **shadcn-ui** - Komponenty UI
- **Axios** - Klient HTTP
- **date-fns** - Obsługa dat

## API

Aplikacja komunikuje się z backendem przez REST API. Endpointy API są dostępne w dokumentacji Swagger pod adresem:
`https://localhost:7246/swagger/index.html`

### Główne endpointy:

- `/api/Books` - Zarządzanie książkami
- `/api/Users` - Zarządzanie użytkownikami
- `/api/Loans` - Zarządzanie wypożyczeniami

## Uwagi dotyczące SSL

Jeśli backend używa certyfikatu SSL z własnym podpisem (self-signed), możesz napotkać problemy z połączeniem. W takim przypadku:

1. Użyj przeglądarki, która akceptuje certyfikat
2. Lub skonfiguruj proxy w `next.config.js`

## Licencja

Projekt prywatny - System Zarządzania Biblioteką Firmy

