# IntelliTrip Planner (Conceptnaam)

## Overview

IntelliTrip Planner is een intelligente, multi-agent applicatie in C# ontworpen om gebruikers te assisteren bij het plannen van reizen. Dit project dient als een demonstratie-applicatie om de kracht van een contextueel aangestuurde, multi-agent architectuur te tonen, met een specifieke focus op het gebruik van het **Model Context Protocol (MCP)** en de bijbehorende **MCP C# SDK** voor interactie met externe services.

De kern van de applicatie is een centrale AI-coördinator (`HoofdAgent`) die een overkoepelend `TripPlanningContextFrame` beheert. Deze `HoofdAgent` delegeert specifieke taken (zoals vluchten zoeken, accommodatie vinden) aan gespecialiseerde sub-agents die via de MCP C# SDK op een gestandaardiseerde en context-bewuste manier communiceren met externe API's (voorgesteld als "Tools" binnen MCP).

## Core Concept

De applicatie volgt een multi-agent systeem (MAS) paradigma:

1.  **Gebruikersinput**: De gebruiker levert initiële reisvoorkeuren (bestemming, data, interesses, budget).
2.  **Context Creatie**: De `HoofdAgent` creëert en onderhoudt een `TripPlanningContextFrame` dat alle relevante informatie en de status van het plan bevat.
3.  **Taakdelegatie**: De `HoofdAgent` analyseert de context en delegeert specifieke taken aan gespecialiseerde sub-agents.
4.  **Externe Communicatie via MCP**: Sub-agents gebruiken de **MCP C# SDK** (`https://github.com/modelcontextprotocol/csharp-sdk`) om gestructureerd en contextueel te interacteren met externe API's (Tools). Elke externe API wordt gedefinieerd via een "Tool Manifest".
5.  **Resultaat Integratie**: Resultaten van sub-agents worden teruggekoppeld en geïntegreerd in het centrale `TripPlanningContextFrame` door de `HoofdAgent`.
6.  **Plan Presentatie**: De `HoofdAgent` stelt (conceptueel) een reisplan samen op basis van de verzamelde informatie.

## Key Features & Agents

* **`HoofdAgent` (Orchestrator)**: Beheert het `TripPlanningContextFrame`, coördineert sub-agents, en synthetiseert het uiteindelijke reisplan.
* **Gebruikersinteractie Module**: Een eenvoudige interface (console/API) voor het verzamelen van gebruikersvereisten.
* **Gespecialiseerde Sub-Agents**:
    * `VluchtAgent`: Vindt vluchtopties via externe vlucht-API's (via MCP SDK).
    * `AccommodatieAgent`: Vindt hotel/accommodatie-opties via externe accommodatie-API's (via MCP SDK).
    * `ExcursieAgent`: Ontdekt excursies en activiteiten (via MCP SDK).
    * *(Optioneel uitbreidbaar met `NatuurSchoonAgent`, `RouteplannerAgent`, `CultuurAgent`, `RestaurantAgent`)*

## Architectuur

De applicatie is ontworpen als een multi-agent systeem waarbij:
* De **`HoofdAgent`** fungeert als de centrale intelligentie en coördinator.
* **Sub-agents** als gespecialiseerde werkers die specifieke deeltaken uitvoeren.
* Communicatie met externe API's wordt geabstraheerd en gestandaardiseerd door de **MCP C# SDK**, waarbij elke externe API een "Tool" is met een bijbehorend "Tool Manifest".
* Interne data en status worden bijgehouden in een dynamisch **`TripPlanningContextFrame`**.

## Technology Stack

* **Taal/Framework**: C# op .NET (bij voorkeur .NET 8 of nieuwer)
* **Kernprotocol (Extern)**: Model Context Protocol (MCP) via de officiële **MCP C# SDK**.
* **Interne Context**: `TripPlanningContextFrame` objecten (geïnspireerd/gebruikmakend van MCP SDK types).
* **Asynchroon Programmeren**: Intensief gebruik van `async/await`.
* **Resilience**: Polly (of vergelijkbaar) voor robuuste externe API-aanroepen.
* **Logging**: Standaard logging via `Microsoft.Extensions.Logging` (of Serilog/NLog).

## Modules Overview

De applicatie is opgebouwd uit de volgende hoofdmodules:

1.  **Applicatie Fundament & Basisstructuur**: Project setup, logging, configuratie.
2.  **Model Context Protocol (MCP) Integratielaag**: Integratie MCP C# SDK, Tool Manifest definities, `IToolExecutor` service.
3.  **Centraal Context Management**: Definitie en beheer van het `TripPlanningContextFrame`.
4.  **`HoofdAgent` Module (Orchestrator)**: Kernlogica, workflow management, taakdelegatie.
5.  **Gebruikersinteractie Module**: Inputverwerking en (conceptuele) outputweergave.
6.  **Gespecialiseerde Sub-Agent Modules**: Implementatie van de individuele agents (`VluchtAgent`, `AccommodatieAgent`, etc.).
7.  **Externe API Mocking Layer**: Voor het simuleren van externe API's tijdens ontwikkeling en demo.
8.  **Resilience Module**: Integratie van Polly voor robuuste API calls.

## Project Goals 

Deze repository is gericht op het ontwikkelen van een functionerende demo applicatie die:

* De multi-agent architectuur demonstreert met een `HoofdAgent` en minimaal 2-3 werkende sub-agents.
* Succesvol de **MCP C# SDK** gebruikt voor (gesimuleerde of echte) aanroepen naar externe API's door de sub-agents.
* Een duidelijke flow van context (`TripPlanningContextFrame`) laat zien.
* De voordelen van de gekozen aanpak (multi-agent, MCP, C#) effectief communiceert.
* Een schone, goed georganiseerde en gedocumenteerde codebasis heeft.

## Getting Started

### Prerequisites

* .NET SDK (versie 8.0 of hoger aanbevolen).
* Een IDE zoals Visual Studio 2022 of Visual Studio Code.

### Build & Run

1.  Clone de repository:
    ```bash
    git clone [https://github.com/jouw-gebruikersnaam/jouw-repositorynaam.git](https://github.com/jouw-gebruikersnaam/jouw-repositorynaam.git)
    cd jouw-repositorynaam
    ```
2.  Open de solution in je IDE of gebruik de .NET CLI:
    ```bash
    dotnet build
    ```
3.  Voer de applicatie uit (pas aan afhankelijk van het entry point project):
    ```bash
    dotnet run --project PadNaarEntryPointProject/EntryPointProject.csproj
    ```
    *(Specifieke instructies kunnen variëren afhankelijk van de uiteindelijke projectstructuur)*
