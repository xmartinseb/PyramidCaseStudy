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

1. Není tak třeba nahlížet na konkrétní cesty, stačí sestupně procházet řádky od špičky po nejširší (_rows-1 ... 0) a pamatovat si pro každé políčko zatím nejvyšší možný součet.
    - Budeme tedy potřebovat dvě pole. Jedno pro iteraci aktuálního řádku, druhé uchovává maxima řádku pod ním.
    - Každé políčko má pod sebou jedno nebo dvě sousední políčka. Stačí tedy vybírat maximálně ze dvou variant.
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
(obrázek viz. root v gitu)

Základ úlohy je, že nějaká firma provozuje interní desktopovou aplikaci a zároveň veřejně dostupnou webovou aplikaci. Účelem je zamyslet se nad jednotlivými komponentami a navrhnout kompletní řešení.