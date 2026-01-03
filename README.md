# WMD Project - The Bureaucracy Algorithm

Dit project is een interactieve Visual Novel die demonstreert hoe ondoorzichtige algoritmen ("Weapons of Math Destruction") onze professionele toekomst kunnen bepalen op basis van verborgen en vaak oneerlijke statistieken.

## Installatie & Uitvoering

### 1. Backend (Het Brein)

De backend draait in een Docker-container en beheert de database en het algoritme.

1. Zorg ervoor dat **Docker Desktop** is geïnstalleerd en draait.
2. Navigeer naar de hoofdmap van het project.
3. Open een terminal en voer het volgende commando uit om de server en database te starten:
   docker compose up --build
4. Zodra de server draait, is het Admin Dashboard toegankelijk via: http://localhost:3000/admin

### 2. Frontend (Het Spel)

De gebruikersinterface is een Unity-game.

1. Open Unity Hub.
2. Voeg het project toe via de map unity_project (Versie: 6000.0.37f1).
3. Open de scene: Scenes/SampleScene.
4. Druk op de Play-knop om het sollicitatiegesprek te starten.

## De Mechanieken

Het spel verzamelt gegevens zonder medeweten van de speler om een OCEAN-persoonlijkheidsprofiel op te stellen. Naast de gegeven antwoorden, analyseert het systeem ook biometrische metadata:

Neuroticisme (Instabiliteit): Wordt berekend op basis van muisbewegingen (mouseDistance). Overmatig bewegen of "jitteren" wordt geïnterpreteerd als angst of stress.

Consciëntieusheid (Zorgvuldigheid): Wordt gemeten via reactietijd. Te snel antwoorden wordt gezien als impulsief, te langzaam als lui of onzeker.

Agreeableness (Inschikkelijkheid): Specifieke "valstrik"-vragen (zoals over vakbonden) filteren rebelse kandidaten uit.

Het systeem is ontworpen om kandidaten te verwerpen (REJECT) als ze niet voldoen aan strikte, onmenselijke criteria, zelfs als hun antwoorden "correct" lijken.

## Bronvermelding

Code & Debugging: Google Gemini (Generatie van Node.js logica, Unity C# scripts, en Docker configuratie).
https://gemini.google.com/share/fdd2f71eea21

Grafische Assets: Midjourney / DALL-E 3 (Stijl: "1930s rubber hose animation", "Cuphead style", "Vintage UI").
https://gemini.google.com/share/9bcb4f4625c7
