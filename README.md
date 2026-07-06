# Quiz - valós idejű online kvízrendszer

Egy full-stack, Kahoot jellegű kvízalkalmazás, ahol a hitelesített felhasználók saját kvízeket hozhatnak létre és vezérelhetnek, a résztvevők pedig PIN-kóddal csatlakozhatnak az élő játékhoz. A rendszer REST API-t, JWT alapú autentikációt, SignalR valós idejű kommunikációt, élő chatet, lapozott adatkezelést és automatizált tesztcsomagot tartalmaz.

## Projekt célja

A projekt célja egy olyan kliens-szerver architektúrájú webalkalmazás megvalósítása volt, amely valós időben kezeli egy kvíz teljes életciklusát:

- felhasználói regisztráció és bejelentkezés,
- saját kvízek létrehozása kérdésekkel és válaszokkal,
- kvíz publikálása hatjegyű PIN-kóddal,
- résztvevők csatlakozása becenévvel,
- kérdések indítása, lezárása és léptetése a kvízgazda által,
- helyes válasz megjelenítése a kérdés lezárása után,
- élő chat a résztvevők között,
- valós idejű állapotfrissítés SignalR-rel.

## Fő funkciók

### Kvízgazda

- Fiók létrehozása és JWT alapú bejelentkezés.
- Saját kvízek listázása lapozással.
- Új kvíz létrehozása 1-20 kérdéssel.
- Kvíz publikálása automatikusan generált PIN-kóddal.
- Élő játék indítása, aktuális kérdés lezárása és következő kérdésre léptetés.
- Saját kvíz részletes adatainak megtekintése.

### Résztvevő

- Publikált kvízek böngészése.
- Csatlakozás PIN-kóddal és egyedi becenévvel.
- Aktuális kérdés követése valós időben.
- Válasz kiválasztása kliens oldali állapotmegőrzéssel.
- Helyes válasz megtekintése a kérdés lezárása után.
- Élő chat használata a kvíz alatt.

## Technológiai stack

### Backend

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- ASP.NET Core Identity
- JWT bearer authentication
- SignalR
- AutoMapper
- Swagger / OpenAPI
- MSTest, Moq, ASP.NET Core integration testing

### Frontend

- Vue 3
- TypeScript
- Vite
- Pinia
- Vue Router
- Axios
- Bootstrap 5
- Microsoft SignalR JavaScript client

## Architektúra

```text
Quiz.sln
+-- Quiz.API          # HTTP API, autentikáció, Swagger, SignalR hub bekötése
+-- Quiz.DataAccess   # EF Core DbContext, modellek, szolgáltatások, migrációk
+-- Quiz.Shared       # Request és response DTO-k
+-- Quiz.SignalR      # Hub és értesítési szolgáltatások
+-- Quiz.Web          # Vue + Vite frontend
+-- Quiz.Test         # Unit, controller és integrációs tesztek
+-- Quiz.Console      # Kiegészítő konzolos projekt
```

A megoldás rétegzett felépítést követ. Az API kontrollerek a HTTP felületet kezelik, az üzleti logika a DataAccess szolgáltatásaiban található, a kliens és szerver közötti szerződéseket pedig a Shared projekt DTO-i írják le. A valós idejű eseményeket a SignalR modul küldi ki a kvízhez tartozó csoportoknak.

## Fontosabb szakmai megoldások

- **JWT és refresh token kezelés:** a bejelentkezés access tokent és refresh tokent ad vissza, a kliens pedig 401 válasz esetén automatikusan frissíti az access tokent.
- **SignalR csoportok:** minden kvíz saját SignalR csoportot kap, így az indítás, kérdéslezárás, kérdésváltás és új chatüzenet csak az adott kvíz résztvevőihez jut el.
- **Problem Details hibakezelés:** a központi exception handler konzisztens JSON hibaválaszokat ad vissza.
- **Lapozott listák:** a publikus kvízek és a felhasználó saját kvízei page alapján kerülnek betöltésre.
- **Tesztelhető felépítés:** a szolgáltatások interface-ek mögött vannak, a tesztek in-memory adatbázissal és mockolt függőségekkel futnak.
- **Kliens oldali állapotmegőrzés:** a bejelentkezett felhasználó, a csatlakozott kvíz és a kiválasztott válasz localStorage-ban is tárolódik, így oldalfrissítés után is folytatható a munkamenet.

## API áttekintés

### Felhasználók - `/users`

| Metódus | Végpont | Leírás | Auth |
| --- | --- | --- | --- |
| POST | `/register` | Új felhasználó regisztrálása | Nem |
| POST | `/login` | Bejelentkezés access és refresh tokennel | Nem |
| POST | `/logout` | Kijelentkezés és refresh token törlése | Igen |
| POST | `/refresh` | Új access és refresh token igénylése | Nem |
| GET | `/{userId}` | Bejelentkezett felhasználó saját adatainak és kvízeinek lekérése | Igen |

### Kvízek - `/quizzes`

