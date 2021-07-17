module Tests

open App
open Xunit
open System.IO
open FSharp.Data

open App.Model


type HolidayData = JsonProvider<holidaySampleFile>

let loadHolidays year =
    let file = Path.Combine(__SOURCE_DIRECTORY__, "..", "public", Holidays.holidayFile year)
    if File.Exists file then
        [for event in HolidayData.Load file do
             // disregard public holidays on weekends
             if isWeekDay event.Date then {
                 StartDate = event.Date
                 EndDate = event.Date
                 Description = event.Name
                 EventType = PublicHoliday }]
    else []


[<Fact>]
let ``We can generate valid data for all months`` () =
    for month in Seq.initInfinite FIRST_MONTH.AddMonths |> Seq.takeWhile (fun d -> d.Year < 2049) do
        let holidays = Map [month.Year, loadHolidays month.Year]
        // assertions are built in
        initMonth holidays month |> ignore


[<Fact>]
let ``We can generate valid state for any day`` () =
    let random = System.Random()
    let days = Seq.init 10000 (fun _ -> (random.Next(0, 365*20)) |> float |> FIRST_MONTH.AddDays)
    for date in days do
        // assertions are built in
        initState date |> ignore



[<EntryPoint>] 
let main _ = 0
