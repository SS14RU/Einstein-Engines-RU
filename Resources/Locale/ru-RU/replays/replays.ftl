# Loading Screen
replay-loading = Загрузка ({ $cur }/{ $total })
replay-loading-reading = Чтение файлов
replay-loading-processing = Обработка файлов
replay-loading-spawning = Создание сущностей
replay-loading-initializing = Инициализация сущностей
replay-loading-starting = Запуск сущностей
replay-loading-failed =
    Не удалось загрузить повтор:
    { $reason }

# Main Menu
replay-loading-retry = Попробовать загрузить с большей терпимостью к ошибкам — МОЖЕТ ВЫЗВАТЬ БАГИ!

# Main Menu
replay-menu-subtext = Клиент повторов
replay-menu-load = Загрузить выбранный повтор
replay-menu-select = Выбрать повтор
replay-menu-open = Открыть папку с повторами
replay-menu-none = Повторов не найдено.

# Main Menu Info Box
replay-info-title = Информация о повторе
replay-info-none-selected = Повтор не выбран
replay-info-invalid = [color=red]Выбран неверный повтор[/color]
replay-info-info =
    { "[" }color=gray]Выбран:[/color]  { $name } ({ $file })
    { "[" }color=gray]Время:[/color]   { $time }
    { "[" }color=gray]ID раунда:[/color]   { $roundId }
    { "[" }color=gray]Длительность:[/color]   { $duration }
    { "[" }color=gray]ForkId:[/color]   { $forkId }
    { "[" }color=gray]Версия:[/color]   { $version }
    { "[" }color=gray]Движок:[/color]   { $engVersion }
    { "[" }color=gray]Type Hash:[/color]   { $hash }
    { "[" }color=gray]Comp Hash:[/color]   { $compHash }

# Replay selection window
replay-menu-select-title = Выбор повтора

# Replay related verbs
replay-verb-spectate = Наблюдать

# command
cmd-replay-spectate-help = replay_spectate [необязательная сущность]
cmd-replay-spectate-desc = Присоединяет или отсоединяет локального игрока к заданному UID сущности.
cmd-replay-spectate-hint = Необязательный EntityUid
