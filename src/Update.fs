module App.Update

open Elmish

open App.Model
   
    
let update (msg: Msg) (state: State) =
    
    match msg with
    
    | SwitchTo date ->
        let todayStatus =
            if state.TodayStatus = "" then
                let thisMonth = initMonth state.Holidays state.Today
                let today = thisMonth.Days |> List.find (fun x -> x.Date.Date = state.Today.Date)
                dayStatus today.DayType
            else state.TodayStatus
        
        { state with
            TodayStatus = todayStatus
            Month = initMonth state.Holidays date
            PreviousMonth = initPreviousMonth state.Holidays date
            NextMonth = initNextMonth state.Holidays date }, 
        Cmd.batch (
            [ Some date; previousMonth date; nextMonth date ]
            |> List.choose (function
               | Some date when not (state.Holidays.ContainsKey date.Year) -> Some (Holidays.Fetch date.Year)
               | _ -> None))
        
    | Holidays (year, events) ->
        { state with Holidays = state.Holidays.Add (year, events) }, 
        Cmd.ofMsg (SwitchTo state.Month.Date)
            
    | HighlightDates dates ->
        { state with HighlightDates = dates }, 
        Cmd.none
