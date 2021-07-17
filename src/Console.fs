[<AutoOpen>]
module App.Console


#if DEBUG
    let info x = Browser.Dom.console.info x
    let warn x = Browser.Dom.console.warn x
#else
    let info x = ()
    let warn x = ()
#endif