# Project Humanoids Simulatieplatform Viscon
## Project Structure
The project is sorted into several folders
- Workflows: Voor geautomatiseerde acties voor pull en push request + checks voor security
- 2.4-BLENDER-GROEP: Voor eventuele modellen
- 2.4-UNITY-GROEP: Voor simulatie (We gaan alleen kijken naar simulatie voor dit onderdeel)
    1. Settings: De instellingen van het project zodatje het project kan aanpassen op het vlak van: Fabriek, Omgeving en SImulatie
    2. Scenes: De scenes waarin de simulatie zich plaats neemt
    3. Prefabs: De central objecten van een delegateie* en de losse onderdelen ervan
    4. Scripts: De scripts die zorgen dat het project funtioneert (verdere informatie later)
    5. Machine Learning: Het machine learning model en Timers
    6. Imported Models: Modellen gebruikt voor de simulatie geimporteert van Unity-Store of SketchFab
    7. Systems: UNity Systemen die meekomen met het maken van het project
*Delegatie = De opzet van een enkele omgeving voor agent.

## Code Structure
![PlantextUML-Class Diagram-DigiTwin-Viscon.png](<PlantextUML-Class Diagram-DigiTwin-Viscon.png>)
Dit is een trainingsomgeving in Unity ML-Agents waarin een AI-agent leert om dozen te sorteren in een fabrieks instelling. De agent pakt producten van een bewegende transportband en brengt ze naar de juiste locatie.

"Transportband genereert een doos → Agent loopt naar de doos → Agent pakt de doos op → Agent loopt naar de juiste afzetzone → CellManager beloont of bestraft de agent → Herhalen totdat de episode eindigt."

1. Tools: Hogere functies gebruikt in niet project-centrische functies
    - FindMissingScripts: Vindt GameObjects met missende scripts zodat je ze kan identificeren en indien nodig verwijderen

2. Utilities: Enums, Structs, etc.
    - ProductIdentityEnums: Bevat de enum met alle producttypes (Apples, Pears). Gedeelde taal tussen alle scripts die producten herkennen of vergelijken

3. Environment: Code die gaat over de omgeving zoals de loopband en de producten
    - ConveyorLogic: Beheert de runtime werking van de conveyor: beweegt producten vooruit, spawnt nieuwe producten aan het einde, en geeft het eerste product af aan de agent
    - DropOffZoneScript: Ontvangt producten van de agent. Controleert of het juiste producttype bezorgd is en meldt het resultaat aan de CellManager
    - BoxObject: Tijdelijke datacontainer die de agent aanmaakt bij het oppakken van een product. Slaat het producttype én de doellocatie op. Bestaat alleen zolang de agent iets vasthoudt
    - ProductIdentity: Permanent label op elk product-prefab dat aangeeft welk type product het is. Wordt uitgelezen op het moment van oppakken

4. Delegates: Code die gaat over de delegate (beheerder van centrale data per cell)
    - DelegateData: Centrale dataopslag voor 1 factory cell. Houdt de successrate, streak, huidige actie van de agent en het huidig product bij. Stuurt een OnChange event uit bij elke update zodat de UI automatisch meevolgt

5. User Interface: Code die gaat over de user interface en het realiseren ervan
    - DelegateUI: Toont de statistieken van DelegateData op het scherm. Luistert naar het OnChange event en werkt vier tekstvelden bij: successrate, streak, huidige actie en huidig product. De CameraManager bepaalt van welke cell de data getoond wordt
    - EpisodeUI: Toont het huidig episodenummer en een voortgangsbalk. Luistert naar het onBoxPassed event van MLAgentScript en berekent de voortgang op basis van het totaal aantal bezorgde dozen over alle agents heen
    - TimeUI: Toont de verstreken tijd sinds het opstarten. Gebruikt unscaled time zodat de klok correct blijft ook als de simulatie sneller draait

6. Machine Learning: Code die gaat over de werking van de Machine Learning
    - CellManager: De scheidsrechter van 1 factory cell. Past alle beloningen en straffen toe op de agent via ScoringSettings. Coördineert de communicatie tussen de agent, de dropoff zones en de DelegateData
    - MLAgentScript: De AI agent zelf. Bepaalt wat de agent waarneemt (observations), hoe hij beweegt (actions) en beheert de pickup- en delivery-flow. Wisselt tussen twee states: SearchingForBox en CarryingBox

7. Game Managers: Code die over het grotere systeem gaat
    - CameraManager: Beheert een lijst van camera-focuspunten, 1 voor het totaaloverzicht en 1 per factory cell. De gebruiker navigeert met next/prev knoppen. Past camerahoek, veldhoek en de DelegateUI aan op basis van de geselecteerde cell
    - GameManager: Draait eenmalig bij opstarten. Leest EnvironmentSettings en spawnt een grid van factory cell prefabs. Registreert elk CameraFocus-punt bij de CameraManager en stelt het totaal aantal agents in

8. Settings: ScriptableObjects die de regels opzetten voor de instellingen (Zie projectstructuur 2.4-UNITY-GROEP - 1. Settings)
    - EnvironmentSettings: Bepaalt hoeveel factory cells er gespawnd worden, in welk grid en hoe ver ze uit elkaar staan
    - FactorySettings: Bepaalt het gedrag van de conveyor: snelheid, spawnpositie, slotafstand en welke productprefabs willekeurig gebruikt kunnen worden
    - ScoringSettings: Bevat alle getallen voor het beloningssysteem: straf per stap, beloning voor correcte bezorging, straf voor foute bezorging, beloning voor oppakken, enzovoort


## Project Utilization
1. Open Uinity
2. Bekijk de instellingen binnen Unity in de folder [0 Settings]
3. Pas de instellingen aan naar de gewenste parameters
4. Start de simulatie
5. Documenteer resultaten
6. Gooi leer data weg
