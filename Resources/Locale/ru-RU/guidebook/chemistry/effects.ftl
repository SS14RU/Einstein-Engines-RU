-create-3rd-person =
    { $chance ->
        [1] создает
       *[other] Создает
    }
-cause-3rd-person =
    { $chance ->
        [1] вызывает
       *[other] Вызывает
    }
-satiate-3rd-person =
    { $chance ->
        [1] насыщает
       *[other] Насыщает
    }
reagent-effect-guidebook-create-entity-reaction-effect =
    { $chance ->
        [1] создаёт
       *[other] Создаёт
    } { $amount ->
        [1] { INDEFINITE($entname) }
       *[other] { $amount } { MAKEPLURAL($entname) }
    }
reagent-effect-guidebook-explosion-reaction-effect =
    { $chance ->
        [1] вызывает
       *[other] Вызывает
    } взрыв
reagent-effect-guidebook-emp-reaction-effect =
    { $chance ->
        [1] вызывает
        *[other] вызывают
    } электромагнитный импульс

reagent-effect-guidebook-foam-area-reaction-effect =
    { $chance ->
        [1] создаёт
       *[other] Создаёт
    } большое количество дыма
reagent-effect-guidebook-smoke-area-reaction-effect =
    { $chance ->
        [1] создаёт
       *[other] Создаёт
    } большое количество дыма

reagent-effect-guidebook-satiate-thirst =
    { $chance ->
        [1] насыщает
       *[other] Насыщает
    } { $relative ->
        [1] жажду средне
       *[other] жажду на { NATURALFIXED($relative, 3) }x от среднего
    }
reagent-effect-guidebook-satiate-hunger =
    { $chance ->
        [1] насыщает
       *[other] Насыщает
    } { $relative ->
        [1] голод средне
       *[other] голод на { NATURALFIXED($relative, 3) }x от среднего
    }
reagent-effect-guidebook-health-change =
    { $chance ->
        [1]
            { $healsordeals ->
                [heals] Лечит
                [deals] Наносит
               *[both] Изменяет здоровье на
            }
       *[other]
            { $healsordeals ->
                [heals] лечит
                [deals] наносит
               *[both] Изменяет здоровье на
            }
    } { $changes }
reagent-effect-guidebook-status-effect =
    { $type ->
        [add]
            { $chance ->
                [1] Вызывает
               *[other] вызывает
            } { LOC($key) } как минимум на { NATURALFIXED($time, 3) } { $time } с накоплением
       *[set]
            { $chance ->
                [1] Вызывает
               *[other] вызывает
            } { LOC($key) } как минимум на { NATURALFIXED($time, 3) } { $time } без накопления
        [remove]
            { $chance ->
                [1] Убирает
               *[other] убирает
            } { NATURALFIXED($time, 3) } { $time } { LOC($key) }
    }
reagent-effect-guidebook-activate-artifact =
    { $chance ->
        [1] Пытается
       *[other] пытается
    } активировать артефакт
reagent-effect-guidebook-set-solution-temperature-effect =
    { $chance ->
        [1] Устанавливает
       *[other] устанавливает
    } температуру раствора точно { NATURALFIXED($temperature, 2) }К
reagent-effect-guidebook-adjust-solution-temperature-effect =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Добавляет
               *[-1] Убирает
            }
       *[other]
            { $deltasign ->
                [1] добавляет
               *[-1] убирает
            }
    } тепло раствору до тех пор, пока он не достигнет { $deltasign ->
        [1] не больше { NATURALFIXED($maxtemp, 2) }К
       *[-1] не меньше { NATURALFIXED($mintemp, 2) }К
    }
reagent-effect-guidebook-adjust-reagent-reagent =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Добавляет
               *[-1] Убирает
            }
       *[other]
            { $deltasign ->
                [1] добавляют
               *[-1] убирают
            }
    } { NATURALFIXED($amount, 2) }u { $reagent } { $deltasign ->
        [1] в
       *[-1] из
    } раствор
reagent-effect-guidebook-adjust-reagent-group =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Добавляет
               *[-1] Убирает
            }
       *[other]
            { $deltasign ->
                [1] добавляют
               *[-1] убирают
            }
    } { NATURALFIXED($amount, 2) }u реагентов в группе { $group } { $deltasign ->
        [1] в
       *[-1] из
    } раствор
