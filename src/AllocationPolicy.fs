module App.AllocationPolicy

open System.Diagnostics


/// After known events are placed in the calendar, allocate the remaining working
/// hours to the remaining free space
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
        Debug.Assert((hoursLeft = 0M), "Some hours are left after BeginningOfMonth allocation")
        days

    /// Allocates the part-time working hours to Monday through Wednesday as much as possible
    /// What doesn't fit goes to the beginning of the month
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
