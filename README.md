# SeriousGame

Serious game multijoueur construit comme exercice pédagogique de Clean Architecture, SOLID, et quelques design patterns. Chaque joueur dirige son entreprise dans la tech et doit remporter des appels d'offres pour la développer ; celui qui a le plus grand chiffre d'affaires à la fin de la partie gagne. Le tout tourne sur un serveur ASP.NET Core SignalR plus un client console/TUI, partageant DTOs/contrats via un projet commun.

## Prérequis

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (`global.json` fixe la version `10.0.101`)

## Build et tests

```bash
dotnet build   # build de toute la solution
dotnet test    # lance tous les projets de tests
```

## Lancer le serveur (WebAPI)

```bash
cd src/SeriousGame.Server
dotnet run --launch-profile https
```

Le serveur démarre sur `https://localhost:7289` (voir `Properties/launchSettings.json`).

## Lancer le client (CLI/TUI)

```bash
cd src/SeriousGame.Client
dotnet run
```

Le client se connecte au serveur via l'URL configurée dans `appsettings.json` (section `WebSocketServer`) et lance l'interface console.

## Publier les applications

Publier le serveur :

```bash
cd src/SeriousGame.Server
dotnet publish -c Release -o ./publish
```

Publier le client :

```bash
cd src/SeriousGame.Client
dotnet publish -c Release -o ./publish
```

Chaque commande génère le binaire compilé dans son dossier `./publish`, prêt à être déployé.

## Documentation

Voir **[docs/README.md](docs/README.md)** pour l'architecture, la structure des projets, et les limitations connues — organisée en une page Markdown liée par sujet.
