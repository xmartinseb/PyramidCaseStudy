# Case study
Vypracoval Martin Sebera, srpen 2026

## 1. SQL dotaz
Jde o JOIN mezi dvěma tabulkami. Logika je stejná napříč v MSSQL, Postgres i v jakékoliv jiné variantě.

Takto by to mělo vypadat v MSSQL

~~~~sql
SELECT ZAP.kniha FROM zakaznik as ZAK
INNER JOIN zapujcky as ZAP ON ZAK.id = ZAP.id_zakaznika
WHERE ZAK.jmeno = 'Jaroslav Novák'
   AND ZAP.datum_zapujceni >= '2026-01-01' AND ZAP.datum_zapujceni < '2026-02-01'
   AND ZAP.datum_vraceni IS NULL
~~~~

Teoreticky by se dal typ DATE rozkládat na jednotlivé části: year, month, apod. Nicméně tahle varianta je lepší kvůli indexaci 
(ač v zadání není indexace nad datumy, v živé databázi by mohla být)

## 2. Pyramida
**Uspořádání v paměti**: vrchol pyramidy má index [rows-1, 0]; Základna pyramidy má tyto indexy: [0, 0...rows-1]

### Algoritmus
Cílem je najít maximální součet v pyramidě, na každém políčku je jedno číslo, začíná se na špičce (řádek s indexem rows-1) a postupuje nahoru (k řádku 0).

Moje implementace se snaží být obecně správná:
- funguje tedy i pro záporná čísla.
- šetří stack, který bývá velikostně omezen, zvládne tak vypočítat i obří pyramidy.

Součástí zadání není najít kompletní max cestu, pouze najít maximální součet, to je ta jednodušší varianta.

1. Není tak třeba nahlížet na konkrétní cesty, stačí procházet řádky od špičky po nejširší (_rows-1 ... 0) a pamatovat si pro každé políčko nejvyšší možný součet.
    - Budeme tedy potřebovat dvě arrays. Jedno pro iteraci aktuálního řádku, druhé uchovává nalezená maxima řádku pod ním.
    - Každé políčko má pod sebou jedno nebo dvě sousední políčka. Při iteraci stačí pro každé políčko vybírat maximum nejvýše ze dvou variant.
    - Každé políčko navštívíme pouze jednou. Složitost algoritmu je tak **O(n)**, kde n je **počet políček pyramidy**
    - **POZOR!** Nelze se spoléhat na lokální maxima, protože cesta, která se v průběhu zdá jako nevýhodná, může být nakonec ta optimální. Je tedy potřeba vyhodnotit každé políčko pyramidy.
2. Až dojdeme na nejširší řádek 0, spočítáme poslední maximální součty.
3. V součtech nalezneme maximum a to vrátíme

### Výhrady k použitému 2D poli int[,]
1. Neefektivní využívání paměti, protože pracujeme pouze s polovinou alokovaného prostoru, zbytek jsou nuly. 
2. Kdybychom použili pole polí int[][], mohli bychom každému řádku nastavit ideální délku.
    - Trade-off: vytváření více objektů (_rows polí místo jednoho pole) a tím více práce pro GC. Ale stálo by to za to.
    - Trade-off: Pole polí zavádí další úroveň dereference, což znamená horší cache locality a mírné zpomalení při čtení z paměti
3. Pro výpočty velkých pyramid je plýtvání paměti kritické:
    - ve fragmentované paměti často nelze alokovat souvislý blok o velikosti několik GB, proto je rozdělení alokací na řádky (pole polí) spolehlivější.
    - I kdybychom úspěšně alokovali např. 16GB paměti, bude to stačit jen pro výpočty s 8GB užitečných dat (půlka paměti jsou zbytečné nuly)

2D pole má jakožto souvislý blok paměti skvělou cache locality, ale pole polí také (řádky jsou stále dlouhé souvislé bloky paměti)

Dalším možným řešením by bylo alokovat souvislý blok paměti (int[]) s vlastním přepočítáváním indexů [row][col]. Výhodou je 100% využití paměti a výborná cache locality.
Nevýhodou je složitější implementace a problém s případným alokováním velkého bloku v rámci fragmentované paměti.

### Úpravy kódu
V rámci poskytnuté šablony projektu jsem provedl několik změn:

1. Místo .NET 8 jsem použil .NET 10 (omlouvám se, ale verzi 8 u sebe nemám nainstalovanou. Kód by měl být ale kompatibilní s verzí 8).
2. **Immutability:** Kód třídy Pyramid jsem zkrátil a v indexeru povolil pouze getter
    - Není totiž potřeba, aby se pyramida po vytvoření měnila. 
    - Pokud to lze, upřednostňuju read-only data, např. kvůli bezpečnému sdílení ve vícevláknovém prostředí, navíc nechtěné změny mohou způsobit bug.
3. **Unit testy:** Přidal jsem vlastní unit testy k pokrytí variant vstupu
4. **Validace:** Přidal jsem validaci parametrů apod.

## 3. Komponentní diagram
(obrázek viz. **Diagram_sipky.png** v rootu gitu NEBO v příloze emailu)

Základ úlohy je, že nějaká firma provozuje interní desktopovou aplikaci a zároveň veřejně dostupnou webovou aplikaci. Účelem je zamyslet se nad jednotlivými komponentami a navrhnout kompletní řešení.

