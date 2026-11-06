# EvoEpoch

Unity 6, C#, namespace `EpochGame`. Проектный каталог Unity называется `EvoEpoch`.

## Слои

`Definition` → статическая конфигурация, в основном ScriptableObject.  
`State` → данные конкретного игрока, обычные сериализуемые классы.  
`Service` → правила и расчёты, обычные классы C#.  
`Presentation` → MonoBehaviour, UI и объекты сцены.

Пример: `BuildingDefinition` + `BuildingState` → `BuildingService` → `BuildingView`.

## Правила

- Бизнес-логика не живёт в MonoBehaviour.
- Runtime-состояние игрока не живёт в ScriptableObject.
- Вычисляемые значения не сохраняются и не дублируются в State.
- Формулы зданий живут в `BuildingBalanceCalculator`.
- Services не зависят от MonoBehaviour.
- Presentation не содержит игровых формул.
- ID стабильные и не совпадают с русским названием: `hunting_camp`, `mineral_extraction`.
- Не добавлять DI-фреймворк, ECS, Zenject, VContainer, Addressables, Rx и глобальный EventBus, пока в этом нет прямой нужды.
- Не создавать `GameManager` на всю игру и глобальные mutable singleton.
- Пустые папки держатся файлом `.gitkeep`. После открытия Unity рядом появляются `.meta` — их нужно коммитить.
- Коммиты делает человек, не агент, пока явно не попросит иначе.
- После серии изменений C# смотреть Unity Console и не идти дальше при красных ошибках.

## Папки кода

`Assets/EpochGame/Scripts` — сборка `EpochGame`.  
`Assets/EpochGame/Content` — ассеты определений.  
`Assets/Tests/EditMode` — тесты, вне игровой сборки.
