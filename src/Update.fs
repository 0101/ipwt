module App.Update

open Elmish

open App.Model
   
    
let update (msg: Msg) (state: State) =
    
    match msg with
    
    | SwitchTo date ->
        let todayStatus =
            if state.TodayStatus = "" then
                let thisMonth = initMonth state.Holidays state.Today
                let today = thisMonth.Days |> List.find (fun x -> x.Date = state.Today)
                dayStatus today.DayType
            else state.TodayStatus
        { state with
            TodayStatus = todayStatus
            Month = initMonth state.Holidays date
            PreviousMonth = initPreviousMonth state.Holidays date
            NextMonth = initNextMonth state.Holidays date
            }, if not (state.Holidays.ContainsKey date.Year) then
                   Holidays.Fetch date.Year
               else Cmd.none
        
    | Holidays (year, events) ->
        { state with
            Holidays = state.Holidays.Add (year, events)
            }, Cmd.ofMsg (SwitchTo state.Month.Date)
            
    | HoverDates dates ->
        { state with
            HighlightDates = dates
            }, Cmd.none
