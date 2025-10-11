# ban
cmd-ban-desc = Банит кого-либо
cmd-ban-help = Использование: <имя или идентификатор пользователя> <причина> [продолжительность в минутах, не указывайте или укажите 0 для постоянного запрета] [используйте True для глобального запрета, в противном случае False]
cmd-ban-player = Не удалось найти игрока с таким именем.
cmd-ban-invalid-minutes = {$minutes} — недопустимое количество минут!
cmd-ban-invalid-severity = {$severity} — недопустимое значение уровня серьёзности!
cmd-ban-invalid-arguments = Недопустимое количество аргументов
cmd-ban-hint = <name/user ID>
cmd-ban-hint-reason = <reason>
cmd-ban-hint-duration = [duration]
cmd-ban-hint-severity = [severity]

cmd-ban-hint-duration-1 = Навсегда
cmd-ban-hint-duration-2 = 1 день
cmd-ban-hint-duration-3 = 3 дня
cmd-ban-hint-duration-4 = 1 неделя
cmd-ban-hint-duration-5 = 2 недели
cmd-ban-hint-duration-6 = 1 месяц
# listbans
# ban panel
cmd-banpanel-desc = Открывает панель банов.
cmd-banpanel-help = Использование: banpanel [имя или GUID пользователя]
cmd-banpanel-server = Эту команду нельзя использовать из консоли сервера.
cmd-banpanel-player-err = Указанный игрок не найден.

# listbans
cmd-banlist-desc = Список активных банов пользователя.
cmd-banlist-help = Использование: banlist <name or user ID>
cmd-banlist-empty = Нет активных банов у пользователя { $user }
cmd-banlistF-hint = <name/user ID>
cmd-ban_exemption_update-desc = Установите исключение для определенного типа запрета для игрока.
cmd-ban_exemption_update-help =
    Использование: ban_exemption_update <игрок> <флаг> [<флаг> [...]]
    Укажите несколько флагов, чтобы предоставить игроку несколько флагов освобождения от бана.
    Чтобы удалить все исключения, запустите эту команду и укажите "None" в качестве единственного флага.
cmd-ban_exemption_update-nargs = Ожидалось по крайней мере 2 аргумента
cmd-ban_exemption_update-locate = Не удается найти игрока '{ $player }'.
cmd-ban_exemption_update-invalid-flag = Недопустимый флаг '{ $flag }'.
cmd-ban_exemption_update-success = Обновлены флаги исключения из запрета для '{ $player }' ({ $uid }).
cmd-ban_exemption_update-arg-player = <игрок>
cmd-ban_exemption_update-arg-flag = <флаг>
cmd-ban_exemption_get-desc = Показать исключения из бана для определенного игрока.
cmd-ban_exemption_get-help = Использование: ban_exemption_get <игрок>
cmd-ban_exemption_get-nargs = Ожидается 1 аргумент
cmd-ban_exemption_get-none = Пользователь не освобождается от каких-либо запретов.
cmd-ban_exemption_get-show = Пользователь освобожден от следующих флагов запрета: { $flags }.
cmd-ban_exemption_get-arg-player = <игрок>

# Kick on ban
# Ban panel
ban-panel-title = Панель банов
ban-panel-player = Игрок
ban-panel-ip = IP-адрес
ban-panel-hwid = HWID
ban-panel-reason = Причина
ban-panel-last-conn = Использовать IP и HWID из последнего подключения?
ban-panel-submit = Забанить
ban-panel-confirm = Вы уверены?
ban-panel-tabs-basic = Основная информация
ban-panel-tabs-reason = Причина
ban-panel-tabs-players = Список игроков
ban-panel-tabs-role = Информация о бане роли
ban-panel-no-data = Вы должны указать пользователя, IP или HWID для бана.
ban-panel-invalid-ip = Не удалось распознать IP-адрес. Пожалуйста, попробуйте снова.
ban-panel-select = Выберите тип
ban-panel-server = Бан на сервере
ban-panel-role = Бан роли
ban-panel-minutes = Минуты
ban-panel-hours = Часы
ban-panel-days = Дни
ban-panel-weeks = Недели
ban-panel-months = Месяцы
ban-panel-years = Годы
ban-panel-permanent = Навсегда
ban-panel-ip-hwid-tooltip = Оставьте пустым и отметьте флажок ниже, чтобы использовать данные последнего подключения.
ban-panel-severity = Строгость:
ban-panel-erase = Стереть сообщения чата и удалить игрока из раунда

# Ban string
server-ban-string = {$admin} создал серверный бан с уровнем серьезности {$severity}, срок действия которого истекает через {$expires} для [{$name}, {$ip}, {$hwid}] с причиной: {$reason}
server-ban-string-no-pii = {$admin} создал серверный бан с уровнем серьезности {$severity}, срок действия которого истекает через {$expires} для {$name} с причиной: {$reason}
server-ban-string-never = никогда

# Kick on ban
ban-kick-reason = Вы были забанены
