## Zadanie 1:
Program uruchomiono 5 razy poleceniem `dotnet run -c Release -- 8`. 
Kolejność wypisów za każdym razem była inna, a łączny czas wynosił od 80 do 94 ms (93, 80, 85, 86, 94 ms).

Z czego wynika różnica?
Kolejność wybiera planista systemu operacyjnego, a nie program. Po każdym `Sleep(10)` wątki budzą się prawie jednocześnie i planista za każdym razem ustawia je w innej kolejności.

IsBackground = true bez Join:
program zakończył się po 1 ms, a wątki doszły tylko do kroku 1 (17 linii zamiast 49). Linia „Auto-03: krok 1” pojawiła się nawet po „Łączny czas”, bo wątki działały jeszcze chwilę po zakończeniu pomiaru. 
`Main` nie czeka na wątki, a wątki tła nie podtrzymują działania procesu, więc zostają przerwane w trakcie pracy. 
W projekcie samochód mógłby zostać przerwany w połowie przejazdu, z zajętymi blokadami, dlatego wątki trzeba kończyć w kontrolowany sposób i czekać na nie przez `Join`.

## Zadanie 2:
8 wątków po 1 000 000 inkrementacji (poprawny wynik 8 000 000), średnie z 5 uruchomień w Release:

| Wersja | Wynik | Czas |
|---|---|---|
| bez synchronizacji | 2 990 739 | 6,2 ms |
| `lock` | 8 000 000 | 354,8 ms |
| `Interlocked` | 8 000 000 | 94,6 ms |

Dlaczego licznik++ nie jest atomowy?
W kodzie IL to cztery instrukcje:

    IL_0000: ldsfld int32 C::counter    // odczyt
    IL_0005: ldc.i4.1                   // liczba 1
    IL_0006: add                        // dodanie
    IL_0007: stsfld int32 C::counter    // zapis

Drugi wątek może wejść między odczyt a zapis. Oba wątki zapisują wtedy tę samą wartość i jedno zwiększenie ginie.

Dlaczego lock jest wolniejszy, a mimo to w projekcie potrzebny częściej?
`Interlocked` to jedna instrukcja procesora, a `lock` za każdym razem zajmuje i zwalnia blokadę, na którą inne wątki czekają.
`Interlocked` obsługuje jednak tylko jedną operację na jednej zmiennej, a w projekcie trzeba chronić kilka kroków naraz, na przykład sprawdzić, czy odcinek jest wolny, i go zająć.

## Zadanie 3:
10 samochodów startujących co 20 ms przejeżdża 20 odcinków, a każdy odcinek ma własną blokadę. Wątek kontrolny co 10 ms sprawdza, czy żaden samochód nie dogonił ani nie wyprzedził poprzednika.
Wersja 1 (najpierw zajmij następny odcinek, potem zwolnij bieżący): 0 naruszeń w każdym z 5 uruchomień.
Wersja 2 (najpierw zwolnij bieżący, potem zajmij następny): od 13 do 67 naruszeń (67, 61, 13, 37, 29).

Jak wątek kontrolny musi czytać stan, aby nie zgłaszać fałszywych naruszeń?
Musi odczytać pozycje wszystkich samochodów naraz, pod tą samą blokadą (`stateLock`), pod którą samochody je zmieniają. Inaczej mógłby połączyć starą pozycję jednego samochodu z nową pozycją drugiego.

Co się stanie, jeśli samochód najpierw zwolni bieżący odcinek?
Przez chwilę nie trzyma żadnego odcinka, więc samochód z tyłu może wjechać na jego odcinek albo zająć następny przed nim. Licznik naruszeń przestaje wtedy wynosić 0.

## Zadanie 4:
4 samochody jednocześnie skręcają w lewo z czterech wjazdów (zasoby N = 0, E = 1, S = 2, W = 3). 
Każdy zajmuje swój wjazd, czeka 50 ms i zajmuje drogę, w którą skręca (z N na E, z E na S, z S na W, z W na N). Przy kolejności z przykładu w treści (S, potem E) powstaje to samo koło, tylko w drugą stronę.

Wersja 1 (bez poprawek): zakleszczenie, które strażnik wykrył po 2 s bez postępu:

    Car N holds N and waits for E
    Car E holds E and waits for S
    Car S holds S and waits for W
    Car W holds W and waits for N

Każdy samochód trzyma jeden zasób i czeka na zasób sąsiada, więc oczekiwanie tworzy zamknięte koło.

Wersja 2 (globalna kolejność): wszystkie 4 samochody przejechały w każdym uruchomieniu.
Samochody zajmują zasoby zawsze od niższego numeru, więc koło oczekiwania nie może powstać.
Łamię warunek Coffmana czekania cyklicznego. Pozostałe warunki to wzajemne wykluczanie, trzymanie i oczekiwanie oraz brak wywłaszczania.

