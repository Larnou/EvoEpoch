# Архитектура

Код разделён по механикам, а не по слоям. В `Scripts/Buildings` рядом лежат definition, state и service одного здания. Слой виден по имени класса.

Сборка одна: `Assets/EpochGame/Scripts/EpochGame.asmdef`, корневой namespace `EpochGame`. Более узкие namespace задаются в файлах, например `EpochGame.Buildings` и `EpochGame.Presentation`.

`Content` хранит ScriptableObject-ассеты. Класс реестра `GameDatabase` будет в `Scripts/Database`, сам ассет — в `Content`.

`GameBootstrap` позже только собирает состояние, базу и сервисы. Игровых правил в нём нет.

Сохранение — JSON в `Application.persistentDataPath`. В State попадают ресурсы, население, исследованные технологии, построенные здания, уровень, прогресс развития и назначенные работники. Туда не попадают цена, производство в секунду, вместимость и флаги «можно улучшить».
