# Shown when greeted with the Suspicion role
suspicion-role-greeting = Вы { $roleName }!
# Shown when greeted with the Suspicion role
# Shown when greeted with the Suspicion role
suspicion-objective = Цель: { $objectiveText }
# Shown when greeted with the Suspicion role
# Shown when greeted with the Suspicion role
suspicion-partners-in-crime =
    { $partnersCount ->
        [zero] Вы сам по себе. Удачи!
        [one] Ваш соучастник { $partnerNames }.
       *[other] Ваш соучастники { $partnerNames }.
    }
