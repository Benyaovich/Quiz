# Quiz - valos ideju online kvizrendszer

Egy teljes stackes, Kahoot jellegu kvizalkalmazas, ahol a hitelesitett felhasznalok sajat kvizeket hozhatnak letre es vezérelhetnek, a resztvevok pedig PIN-koddal csatlakozhatnak az elo jatekhoz. A rendszer REST API-t, JWT alapu autentikaciot, SignalR valos ideju kommunikaciot, chatet, lapozott adatkezelest es automatizalt tesztcsomagot tartalmaz.

## Projekt celja

A projekt celja egy olyan kliens-szerver architekturaju webalkalmazas megvalositasa volt, amely valos idoben kezeli egy kviz teljes eletciklusat:

- felhasznalo regisztracio es bejelentkezes,
- sajat kvizek letrehozasa kerdesekkel es valaszokkal,
- kviz publikalasa hatjegyu PIN-koddal,
- resztvevok csatlakozasa becenevvel,
- kerdesek inditasa, lezarasa es leptetese a kvizgazda altal,
- helyes valasz megjelenitese a kerdes lezarasa utan,
- elo chat a resztvevok kozott,
- valos ideju allapotfrissites SignalR-rel.

## Fo funkciok

### Kvizgazda

- Fiok letrehozasa es JWT alapu bejelentkezes.
- Sajat kvizek listazasa lapozassal.
- Uj kviz letrehozasa 1-20 kerdessel.
- Kviz publikalasa automatikusan general PIN-koddal.
- Elo jatek inditasa, aktualis kerdes lezarasa, kovetkezo kerdesre leptetes.
- Saját kviz reszletes adatainak megtekintese.

### Resztvevo

- Publikalt kvizek bongeszese.
- Csatlakozas PIN-koddal es egyedi becenevvel.
- Aktualis kerdes kovetese valos idoben.
- Valasz kivalasztasa kliens oldali allapotmegorzessel.
- Helyes valasz megtekintese a kerdes lezarasa utan.
- Elo chat hasznalata a kviz alatt.

## Technologiai stack

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

## Architekturális felépítés

```text
Quiz.sln
├── Quiz.API          # HTTP API, autentikacio, Swagger, SignalR hub bekotese
├── Quiz.DataAccess   # EF Core DbContext, modellek, szolgaltatasok, migraciok
├── Quiz.Shared       # Request es response DTO-k
├── Quiz.SignalR      # Hub es ertesitesi szolgaltatasok
├── Quiz.Web          # Vue + Vite frontend
├── Quiz.Test         # unit, controller es integracios tesztek
└── Quiz.Console      # kiegeszito konzolos projekt
```

A megoldas retegezett felepitest kovet. Az API kontrollerek csak a HTTP feluletet kezelik, az uzleti logika a DataAccess szolgaltatasaiban talalhato, a kliens es szerver kozotti szerzodeseket pedig a Shared projekt DTO-i irjak le. A valos ideju eseményeket a SignalR modul kuldi ki a kvizhez tartozo csoportoknak.

## Fontosabb szakmai megoldasok

- **JWT es refresh token kezeles:** a bejelentkezes access tokent es refresh tokent ad vissza, a kliens pedig 401 valasz eseten automatikusan frissiti az access tokent.
- **SignalR csoportok:** minden kviz sajat SignalR csoportot kap, igy az inditas, kerdeslezaras, kerdesvaltas es uj chatuzenet csak az adott kviz resztvevoihez jut el.
- **Problem Details hibakezeles:** a kozponti exception handler konzisztens JSON hibavalaszokat ad vissza.
- **Lapozott listak:** a publikus kvizek es a felhasznalo sajat kvizei page alapjan kerulnek betoltesre.
- **Tesztelheto felepites:** a szolgaltatasok interface-ek mogott vannak, a tesztek in-memory adatbazissal es mockolt fuggosegekkel futnak.
- **Kliens oldali allapotmegorzes:** a bejelentkezett felhasznalo, a csatlakozott kviz es a kivalasztott valasz localStorage-ban is tarolodik, igy oldalfrissites utan is folytathato a munkamenet.

## API attekintes

### Felhasznalok - `/users`

| Metodus | Vegpont | Leiras | Auth |
| --- | --- | --- | --- |
| POST | `/register` | Uj felhasznalo regisztralasa | Nem |
| POST | `/login` | Bejelentkezes access es refresh tokennel | Nem |
| POST | `/logout` | Kijelentkezes es refresh token torlese | Igen |
| POST | `/refresh` | Uj access es refresh token igenylese | Nem |
| GET | `/{userId}` | Bejelentkezett felhasznalo sajat adatainak es kvizeinek lekérése | Igen |

### Kvizek - `/quizzes`