Wersja 3 (TryEnter z wycofaniem): przy limicie 1000 ms w 2 z 5 uruchomień wszystkie samochody wycofały się naraz, wróciły prawie w tej samej chwili i znowu się zablokowały, więc przez 2 s nikt nie przejechał i strażnik zgłosił brak postępu.
To przykład livelocku: samochody ciągle zajmują i oddają zasoby, a żaden nie przejeżdża.
Po zmniejszeniu limitu do 100 ms wszystkie 4 samochody przejechały w każdym z 5 uruchomień, po 6 lub 7 wycofaniach (7, 6, 7, 6, 6).
Livelock ogranicza losowa przerwa przed ponowną próbą i krótki limit czekania: samochody wracają w różnych chwilach i szybko próbują ponownie.

## Zadanie 5:
16 wątków pracuje w pętli, a wątek główny włącza na 5 s pauzę. Zużycie procesora zmierzono przez `Process.GetCurrentProcess().TotalProcessorTime` przed pauzą i po niej, a opóźnienie reakcji jako czas od wznowienia do chwili, w której zauważył je najwolniejszy wątek.

A (`while (paused) { }`): procesor 69 953 ms, reakcja 12,39 ms.
B (`Sleep(1)`): procesor 78 ms, reakcja 18,47 ms.
C (`Monitor.Wait` i `PulseAll`): procesor 0 ms, reakcja 33,57 ms.
A zajmuje procesor przez całą pauzę (prawie 70 s czasu procesora w ciągu 5 s), B budzi się co 1 ms, a C śpi, dopóki `PulseAll` go nie obudzi. C reaguje najwolniej, bo obudzone wątki muszą po kolei odzyskać blokadę.

A bez volatile: żaden z 16 wątków nie zobaczył wznowienia, wszystkie kręciły się w pętli do końca programu.
JIT w Release odczytuje flagę tylko raz i trzyma ją w rejestrze, bo w pętli nic jej nie zmienia, a model pamięci na to pozwala, gdy zmienna nie jest volatile. Volatile wymusza odczyt z pamięci przy każdym obrocie pętli.

C z if zamiast while: 16 komunikatów „woke up although the pause is still on!”, po jednym od każdego wątku. W połowie pauzy wątek główny wywołuje `PulseAll` bez wyłączania pauzy.
Z while wątek po obudzeniu sprawdza warunek ponownie i znowu zasypia, a z if od razu pracuje dalej, mimo trwającej pauzy.

## Zadanie 6:
Test: 8 wątków przez około 10 s odczytuje `Teraz` i sprawdza, czy czas się nie cofa, a wątek główny co 300 ms losowo zmienia mnożnik (od 1 do 20) i włącza albo wyłącza pauzę.
Wynik: 211 456 182 odczytów, czas cofnął się 0 razy. Czas symulacji na końcu wyniósł 46 625 ms.

Jak zegar zachowuje monotoniczność przy zmianie mnożnika?
Przy każdej zmianie (mnożnik, pauza, wznowienie) zegar najpierw zapisuje bieżący czas symulacji jako nowy punkt startowy, a dopiero od niego liczy dalej z nowym ustawieniem. Czas płynie więc dalej od tego samego miejsca, bez skoku do tyłu ani do przodu.
Wszystkie operacje są pod jednym `lock`, więc żaden wątek nie odczyta stanu w połowie zmiany.

## Zadanie 7:
Rondo z 5 segmentami, 10 samochodów jeździ przez 5 s. Każdy przejazd zaczyna się na losowym segmencie i zajmuje od 1 do 4 kolejnych segmentów (po segmencie 4 jest znowu 0).

Średnie z 3 uruchomień w Release:
strategia 1 (globalna kolejność, jak wersja 2 w zad 4): 77,3 przejazdu/s, 0 wycofań,
strategia 2 (`TryEnter` z limitem 10 ms i wycofaniem, jak wersja 3 w zad 4): 69,7 przejazdu/s, około 1500 wycofań.

Dlaczego nie ma zakleszczenia?
W strategii 1 wszystkie samochody zajmują segmenty w rosnącej kolejności numerów (`Array.Sort`), więc koło oczekiwania nie może powstać (złamany warunek czekania cyklicznego).
W strategii 2 samochód, który w ciągu 10 ms nie dostanie kolejnego segmentu, sam oddaje wszystkie zajęte (złamany warunek braku wywłaszczania), więc nikt nie czeka w nieskończoność. Losowa przerwa przed kolejną próbą ogranicza livelock.

Strategia 1 ma o około 10% większą przepustowość, bo w strategii 2 samochody tracą czas na nieudane próby: czekają do 10 ms, trzymając zajęte już segmenty, a potem oddają je bez przejazdu.