reagent-effect-guidebook-adjust-temperature =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Добавляет
               *[-1] Убирает
            }
       *[other]
            { $deltasign ->
                [1] добавляют
               *[-1] убирают
            }
    } { POWERJOULES($amount) } тепла { $deltasign ->
        [1] в
       *[-1] из
    } организма
reagent-effect-guidebook-chem-cause-disease =
    { $chance ->
        [1] Вызывает
       *[other] вызывают
    } болезнь { $disease }
reagent-effect-guidebook-chem-cause-random-disease =
    { $chance ->
        [1] Вызывает
       *[other] вызывают
    } болезни { $diseases }
reagent-effect-guidebook-jittering =
    { $chance ->
        [1] Вызывает
       *[other] вызывают
    } дрожь
reagent-effect-guidebook-chem-clean-bloodstream =
    { $chance ->
        [1] Очищает
       *[other] очищают
    } кровь от других химикатов
reagent-effect-guidebook-cure-disease =
    { $chance ->
        [1] Исцеляет
       *[other] исцеляют
    } болезни
reagent-effect-guidebook-cure-eye-damage =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Исцеляет
               *[-1] Повреждает
            }
       *[other]
            { $deltasign ->
                [1] исцеляют
               *[-1] повреждают
            }
    } глаза
reagent-effect-guidebook-chem-vomit =
    { $chance ->
        [1] Вызывает
       *[other] вызывают
    } рвоту
reagent-effect-guidebook-create-gas =
    { $chance ->
        [1] Создаёт
       *[other] создают
    } { $moles } { $moles ->
        [1] моль
       *[other] молей
    } { $gas }
reagent-effect-guidebook-drunk =
    { $chance ->
        [1] Вызывает
       *[other] вызывают
    } опьянение
reagent-effect-guidebook-electrocute =
    { $chance ->
        [1] Поражает током
       *[other] поражают током
    } метаболизатор на { NATURALFIXED($time, 3) } { $time }
reagent-effect-guidebook-extinguish-reaction =
    { $chance ->
        [1] Тушит
       *[other] тушат
    } огонь
reagent-effect-guidebook-flammable-reaction =
    { $chance ->
        [1] Повышает
       *[other] повышают
    } воспламеняемость
reagent-effect-guidebook-ignite =
    { $chance ->
        [1] Поджигает
       *[other] поджигают
    } метаболизатор
reagent-effect-guidebook-make-sentient =
    { $chance ->
        [1] Делает
       *[other] делают
    } метаболизатор разумным
reagent-effect-guidebook-make-polymorph =
    { $chance ->
        [1] Превращает
        *[other] превращают
    } метаболизатор в { $entityname }

reagent-effect-guidebook-modify-bleed-amount =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Вызывает
               *[-1] Уменьшает
            }
       *[other]
            { $deltasign ->
                [1] вызывают
               *[-1] уменьшают
            }
    } кровотечение
reagent-effect-guidebook-modify-blood-level =
    { $chance ->
        [1]
            { $deltasign ->
                [1] Повышает
               *[-1] Понижает
            }
       *[other]
            { $deltasign ->
                [1] повышают
               *[-1] понижают
            }
    } уровень крови
reagent-effect-guidebook-paralyze =
    { $chance ->
        [1] Парализует
       *[other] парализуют
    } метаболизатор минимум на { NATURALFIXED($time, 3) } { $time }
reagent-effect-guidebook-movespeed-modifier =
    { $chance ->
        [1] Изменяет
       *[other] изменяют
    } скорость передвижения на { NATURALFIXED($walkspeed, 3) }x минимум на { NATURALFIXED($time, 3) } { $time }
reagent-effect-guidebook-reset-narcolepsy =
    { $chance ->
        [1] Временно подавляет
       *[other] временно подавляют
    } нарколепсию
reagent-effect-guidebook-wash-cream-pie-reaction =
    { $chance ->
        [1] Смывает
       *[other] смывают
    } кремовый пирог с лица
reagent-effect-guidebook-cure-zombie-infection =
    { $chance ->
        [1] Лечит
       *[other] лечат
    } текущую зомби-инфекцию
