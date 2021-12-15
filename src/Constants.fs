[<AutoOpen>]
module App.Constants

open System

let FIRST_MONTH = DateTime(2021, 7, 1)
let PART_TIME_RATIO = 0.6M
let DAILY_HOURS = 8M
let PART_TIME_DAILY_HOURS = PART_TIME_RATIO * DAILY_HOURS
