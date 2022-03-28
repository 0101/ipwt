module App.Events

open App.Utils.Events
open App.AllocationPolicy


let EVENTS = [
    on 2021 7 29, DayOn, ".NET Conf"
    (from 2021 8 23, to' 2021 8 27), DayOn, "Azure Bootcamp"
    on 2021 8 4, DayOff, ""
    on 2021 11 30, PartialDay 4.8m, ""
    (from 2021 12 13, to' 2021 12 23), Vacation, ""
    on 2022 1 13, PartialDay 4.8m, ""
    on 2022 1 18, SickDay, ""
    on 2022 1 19, SickDay, ""
    on 2022 1 26, Vacation, ""
    on 2022 1 27, Vacation, ""
    on 2022 1 31, Vacation, ""
    on 2022 4 7, Vacation, ""
    (from 2022 4 19, to' 2022 4 21), Vacation, ""
    (from 2022 4 22, to' 2022 5 10), DayOff, "Trip to US"
    (from 2022 5 11, to' 2022 5 16), Vacation, ""
]

let ALLOCATION_POLICY = [
    from 2022 1 1, AllocationPolicy.ThreeDaysAWeek
]