### Základní rozdělení
- Internet = vnější svět (uživatelé, konzumenti API, cloud)
- Vnější firewall, který odfiltruje vnější komunikaci, zachová pouze povolená pravidla, např. komunikaci http(s)
- Zóna DMZ jakožto přechod z veřejného internetu do LAN
- Vnitřní firewall, který slouží jako poslední přísná bariéra při přijímání komunikace zvenčí. Zároveň filtruje komunikaci z LAN do veřejné sítě.
   - Kdyby došlo k napadení DMZ či nginx, vnitřní firewall ochrání samotnou LAN
- Samotná LAN, kde sídlí všechna firemní infrastruktura

### Nginx
- Reverzní proxy, která přesměrovává příchozí komunikaci na konkrétní endpointy (veřejná webová aplikace či různé API)
- Stačí https komunikace, v případě provozování SignalR je nutné povolit dlouhodobé websockety, aby SignalR nevyužil fallbackové implementace, jako např. polling.
- Cachuje statický obsah webových aplikací

### LAN
- Důvěryhodná vnitřní síť, která obsahuje veškerou firemní infrastrukturu

#### Desktop aplikace
- Běží přímo v LAN, proto komunikuje s API přímo
- Nemá přímý přístup k SQL kvůli bezpečnosti a konzistenci přístupu (API poskytuje stejné validace vstupu i přidanou logiku jak pro desktop, tak i pro ostatní konzumenty)

#### Webová aplikace a API
- Hostováno přímo v LAN, s vnějším světem je spojeno přes reverzní proxy a firewally, které propouští pouze bezpečnou komunikaci (zvenku https, lokálně někdy i http)
- Díky umístění webového serveru (potažmo kubernetes) do LAN vzniká přímý přístup k SQL, Redis apod., což je v pořádku, jelikož se již nacházíme v důvěryhodném vnitřním prostředí

#### Kubernetes
- V tomto příkladu využit jako provozní vrstva pro webovou aplikaci a všechna API
- Pokud nějaká služba vypadne, kubernetes nastartuje novou instanci
- Pokud je nějaká služba přetížená, kubernetes ji zreplikuje - tím **roste throughput** systému
- Tento přístup se hodí zejména pokud bude potřeba nějaké API nebo **mikroservisu horizontálně škálovat**

#### SQL
- Zvyšuje dostupnost (automatický failover při výpadku primárního node) a balancuje zátěž dotazů SELECT mezi dostupné repliky. Zápisové dotazy směruje na primární node.
- Nepoužívá kubernetes, jelikož se z principu příliš neškáluje a není stateless

#### Redis
- Slouží jako **distribuovaná cache**, která odlehčuje SQL serveru
  - V distribuovaných systémech je právě SQL často bottleneckem, jelikož se špatně horizontálně škáluje. Při mnoha paralelních dotazech dochází k četným lockům dat a **deadlockům**
- Redis je jednoduché a rychlé RAM úložiště typu key-value, dotazy do něj jsou mnohem rychlejší než SQL.
- Pokud se v rámci obsluhy http requestu použije Redis místo SQL, **klesá latence** 
- Do redis cache se nejlépe hodí cool data (málokdy se mění), či snapshoty nějakých aktuálních dat
  - Kdyby nastal kompletní výpadek SQL, bude alespoň část dat k dispozici (odstranění **single point of failure**). Někdy je lepší zobrazit naposledy načtený snapshot dat s poznámkou o výpadku, než zahodit každý request.

#### Messaging systémy
- V tomto příkladu nevyužity
- Technicky silný základ pro asynchronní komunikaci (producenti posílají zprávy, konzumenti je odebírají)
- **Decoupling komponent**: při produkování zpráv není ani potřeba, aby byl konzument zrovna v provozu. To je největší rozdíl oproti např. přímé HTTP komunikaci, která může být pro komponenty systému příliš svazující a při neošetřeném používání může vést i ke **kaskádovým selháním**

#### AD
- uchovává uživatele, jejich skupiny, role a práva. Je dobře integrovaná v samotném ekosystému Windows
- Desktopové aplikace s AD nemusí komunikovat přímo, stačí se spolehnout na Windows - provádí ověření a podporuje SSO (tedy není nutné se po přihlášení k PC znovu přihlašovat do dalších služeb) 

#### Identity provider
- Slouží webovým aplikacím a API
  - Pomocí LDAPS ověřuje nová přihlášení oproti AD
  - Vydává tokeny pro OAuth2

#### OAuth2
- Při přihlašování uživatele se kontaktuje identity provider, který vydá nový podepsaný JWT token.
- Výhoda: webová služba nemusí při každém requestu ověřovat uživatelův token přes identity provider, ale ověří si ho lokálně díky tomu, že zná veřejné klíče identity provideru
   - To **snižuje latenci**, protože není nutná další mezikomunikace při každém requestu
   - Brání to vzniku **bottlenecku** v identity provideru
   - Lokální ověření tokenu je schopné ověřit vše: identitu uživatele, jeho role, práva apod.
   - Při výpadku identity provideru se nemůže přihlásit žádný další uživatel. Již přihlášení uživatelé zůstanou funkční až do **expirace tokenu**

#### Observability
- V rámci LAN bych použil nějaký centrální systém pro strukturované **logování a metriky**, ideálně napojit na **Grafana**
- V distribuovaném systému bych pro logování použil také **Korelační ID**, které dává do souvislosti více částí komunikace mezi komponentami 