# Case study
Vypracoval Martin Sebera, srpen 2026

## 1. SQL dotaz

## 2. Pyramida
**Uspořádání v paměti**: vrchol pyramidy má index [rows-1, 0]; Základna pyramidy má tyto indexy: [0, 0...rows-1]

### Algoritmus
Cílem je najít maximální součet v pyramidě, na každém políčku je jedno číslo, začíná se na špičce (tedy dole) a postupuje nahoru.
Moje implementace je zcela obecná a funguje tedy i pro záporná čísla.

Součástí zadání není najít kompletní max cestu, pouze najít maximální součet, to je ta jednodušší varianta.

1. Není třeba zkoušet konkrétní cesty, stačí sestupně procházet úrovně od špičky po nejširší (_rows-1...0) a pamatovat si pro každé políčko zatím nejvyšší možný součet.
    - Budeme tedy potřebovat dvě pole (stačí na stacku - lepší pro GC) o délce _rows. Jedno pro procházení tohoto řádku, druhé uchovává maxima řádku pod ním.
2. Když dojdeme na nejširší řádek 0, spočítáme poslední maximální součty.
3. V součtech nalezneme maximum a to vrátíme

### Výhrady k použitému 2D poli int[,]
1. Neefektivní využívání paměti, protože pracujeme pouze s polovinou alokovaného prostoru, zbytek jsou nuly. 
2. Kdybychom použili pole polí int[][], mohli bychom každému řádku nastavit ideální délku.
    - Trade-off: vytváření více objektů (_rows polí místo jednoho pole) a tím více práce pro GC. Ale stálo by to za to.
    - Trade-off: Pole polí zavádí další úroveň dereference, což znamená horší cache locality a mírné zpomalení při čtení z paměti
3. Pro výpočty velkých pyramid je plýtvání paměti kritické:
    - ve fragmentované paměti často nelze alokovat souvislý blok o velikosti několik GB, proto je rozdělení alokací na řádky (pole polí) spolehlivější.
    - I kdybychom úspěšně alokovali např. 16GB paměti, bude to stačit jen pro výpočty s 8GB dat (půlka paměti jsou zbytečné nuly)

2D pole má jakožto souvislý blok paměti skvělou cache locality, ale pole polí také (řádky jsou stále dlouhé souvislé bloky paměti)

Dalším možným řešením by bylo alokovat souvislý blok paměti (int[]) s vlastním přepočítáváním indexů [row][col]. Výhodou je 100% využití paměti a výborná cache locality.
Nevýhodou je složitější implementace a problém s alokováním velkého bloku v rámci fragmentované paměti.

### Úpravy kódu
1. Místo .NET 8 jsem použil .NET 10, což např. povoluje další optimalizace.
2. Kód třídy Pyramid jsem zkrátil a v indexeru povolil pouze getter
    - Není totiž potřeba, aby se pyramida po vytvoření měnila. 
    - Pokud to lze, upřednostňuju read-only data, např. kvůli bezpečnému sdílení ve vícevláknovém prostředí, navíc nechtěné změny mohou způsobit bug.
3. Přidal jsem vlastní unit testy k pokryté všech zajímavých variant vstupu
4. Přidal jsem validaci parametrů apod.

## 3. Komponentní diagram