reagent-effect-guidebook-cause-zombie-infection =
    { $chance ->
        [1] Заражает
       *[other] заражают
    } индивида зомби-инфекцией
reagent-effect-guidebook-innoculate-zombie-infection =
    { $chance ->
        [1] Лечит
       *[other] лечат
    } текущую зомби-инфекцию и даёт иммунитет к повторному заражению

reagent-effect-guidebook-plant-attribute =
    { $chance ->
        [1] Изменяет
        *[other] изменяют
    } {$attribute} на [color={$colorName}]{$amount}[/color]

reagent-effect-guidebook-plant-cryoxadone =
    { $chance ->
        [1] Омолаживает
        *[other] омолаживают
    } растение, в зависимости от его возраста и времени роста

reagent-effect-guidebook-plant-phalanximine =
    { $chance ->
        [1] Делает
        *[other] делают
    } непригодное из-за мутации растение снова пригодным

reagent-effect-guidebook-plant-diethylamine =
    { $chance ->
        [1] Увеличивает
        *[other] увеличивает
    } продолжительность жизни и/или базовое здоровье растения с вероятностью 10% для каждой.

reagent-effect-guidebook-plant-robust-harvest =
    { $chance ->
        [1] Увеличивает
        *[other] увеличивает
    } потенцию растения на {$increase} вплоть до максимума {$limit}. Приводит к потере растением семян, когда потенция достигает {$seedlesstreshold}. Попытка добавить потенцию свыше {$limit} может привести к снижению урожайности с вероятностью 10%.

reagent-effect-guidebook-plant-seeds-add =
    { $chance ->
        [1] Восстанавливает
        *[other] восстанавливают
    } семена растения.

reagent-effect-guidebook-plant-seeds-remove =
    { $chance ->
        [1] Удаляет
        *[other] удаляют
    } семена растения.

reagent-effect-guidebook-missing =
    { $chance ->
        [1] Вызывает
       *[other] вызывают
    } неизвестный эффект, так как никто ещё не описал его

reagent-effect-guidebook-change-glimmer-reaction-effect =
    { $chance ->
        [1] Изменяет
        *[other] изменяют
    } показатель сияния на {$count}

reagent-effect-guidebook-chem-remove-psionic =
    { $chance ->
        [1] Убирает
        *[other] убирают
    } псионические способности

reagent-effect-guidebook-chem-reroll-psionic =
    { $chance ->
        [1] Даёт
        *[other] дают
    } шанс получить другую псионическую способность

reagent-effect-guidebook-chem-restorereroll-psionic =
    { $chance ->
        [1] Восстанавливает
        *[other] восстанавливают
    } способность снова получать пользу от «открывающих разум» реагентов

reagent-effect-guidebook-add-moodlet =
    Изменяет настроение на {$amount}
    { $timeout ->
        [0] бессрочно
        *[other] на {$timeout} секунд
    }

reagent-effect-guidebook-remove-moodlet =
    Убирает эффект настроения {$name}.

reagent-effect-guidebook-purge-moodlets =
    Убирает все активные непостоянные эффекты настроения.

reagent-effect-guidebook-purify-evil = Очищает тёмные силы

reagent-effect-guidebook-stamina-change =
    { $chance ->
        [1] { $deltasign ->
                [-1] Восстанавливает
                *[1] Наносит
            }
        *[other] { $deltasign ->
                    [-1] восстанавливают
                    *[1] наносят
                 }
    } {$amount} выносливости

# Shadowling

reagent-effect-guidebook-blind-non-sling =
    { $chance ->
        [1] Осlepляет
        *[other] осlepляют
    } всех, кто не является теневиком

reagent-effect-guidebook-heal-sling =
    { $chance ->
        [1] Лечит
        *[other] лечат
    } теневика и порабощённых

reagent-effect-guidebook-add-to-chemicals =
    { $chance ->
        [1] { $deltasign ->
                [1] Добавляет
                *[-1] Убирает
            }
        *[other]
            { $deltasign ->
                [1] добавляют
                *[-1] убирают
            }
    } {NATURALFIXED($amount, 2)}u {$reagent} { $deltasign ->
        [1] в
        *[-1] из
    } раствор
