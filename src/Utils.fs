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

let getMonthDates (month: DateTime) =
    let firstDay = DateTime(month.Year, month.Month, 1)
    Seq.initInfinite (float >> firstDay.AddDays)
    |> Seq.takeWhile (fun x -> x.Month = firstDay.Month)
    |> Seq.toList


let dayOn date = { Date = date
                   DayType = DayOn
                   Description = None }

let dayOff date = { Date = date
                    DayType = DayOff
                    Description = None }

let partialDay date hours =
    let dayType = PartialDay hours
    { Date = date
      DayType = dayType
      Description = Some dayType.DisplayText }

let dayToEvent (d: Day) = {
    StartDate = d.Date
    EndDate = d.Date
    Description = d.Description |> Or ""
    EventType = d.DayType }


let dayStatus = function
    | DayOn -> "Yep"
    | PartialDay _ -> "Kinda"
    | _ -> "Nope"


module Events = 

    let on year month day = DateTime(year, month, day), DateTime(year, month, day) 
    let from year month day = DateTime(year, month, day)
    let to' = from
