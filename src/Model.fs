module App.Model

open System

open System.Diagnostics
open App.Events


let FIRST_MONTH = DateTime(2021, 7, 1)
let PART_TIME_RATIO = 0.6M
let DAILY_HOURS = 8M
let PART_TIME_DAILY_HOURS = PART_TIME_RATIO * DAILY_HOURS

    
let eventDates event = [for x in [0..(event.EndDate - event.StartDate).Days] -> event.StartDate.AddDays(float x).Date]    


let getEventsFor (month: DateTime) (holidays: Map<int, Event list>) =
    EVENTS
    |> Seq.map (fun ((start, end'), type', desc) -> {
        StartDate = start
        EndDate = end'
        EventType = type'
        Description = if String.IsNullOrWhiteSpace desc then type'.DisplayText else desc })
    |> Seq.append (holidays.TryFind month.Year |> Or [])
    |> Seq.where (fun e -> e.StartDate.Year = month.Year && e.StartDate.Month = month.Month ||
                           e.EndDate.Year = month.Year && e.EndDate.Month = month.Month)
    |> Seq.toList
    

module AllocationPolicy =

    /// Allocates all the part-time working hours to the beginning of the month     
    let BeginningOfMonth remainingHours remainingDates =
        let hoursLeft, days =
            ((remainingHours, []), remainingDates |> Set.toList |> List.sort)
            ||> List.fold (fun (remainingHours, days) date ->
                let hours, day =
                    if date |> isWeekDay && remainingHours >= DAILY_HOURS then
                        DAILY_HOURS, dayOn date
                    elif date |> isWeekDay && remainingHours > 0M then
                        remainingHours, partialDay date remainingHours 
                    else
                        0M, dayOff date
                remainingHours - hours, day::days )
        Debug.Assert(hoursLeft <= 0M, "Some hours are left after BeginningOfMonth allocation")
        days

    /// Allocates the part-time working hours to Monday through Wednesday as much as possible    
    let ThreeDaysAWeek remainingHours remainingDates =
        let hoursLeft, days, remainingDates =
            ((remainingHours, [], remainingDates), remainingDates)
            ||> Set.fold (fun (remainingHours, days, remainingDates) date ->
                let hours, day =
                    if (date |> isMondayTuesdayWednesday && remainingHours >= DAILY_HOURS) then
                        DAILY_HOURS, [dayOn date]
                    elif date |> isMondayTuesdayWednesday && remainingHours > 0M then
                        remainingHours, [partialDay date remainingHours]
                    else
                        0M, []
                remainingHours - hours, days @ day, Set.difference remainingDates (set [for d in day -> d.Date]) )
        [yield! days
         yield! BeginningOfMonth hoursLeft remainingDates]
 
    
let initMonth holidays (month: DateTime) =
    let events = getEventsFor month holidays 
    let firstDay = DateTime(month.Year, month.Month, 1)
    let dates =
        Seq.initInfinite (float >> firstDay.AddDays)
        |> Seq.takeWhile (fun x -> x.Month = firstDay.Month)
        |> Seq.toList
    
    let weekdays = dates |> Seq.where isWeekDay |> Seq.length
    
    let workingHours = (decimal weekdays) * DAILY_HOURS
    let partTimeHours = workingHours * PART_TIME_RATIO
    
    let eventDates = [for event in events do
                      for date in eventDates event do
                          if date.Month = month.Month then 
                              date, event]
    
    // events that reduce the number of working days
    let deductions = eventDates
                     |> List.choose (fun (_, e) ->
                         match e.EventType with
                         | PublicHoliday | Vacation | SickDay -> Some e.EventType
                         | _ -> None)  
                     |> List.countBy id
    let workingDays = weekdays - (deductions |> List.sumBy snd) 
    
    // process event days 
    let remainingDates, remainingHours, eventDays =
        ((Set dates, partTimeHours, []), eventDates)
        ||> List.fold (fun (remainingDates, remainingHours, days) (eventDate, event) ->
            let hours = match event.EventType with
                        | DayOn -> DAILY_HOURS
                        | PublicHoliday | Vacation | SickDay -> PART_TIME_DAILY_HOURS
                        | PartialDay hours -> hours
                        | DayOff -> 0M
            let day = {
                Date = eventDate
                DayType = event.EventType
                Description = Some event.Description }
            remainingDates |> Set.remove eventDate, (remainingHours - hours), day::days)

    // allocate working days to the remaining space according to chosen policy    
    let regularDays = AllocationPolicy.BeginningOfMonth remainingHours remainingDates

    // Add naturally occuring partial days to events list
    let regularPartialDays = regularDays |> List.choose (function
        | { DayType = PartialDay _ } as d -> Some {
            StartDate = d.Date
            EndDate = d.Date
            Description = d.Description |> Or "Partial day"
            EventType = d.DayType }
        | _ -> None)
    
    let events = [yield! events
                  yield! regularPartialDays] 
        
    let allDays = [yield! eventDays
                   yield! regularDays] 
    
    
    // Check that allocated working hours add up to expected number
    let checksum = allDays
                   |> List.sumBy (function | { DayType = DayOn } -> DAILY_HOURS
                                           | { DayType = PartialDay percentage } ->
                                               percentage * DAILY_HOURS
                                           | _ -> 0M)
    Debug.Assert((checksum = decimal workingDays * DAILY_HOURS * PART_TIME_RATIO), $"Checksum failed for {month}!")
    
    { Date = firstDay
      Days = allDays |> List.sortBy (fun d -> d.Date)
      Events = events |> List.sortBy (fun e -> e.StartDate)
      Weekdays = weekdays
      Deductions = deductions
      WorkingDays = workingDays }

let initPreviousMonth holidays (date: DateTime) =
    if date.AddMonths -1 < FIRST_MONTH
    then None
    else Some (date.AddMonths -1) |> Option.map (initMonth holidays)
    
let initNextMonth holidays (date: DateTime) = Some (initMonth holidays (date.AddMonths 1)) 


let initState (today: DateTime) =
    let holidays = Map []
    { Today = today
      TodayStatus = ""
      Holidays = holidays
      Month = initMonth holidays today
      PreviousMonth = initPreviousMonth holidays today
      NextMonth = initNextMonth holidays today
      HighlightDates = Set.empty }
    
    
let init () =
    let today = DateTime.Today
    initState today, Holidays.Fetch today.Year