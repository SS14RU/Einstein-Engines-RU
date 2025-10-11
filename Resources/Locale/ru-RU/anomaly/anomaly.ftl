anomaly-component-contact-damage = Аномалия сдирает с вас кожу!

anomaly-vessel-component-anomaly-assigned = Аномалия присвоена сосуду.
anomaly-vessel-component-not-assigned = Этому сосуду не присвоена ни одна аномалия. Попробуйте использовать на нём сканер.
anomaly-vessel-component-assigned = Этому сосуду уже присвоена аномалия.

anomaly-vessel-component-upgrade-output = point output

anomaly-particles-delta = Дельта-частицы
anomaly-particles-epsilon = Эпсилон-частицы
anomaly-particles-zeta = Зета-частицы
anomaly-particles-omega = Омега-частицы

anomaly-particles-sigma = Сигма-частицы

anomaly-scanner-component-scan-complete = Сканирование завершено!

anomaly-scanner-scan-copied = Данные сканирования аномалии скопированы!

anomaly-scanner-ui-title = сканер аномалий
anomaly-scanner-no-anomaly = Нет просканированной аномалии.
anomaly-scanner-severity-percentage = Текущая опасность: [color=gray]{ $percent }[/color]
anomaly-scanner-severity-percentage-unknown = Current severity: [color=red]ERROR[/color]
anomaly-scanner-stability-low = Текущее состояние аномалии: [color=gold]Распад[/color]
anomaly-scanner-stability-medium = Текущее состояние аномалии: [color=forestgreen]Стабильное[/color]
anomaly-scanner-stability-high = Текущее состояние аномалии: [color=crimson]Рост[/color]
anomaly-scanner-stability-unknown = Current anomaly state: [color=red]ERROR[/color]
anomaly-scanner-point-output = Пассивная генерация очков: [color=gray]{ $point }[/color]
anomaly-scanner-point-output-unknown = Point output: [color=red]ERROR[/color]
anomaly-scanner-particle-readout = Анализ реакции на частицы:
anomaly-scanner-particle-danger = - [color=crimson]Опасный тип:[/color] { $type }
anomaly-scanner-particle-unstable = - [color=plum]Нестабильный тип:[/color] { $type }
anomaly-scanner-particle-containment = - [color=goldenrod]Сдерживающий тип:[/color] { $type }
anomaly-scanner-particle-transformation = - [color=#6b75fa]Transformation type:[/color] {$type}
anomaly-scanner-particle-danger-unknown = - [color=crimson]Danger type:[/color] [color=red]ERROR[/color]
anomaly-scanner-particle-unstable-unknown = - [color=plum]Unstable type:[/color] [color=red]ERROR[/color]
anomaly-scanner-particle-containment-unknown = - [color=goldenrod]Containment type:[/color] [color=red]ERROR[/color]
anomaly-scanner-particle-transformation-unknown = - [color=#6b75fa]Transformation type:[/color] [color=red]ERROR[/color]
anomaly-scanner-pulse-timer = Время до следующего импульса: [color=gray]{ $time }[/color]

anomaly-gorilla-core-slot-name = Ядро аномалии
anomaly-gorilla-charge-none = Внутри нет [bold]ядра аномалии[/bold].
anomaly-gorilla-charge-limit = Осталось [color={$count ->
        [3]green
        [2]yellow
        [1]orange
        [0]red
        *[other]purple
    }]{$count} {$count ->
        [one]заряд
        [few]заряда
        *[other]зарядов
    }[/color].

anomaly-gorilla-charge-infinite = В нем [color=gold]неограниченое количество зарядов[/color]. [italic]Пока что...[/italic]

anomaly-sync-connected = Аномалия успешно привязана
anomaly-sync-disconnected = Соединение с аномалией было потеряно!
anomaly-sync-no-anomaly = Отсутствует аномалия в пределах диапазона.
anomaly-sync-examine-connected = Он [color=darkgreen]присоединён[/color] к аномалии.
anomaly-sync-examine-not-connected = Он [color=darkred]не присоединён[/color] к аномалии.
anomaly-sync-connect-verb-text = Присоединить аномалию
anomaly-sync-connect-verb-message = Присоединить близлежащую аномалию к {$machine}.

anomaly-generator-ui-title = Генератор Аномалий
anomaly-generator-fuel-display = Топливо:
anomaly-generator-cooldown = Перезарядка: [color=gray]{ $time }[/color]
anomaly-generator-no-cooldown = Перезарядка: [color=gray]Завершена[/color]
anomaly-generator-yes-fire = Статус: [color=forestgreen]Готов[/color]
anomaly-generator-no-fire = Статус: [color=crimson]Не готов[/color]
anomaly-generator-generate = Создать Аномалию
anomaly-generator-charges =
    { $charges ->
        [one] { $charges } заряд
       *[other] { $charges } заряды
    }

anomaly-generator-announcement = Была сгенерирована аномалия!

anomaly-command-pulse = Пульсирует аномалию
anomaly-command-supercritical = Доводит аномалию до суперкритического состояния

# Flavor text on the footer
# Flavor text on the footer
anomaly-generator-flavor-left = Аномалия может возникнуть внутри пользователя.
anomaly-generator-flavor-right = v1.1
anomaly-behavior-unknown = [color=red]ОШИБКА. Не удаётся считать данные.[/color]

anomaly-behavior-title = анализ отклонений поведения:
anomaly-behavior-point = [color=gold]Аномалия создаёт {$mod}% от общего количества точек[/color]

anomaly-behavior-safe = [color=forestgreen]Аномалия чрезвычайно стабильна. Пульсации крайне редки.[/color]
anomaly-behavior-slow = [color=forestgreen]Частота пульсаций значительно ниже обычной.[/color]
anomaly-behavior-light = [color=forestgreen]Сила пульсаций значительно снижена.[/color]
anomaly-behavior-balanced = Отклонений в поведении не обнаружено.
anomaly-behavior-delayed-force = Частота пульсаций сильно снижена, но их сила увеличена.
anomaly-behavior-rapid = Частота пульсаций значительно выше, но их сила ослаблена.
anomaly-behavior-reflect = Обнаружено защитное покрытие.
anomaly-behavior-nonsensivity = Обнаружена слабая реакция на частицы.
anomaly-behavior-sensivity = Обнаружена усиленная реакция на частицы.
anomaly-behavior-secret = Обнаружены помехи. Часть данных не удаётся считать.
anomaly-behavior-inconstancy = [color=crimson]Обнаружена непостоянность. Типы частиц могут изменяться со временем.[/color]
anomaly-behavior-fast = [color=crimson]Частота пульсаций сильно увеличена.[/color]
anomaly-behavior-strenght = [color=crimson]Сила пульсаций значительно увеличена.[/color]
anomaly-behavior-moving = [color=crimson]Обнаружена нестабильность координат.[/color]