| Metódus | Végpont | Leírás | Auth |
| --- | --- | --- | --- |
| GET | `/` | Publikált kvízek listázása lapozással | Nem |
| POST | `/{quizId}` | Csatlakozott kvíz adatainak szinkronizálása | Nem |
| POST | `/create` | Új kvíz létrehozása | Igen |
| GET | `/{quizId}/publish` | Kvíz publikálása és PIN generálása | Igen |
| POST | `/{quizId}/join` | Csatlakozás kvízhez PIN-kóddal | Nem |
| GET | `/{quizId}/start` | Kvíz indítása | Igen |
| POST | `/{quizId}/activeQuestion` | Aktuális kérdés lekérése | Nem |
| GET | `/{quizId}/closeQuestion` | Aktuális kérdés lezárása | Igen |
| GET | `/{quizId}/nextQuestion` | Következő kérdésre léptetés | Igen |
| POST | `/{quizId}/activeQuestion/answer` | Helyes válasz indexének lekérése | Nem |
| POST | `/{quizId}/chat` | Chatüzenet küldése | Nem |
| GET | `/my-quizzes/{quizId}` | Saját kvíz részletes adatainak lekérése | Igen |

## Validációs szabályok

- Felhasználónév vagy becenév: 3-255 karakter.
- E-mail: érvényes e-mail formátum.
- Jelszó: legalább 6 karakter, kisbetű, nagybetű, szám és speciális karakter.
- Kvíz címe: 3-255 karakter.
- Kvíz kérdéseinek száma: 1-20.
- Kérdés szövege: 3-255 karakter.
- Válaszok száma: 2-4.
- PIN: hat számjegyű kód.

## Futtatás fejlesztői környezetben

### Előkövetelmények

- .NET 8 SDK
- Node.js 20.19+ vagy 22.12+
- SQL Server vagy SQL Server LocalDB

### Backend indítása

1. Állítsd be a `Quiz.API/appsettings.Development.json` vagy user secrets értékeit:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=QuizDb;Trusted_Connection=True;TrustServerCertificate=True"
  },
  "JwtSettings": {
    "SecretKey": "legalabb-32-karakter-hosszu-fejlesztoi-titok",
    "Audience": "https://localhost:7181/",
    "Issuer": "https://localhost:7181/"
  }
}
```

2. Adatbázis migráció futtatása:

```powershell
dotnet ef database update --project Quiz.DataAccess --startup-project Quiz.API
```

3. API indítása:

```powershell
dotnet run --project Quiz.API
```

Fejlesztői módban a Swagger felület a futtatott API `/swagger` útvonalán érhető el.

### Frontend indítása

```powershell
cd Quiz.Web
npm install
npm run dev
```

A Vite fejlesztői szerver proxyzza az API és SignalR hívásokat a `https://localhost:7181` backend felé.

## Tesztelés

A projektben három tesztszint szerepel:

- **Unit tesztek:** a szolgáltatások üzleti logikáját ellenőrzik izoláltan.
- **Controller tesztek:** a kontrollerek válaszait és hibakezelését ellenőrzik mockolt függőségekkel.
- **Integrációs tesztek:** valós alkalmazás-hosttal és in-memory adatbázissal ellenőrzik a komponensek együttműködését.

Futtatás:

```powershell
dotnet test
```

Frontend ellenőrzés:

```powershell
cd Quiz.Web
npm run build
npm run lint
```

## Demonstrálható felhasználói folyamat

1. A kvízgazda regisztrál és bejelentkezik.
2. Létrehoz egy új kvízt több kérdéssel.
3. Publikálja a kvízt, a rendszer PIN-kódot generál.
4. A résztvevők a publikus listából vagy azonosító alapján csatlakoznak becenévvel és PIN-kóddal.
5. A kvízgazda elindítja a játékot.
6. A résztvevők valós időben megkapják az aktuális kérdést.
7. A gazda lezárja a kérdést, a kliensek megjelenítik a helyes választ.
8. A gazda a következő kérdésre léptet, vagy az utolsó kérdés után lezárja a kvízt.

## Portfólió szempontból kiemelhető

Ez a projekt jól demonstrálja, hogyan lehet egy modern webalkalmazásban összekötni a klasszikus REST API-kat a valós idejű kommunikációval. A megoldás nem csak CRUD felületeket tartalmaz, hanem állapotgépet követő üzleti folyamatot, szerepkörök szerinti jogosultságkezelést, hibakezelést, tokenfrissítést, frontend állapotkezelést és automatizált tesztelést is.

Különösen érdekes részei:

- valós idejű kvízesemények SignalR-rel,
- JWT + refresh token alapú autentikáció,
- résztvevői munkamenet kezelése bejelentkezés nélkül,
- REST és websocket jellegű működés egy alkalmazásban,
- rétegzett .NET architektúra,
- Vue 3 + Pinia kliensoldali állapotkezelés,
- unit, controller és integrációs tesztek egy projektben.

## Továbbfejlesztési lehetőségek

- Pontszámítás és eredménytábla bevezetése.
- Időzítő kérdésenként.
- Admin felület felhasználók és kvízek moderálásához.
- Docker Compose alapú lokális futtatás SQL Server konténerrel.
- CI pipeline automatikus builddel és tesztfuttatással.
- Részletesebb OpenAPI példák request/response mintákkal.
