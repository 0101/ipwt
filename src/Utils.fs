[<AutoOpen>]
module App.Utils


open System


let Or = Option.defaultValue


module Set =
    let ofOption = function Some value -> Set.singleton value | None -> Set.empty
    
    
let isWeekDay (d: DateTime) = d.DayOfWeek <> DayOfWeek.Saturday && d.DayOfWeek <> DayOfWeek.Sunday

let isMondayTuesdayWednesday (date: DateTime) = date.DayOfWeek = DayOfWeek.Monday ||
                                                date.DayOfWeek = DayOfWeek.Tuesday ||
                                                date.DayOfWeek = DayOfWeek.Wednesday 

let dayOn date = { Date = date
                   DayType = DayOn
                   Description = None }

let dayOff date = { Date = date
                    DayType = DayOff
                    Description = None }

let partialDay date hours =
    let percentage = hours / 8M
    { Date = date
      DayType = PartialDay percentage
      Description = Some $"Partial day - %.1f{hours} hours" }
    
    
let dayStatus = function
    | DayOn -> "Yep"
    | PartialDay _ -> "Kinda"
    | _ -> "Nope"


module Events = 

    let on year month day = DateTime(year, month, day), DateTime(year, month, day) 
    let from year month day = DateTime(year, month, day)
    let to' = from
    