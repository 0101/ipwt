module App.Events

open App.Utils.Events


let EVENTS = [
    on 2021 7 29, DayOn, ".NET Conf"
    (from 2021 8 23, to' 2021 8 27), DayOn, "Azure Bootcamp"
    on 2021 8 4, DayOff, ""
    on 2021 11 30, PartialDay 4.8m, ""
    (from 2021 12 13, to' 2021 12 23), Vacation, ""
]
