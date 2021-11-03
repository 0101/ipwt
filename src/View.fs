module App.View

open Fable.React
open Fable.React.Props
open Browser.Dom

open System

open App.Model


let dateFormat = Date.Format.localFormat Date.Local.englishUS


let eventDate event =
    let start = event.StartDate |> dateFormat "dddd, MMMM d"
    let end'  = event.EndDate   |> dateFormat "dddd, MMMM d"
    if event.StartDate = event.EndDate then start  
    else $"{start} — {end'}"


let dayCell active dispatch highlightDates today (date: DateTime, day: Day option) =
    let isToday = if date.Date = today then " today" else ""
    let highlight = if highlightDates |> Set.contains date  then " highlight" else ""
    let partialStyle = match day with
                       | Some {DayType = PartialDay hours} ->
                           let percentage = hours / 8M
                           let lightness = 100M - 50M * percentage
                           [BackgroundColor ("hsl(116, 67%" + $", %.0f{lightness}" + "%)")]
                       | _ -> []

    match day with
    | None -> td [ ClassName "filler-day" ] [ str $"{date.Day}" ]
    | Some day -> td [
        ClassName $"{day.DayType}{isToday}{highlight}"
        Style partialStyle 
        Title ((day.DayType |> dayStatus) + (day.Description |> Option.map (sprintf ", %s") |> Or ""))
        if active then OnMouseOver (fun _ -> HighlightDates (Set [day.Date]) |> dispatch) ] [ str $"{date.Day}" ]


let calendar dispatch highlight today active = function
    | None -> div [ ClassName "calendar-empty" ] []
    | Some (month: Month) ->
        let firstDay = month.Days.[0].Date
        let leadingDays = Seq.initInfinite ((*) -1 >> float >> firstDay.AddDays)
                          |> Seq.skip 1
                          |> Seq.takeWhile (fun x -> x.DayOfWeek <> DayOfWeek.Sunday)
        let lastDay = (month.Days |> List.last).Date
        let trailingDays = Seq.initInfinite (float >> lastDay.AddDays)
                           |> Seq.skip 1
                           |> Seq.takeWhile (fun x -> x.DayOfWeek <> DayOfWeek.Monday)
        let weeks =
            seq {
                for day in leadingDays -> day, None
                for day in month.Days -> day.Date, Some day
                for day in trailingDays -> day, None
            }
            |> Seq.where (fst >> isWeekDay)
            |> Seq.sort
            |> Seq.chunkBySize 5
            |> Seq.where (Array.exists (snd >> Option.isSome))
            
        div [ ClassName "calendar-month"
              OnMouseLeave (fun _ -> HighlightDates Set.empty |> dispatch)
              if not active then OnClick (fun _ -> SwitchTo month.Date |> dispatch) ] [
            table [] [
                thead [] [ tr [] [ for c in "MTWTF" -> th [] [ str $"{c}" ] ] ]
                tbody [] [
                    for week in weeks -> tr [] [
                        for day in week -> day |> dayCell active dispatch highlight today
                    ]
                ]
            ]
        ]


let view (state : State) dispatch =
    
    document.onkeydown <- (fun e ->
        match e.key, state.PreviousMonth, state.NextMonth with
        | "ArrowLeft", Some month, _ 
        | "ArrowRight", _, Some month -> SwitchTo month.Date |> dispatch
        | "Home", _, _ -> SwitchTo state.Today |> dispatch
        | _ -> ())
    
    let monthName = Date.Format.localFormat Date.Local.englishUS "MMMM" state.Month.Date
    
    let partTimeWorkingDays = $"%.1f{decimal state.Month.WorkingDays * PART_TIME_RATIO}".Replace(".0", "")
    
    let calendar' = calendar dispatch state.HighlightDates state.Today
    let activeCalendar = calendar' true
    let passiveCalendar = calendar' false
    
    div [ ClassName "page-wrapper"; OnKeyPress (fun e -> Console.info e.key) ] [
        div [ ClassName "status-section" ] [
            h2 [] [ str "Is Petr working today?" ]
            h1 [] [ str state.TodayStatus ]
        ]
        div [ ClassName "calendar-section" ] [
            div [ ClassName "active-month" ] [ h3 [] [ str $"{monthName} {state.Month.Date.Year}" ] ]
            div [ ClassName "calendar previous" ] [ passiveCalendar state.PreviousMonth ]
            div [ ClassName "calendar active" ]   [ activeCalendar (Some state.Month) ]
            div [ ClassName "calendar next" ]     [ passiveCalendar state.NextMonth ]
        ]
        div [ ClassName "details-section" ] [
            div [ ClassName "stats" ] [
                p [] [ span [] [ str "Weekdays" ]; strong [] [ str $"{state.Month.Weekdays}" ] ]
                div [ ClassName "deductions" ] [
                    for label, count in state.Month.Deductions do
                        p [] [ span [] [ str $"{label.DisplayText}" ]; strong [] [ str $"-{count}" ] ]
                ]
                p [] [ span [] [ str "Working days" ]; strong [] [ str $"{state.Month.WorkingDays}" ] ]
                p [ ClassName "ratio" ] [ span [] [ str <| $"× %.0f{PART_TIME_RATIO * 100M} " + "%" ]; strong [] [ str partTimeWorkingDays ] ]
            ]
            if state.Month.Events <> [] then
                div [ ClassName "events"; OnMouseLeave (fun _ -> HighlightDates Set.empty |> dispatch) ] [
                    for event in state.Month.Events do
                        let highlight =
                            if state.HighlightDates |> Set.exists (fun date -> date >= event.StartDate && date <= event.EndDate)
                            then " highlight" else  ""
                        div [ ClassName $"event {event.EventType}{highlight}"; OnMouseOver (fun _ -> event |> eventDates |> Set |> HighlightDates |> dispatch) ] [
                            div [ ClassName "date" ] [ str $"{eventDate event}" ]
                            div [ ClassName "description" ] [ str event.Description ]
                        ]
                ]
        ]
        div [ ClassName "footer" ] [
            a [ Href "https://github.com/0101/ipwt" ] [ str "source" ]
            str "|"
            a [ Href "https://github.com/0101/ipwt/issues" ] [ str "report bug" ]
        ]
    ]
