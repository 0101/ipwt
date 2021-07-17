module App.App

open Browser.Dom
open Elmish
open Elmish.React
open Elmish.Debug


Program.mkProgram
    App.Model.init
    App.Update.update
    App.View.view
#if DEBUG
|> Program.withDebugger
#endif
|> Program.withReactSynchronous "app-container"
|> Program.run
