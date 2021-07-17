module App.Holidays

open System
open Elmish
open Fetch
open Fable.Core


let holidayFile year = $"holidays/holidays-{year}.json"

let fetchHolidayData (year: int) = async {
    let! response = fetch $"/{holidayFile year}" [] |> Async.AwaitPromise
    return! response.text() |> Async.AwaitPromise
}

let parseHolidayData text = [
    for event in HolidayData.ParseArray text do
        let date = (DateTime.Parse event.date).Date
        // disregard public holidays on weekends
        if isWeekDay date then {
            StartDate = date
            EndDate = date
            Description = event.name
            EventType = PublicHoliday }
]

let fetchHolidays year = async {
    let! text = fetchHolidayData year
    return year, parseHolidayData text
}

let Fetch year = Cmd.OfAsync.perform fetchHolidays year Holidays
