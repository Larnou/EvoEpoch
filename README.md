# EvoEpoch

<p align="left">
  <a href="https://unity.com/">
    <img alt="Unity 6000.6.8f1" src="https://img.shields.io/badge/Unity-6000.6.8f1-111111?style=for-the-badge&logo=unity&logoColor=white">
  </a>
  <a href="https://learn.microsoft.com/dotnet/csharp/">
    <img alt="C#" src="https://img.shields.io/badge/C%23-EpochGame-68217A?style=for-the-badge&logo=csharp&logoColor=white">
  </a>
  <a href="https://github.com/Larnou/EvoEpoch">
    <img alt="Android portrait" src="https://img.shields.io/badge/Android-Portrait-3DDC84?style=for-the-badge&logo=android&logoColor=white">
  </a>
  <img alt="URP 2D" src="https://img.shields.io/badge/URP-2D-2196F3?style=for-the-badge">
  <img alt="Prototype" src="https://img.shields.io/badge/Status-Ice%20Age%20prototype-F5A623?style=for-the-badge">
</p>

Мобильная idle-игра про развитие поселения. Первый прототип — начало ледникового периода: ручной сбор еды и дерева, технологии, первые здания, население и автоматическое производство.

Репозиторий: [github.com/Larnou/EvoEpoch](https://github.com/Larnou/EvoEpoch).  
Игровой код живёт в namespace `EpochGame`. Имя Unity-проекта — `EvoEpoch`.

## Открыть проект

1. Unity Hub → редактор **6000.6.8f1**.
2. **Add** → `R:\Coding\EvoEpoch`.
3. Внешний редактор скриптов: **JetBrains Rider**.
4. После импорта открой **Window → General → Console**. Красных ошибок быть не должно.

Платформа сборки пока Windows. Android-сборка появится, когда прототип будет играться в редакторе.

## Как устроен код

Четыре слоя, без отдельной папки на каждый:

| Слой | Что это | Пример |
| --- | --- | --- |
| Definition | Статическая настройка, обычно ScriptableObject | `BuildingDefinition` |
| State | Данные этого игрока, обычный C# класс | `BuildingState` |
| Service | Расчёты и правила | `BuildingService` |
| Presentation | MonoBehaviour, UI, объекты сцены | `BuildingView` |

View не считает цену, производство и доступность улучшения. Сохраняются только исходные данные. Всё, что можно вычислить, в сохранение не пишется.

## Папки

```text
Assets/
├── EpochGame/
│   ├── Scripts/          C# и EpochGame.asmdef
│   │   ├── Core/
│   │   ├── Economy/
│   │   ├── Gathering/
│   │   ├── Technology/
│   │   ├── Buildings/
│   │   ├── Population/
│   │   ├── Production/
│   │   ├── Era/
│   │   ├── Save/
│   │   ├── Database/
│   │   └── Presentation/
│   ├── Content/          ScriptableObject-ассеты
│   ├── Prefabs/
│   └── Art/Placeholder/
├── Scenes/
└── Tests/EditMode/
```

`Docs/` описывает архитектуру, игровой цикл и временный баланс. Это ещё не реализация.

## Сейчас в прототипе нет

Боя, героев, инвентаря, квестов, рекламы, платежей, RuStore, сервера и офлайн-производства. Графика пока заглушки.
