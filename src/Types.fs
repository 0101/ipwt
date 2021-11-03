[<AutoOpen>]
module App.Types

open System

type DayType =
    | DayOn
    | DayOff 
    | PartialDay of decimal
    | PublicHoliday
    | Vacation 
    | SickDay
    with
        member this.DisplayText = this |> function
            | PublicHoliday -> "Public holiday"
            | SickDay -> "Sick day"
            | DayOff -> "Day off"
            | PartialDay hours -> $"Partial day – %.1f{hours} hours"
            | x -> x.ToString()

type Day = {
    Date: DateTime
    DayType: DayType
    Description: string option
}

type Event = {
    StartDate: DateTime
    EndDate: DateTime
    Description: string
    EventType: DayType
}

type Month = {
    Date: DateTime
    Days: Day list
    Events: Event list
    Weekdays: int
    Deductions: (DayType * int) list
    WorkingDays: int
}

type State = {
    Today: DateTime
    TodayStatus: string
    Month: Month
    PreviousMonth: Month option
    NextMonth: Month option
    HighlightDates: Set<DateTime>
    Holidays: Map<int, Event list>
}

let [<Literal>] holidaySampleFile = "../public/holidays/holidays-2021.json" 
type HolidayData = Fable.JsonProvider.Generator<holidaySampleFile>

type Msg =
    | SwitchTo of DateTime
    | Holidays of int * Event list
    | HighlightDates of Set<DateTime>