| Metodus | Vegpont | Leiras | Auth |
| --- | --- | --- | --- |
| GET | `/` | Publikalt kvizek listazasa lapozassal | Nem |
| POST | `/{quizId}` | Csatlakozott kviz adatainak szinkronizalasa | Nem |
| POST | `/create` | Uj kviz letrehozasa | Igen |
| GET | `/{quizId}/publish` | Kviz publikalasa es PIN generalasa | Igen |
| POST | `/{quizId}/join` | Csatlakozas kvizhez PIN-koddal | Nem |
| GET | `/{quizId}/start` | Kviz inditasa | Igen |
| POST | `/{quizId}/activeQuestion` | Aktualis kerdes lekérése | Nem |
| GET | `/{quizId}/closeQuestion` | Aktualis kerdes lezarasa | Igen |
| GET | `/{quizId}/nextQuestion` | Kovetkezo kerdesre leptetes | Igen |
| POST | `/{quizId}/activeQuestion/answer` | Helyes valasz indexenek lekérése | Nem |
| POST | `/{quizId}/chat` | Chat uzenet kuldese | Nem |
| GET | `/my-quizzes/{quizId}` | Sajat kviz reszletes adatainak lekérése | Igen |

## Validacios szabalyok

- Felhasznalonev vagy becenev: 3-255 karakter.
- E-mail: ervenyes e-mail formatum.
- Jelszo: legalabb 6 karakter, kisbetu, nagybetu, szam es specialis karakter.
- Kviz cim: 3-255 karakter.
- Kviz kerdesek szama: 1-20.
- Kerdes szovege: 3-255 karakter.
- Valaszok szama: 2-4.
- PIN: hat szamjegyu kod.

## Futtatas fejlesztoi kornyezetben

### Elokovetelmenyek

- .NET 8 SDK
- Node.js 20.19+ vagy 22.12+
- SQL Server vagy SQL Server LocalDB

### Backend inditasa

1. Allitsd be a `Quiz.API/appsettings.Development.json` vagy user secrets ertekeit:

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

2. Adatbazis migracio futtatasa:

```powershell
dotnet ef database update --project Quiz.DataAccess --startup-project Quiz.API
```

3. API inditasa:

```powershell
dotnet run --project Quiz.API
```

Fejlesztoi modban a Swagger felulet a futtatott API `/swagger` utvonalán érheto el.

### Frontend inditasa

```powershell
cd Quiz.Web
npm install
npm run dev
```

A Vite fejlesztoi szerver proxyzza az API es SignalR hivasokat a `https://localhost:7181` backend fele.

## Teszteles

A projektben harom tesztszint szerepel:

- **Unit tesztek:** a szolgaltatasok uzleti logikajat ellenorzik izolaltan.
- **Controller tesztek:** a kontrollerek valaszait es hibakezeleset ellenorzik mockolt fuggosegekkel.
- **Integracios tesztek:** valos alkalmazas-hosttal es in-memory adatbazissal ellenorzik a komponensek egyuttmukodeset.

Futtatas:

```powershell
dotnet test
```

Frontend ellenorzes:

```powershell
cd Quiz.Web
npm run build
npm run lint
```

## Demonstralhato felhasznaloi folyamat

1. A kvizgazda regisztral es bejelentkezik.
2. Letrehoz egy uj kvizt tobb kerdessel.
3. Publikalja a kvizt, a rendszer PIN-kodot general.
4. A resztvevok a publikus listabol vagy azonosito alapjan csatlakoznak becenevvel es PIN-koddal.
5. A kvizgazda elinditja a jatekot.
6. A resztvevok valos idoben megkapjak az aktualis kerdest.
7. A gazda lezarja a kerdest, a kliensek megjelenitik a helyes valaszt.
8. A gazda a kovetkezo kerdesre leptet, vagy az utolso kerdes utan lezárja a kvizt.

## Portfolió szempontból kiemelheto

Ez a projekt jol demonstralja, hogyan lehet egy modern webalkalmazasban osszekotni a klasszikus REST API-kat a valos ideju kommunikacioval. A megoldas nem csak CRUD feluleteket tartalmaz, hanem allapotgepet koveto uzleti folyamatot, szerepkorok szerinti jogosultsagkezelest, hibakezelest, tokenfrissitest, frontend allapotkezelest es automatizalt tesztelest is.

Kulonosen erdekes reszei:

- valos ideju kvízesemenyek SignalR-rel,
- JWT + refresh token alapu authentikacio,
- resztvevoi munkamenet kezelese bejelentkezes nelkul,
- REST es websocket jellegu mukodes egy alkalmazasban,
- retegezett .NET architektura,
- Vue 3 + Pinia kliensoldali allapotkezeles,
- unit, controller es integracios tesztek egy projektben.

## Tovabbfejlesztesi lehetosegek

- Pontszamitas es eredmenytabla bevezetese.
- Idozito kerdesenkent.
- Admin felulet felhasznalok es kvizek moderálásához.
- Docker Compose alapú lokális futtatás SQL Server konténerrel.
- CI pipeline automatikus builddel es tesztfuttatassal.
- Reszletesebb OpenAPI peldak request/response mintakkal.
