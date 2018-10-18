﻿namespace Aardvark.UI.Primitives

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI

type EmbeddedResources = EmbeddedResources

[<AutoOpen>]
module private GlobalizationHelpers = 
    open System
    open System.Globalization

    let parseStable (s : string) =
        let cleaned = s.Replace(",",".")
        try 
            Double.Parse(cleaned,NumberStyles.Any,CultureInfo.InvariantCulture) |> Result.Ok
        with e -> Result.Error e.Message


[<AutoOpen>]
module Simple =
    open System
    open Microsoft.FSharp.Reflection

    let private uniqueClass str = "unique-" + str

    let private unionToCases<'a when 'a : comparison> =
        FSharpType.GetUnionCases(typeof<'a>,true) |> Array.map (fun c -> unbox<'a>(FSharpValue.MakeUnion(c, [||], true)), c.Name) |> Map.ofArray

    let private enumToCases<'a when 'a : comparison> =
        let names = Enum.GetNames(typeof<'a>)
        let values = Enum.GetValues(typeof<'a>) |> unbox<'a[]>
        Array.zip values names |> Map.ofArray

    module Plain =

        let labeledIntegerInput' (containerAttributes : AttributeMap<'msg>) (labelAttributes : AttributeMap<'msg>) (labelSize : Option<int>) (name : string) (minValue : int) (maxValue : int) (changed : int -> 'msg) (value : IMod<int>) =
            let defaultValue = max 0 minValue
            let labelSize = Option.defaultValue name.Length labelSize
            let parse (str : string) =
                let str = str.Replace(".", "").Replace(",", "")
                match System.Int32.TryParse str with
                    | (true, v) -> v
                    | _ -> defaultValue

            let changed =
                AttributeValue.Event  {
                    clientSide = fun send src -> 
                        String.concat "" [
                            "if(!event.inputType && event.target.value != event.target.oldValue) {"
                            "   event.target.oldValue = event.target.value;"
                            "   " + send src ["event.target.value"] + ";"
                            "}"
                        ]
                    serverSide = fun client src args ->
                        match args with
                            | a :: _ -> 
                                let str : string = Pickler.unpickleOfJson a 
                                match System.Int32.TryParse str with
                                    | (true,v) -> clamp minValue maxValue v |> changed |> Seq.singleton
                                    | _ -> 
                                        Log.warn "[media, labeledIntegerInput] could not parse: %s as int." str
                                        Seq.empty
                            | _ -> Seq.empty
                }

            Incremental.div containerAttributes <| 
                alist {
                    yield Incremental.div labelAttributes (AList.ofList [ text name ])
                    yield Incremental.input <|
                        AttributeMap.ofListCond [
                            yield always <| attribute "type" "number"
                            yield always <| attribute "step" "1"
                            if minValue > System.Int32.MinValue then yield always <| attribute "min" (string minValue)
                            if maxValue < System.Int32.MaxValue then yield always <| attribute "max" (string maxValue)

                            yield always <| attribute "placeholder" name
                            yield always <| attribute "size" (string labelSize)
                    
                            yield always <| ("oninput", changed)
                            yield always <| ("onchange", changed)

                            yield "value", value |> Mod.map (string >> AttributeValue.String >> Some)

                        ]
                }

        let labeledFloatInput' (containerAttribs : AttributeMap<'msg>) (labelAttribs : AttributeMap<'msg>) (name : string) (minValue : float) (maxValue : float) (step : float) (changed : float -> 'msg) (value : IMod<float>)  =
            let changed =
                AttributeValue.Event  {
                    clientSide = fun send src -> 
                        String.concat "" [
                            "if(!event.inputType && event.target.value != event.target.oldValue) {"
                            "   event.target.oldValue = event.target.value;"
                            "   " + send src ["event.target.value"] + ";"
                            "}"
                        ]
                    serverSide = fun client src args ->
                        match args with
                            | a :: _ -> 
                                let s : string = Pickler.unpickleOfJson a 
                                match GlobalizationHelpers.parseStable s with
                                    | Result.Ok v -> 
                                        (clamp minValue maxValue v ) |> changed |> Seq.singleton
                                    | Result.Error e -> 
                                        Log.warn "[Media, NumericInput.Simple] could not parse float: %s (%s)" s e
                                        Seq.empty
                            | _ -> Seq.empty
                }

            Incremental.div containerAttribs <|
                AList.ofList [
                    Incremental.div labelAttribs (AList.ofList [ text name ])
                    Incremental.input <|
                        AttributeMap.ofListCond [
                            yield always <| attribute "type" "number"
                            yield always <| attribute "step" (string step)
                            yield always <| attribute "min" (string minValue)
                            yield always <| attribute "max" (string maxValue)

                            yield always <| attribute "placeholder" name
                            yield always <| attribute "size" "4"
                    
                            yield always <| ("oninput", changed)
                            yield always <| ("onchange", changed)

                            yield "value", value |> Mod.map (string >> AttributeValue.String >> Some)

                        ]
                ]

        let dropDown<'a, 'msg when 'a : comparison and 'a : equality> (att : AttributeMap<'msg>) 
                (current : IMod<'a>) (update : 'a -> 'msg) (names : Map<'a, string>) : DomNode<'msg> =
        
            let mutable back = Map.empty
            let forth = 
                names |> Map.map (fun a s -> 
                    let id = newId()
                    back <- Map.add id a back
                    id
                )
        
            let selectedValue = current |> Mod.map (fun c -> Map.find c forth)
        
            let boot = 
                String.concat "\r\n" [
                    sprintf "__ID__.selectedIndex = %d;" (Mod.force selectedValue)
                    "current.onmessage = function(v) { __ID__.selectedIndex = v; };"
                ]

            let attributes = 
                AttributeMap.union att (AttributeMap.ofList [(onChange (fun str -> Map.find (str |> int) back |> update))])

            onBoot' ["current", Mod.channel selectedValue] boot  (
                Incremental.select attributes <|
                    AList.ofList [
                        for (value, name) in Map.toSeq names do
                            let v = Map.find value forth
                            yield option [attribute "value" (string v)] [ text name ]
                    ]
            )

        let dropDownEnum (att : AttributeMap<'msg>) (current : IMod<'a>) (update : 'a -> 'msg) =
            dropDown att current update enumToCases<'a> 

    module SemUi =

        open Plain

        let integerInput (name : string) (minValue : int) (maxValue : int) (changed : int -> 'msg) (value : IMod<int>) =
            let defaultValue = max 0 minValue
            let parse (str : string) =
                let str = str.Replace(".", "").Replace(",", "")
                match System.Int32.TryParse str with
                    | (true, v) -> v
                    | _ -> defaultValue

            let changed =
                AttributeValue.Event  {
                    clientSide = fun send src -> 
                        String.concat "" [
                            "if(!event.inputType && event.target.value != event.target.oldValue) {"
                            "   event.target.oldValue = event.target.value;"
                            "   " + send src ["event.target.value"] + ";"
                            "}"
                        ]
                    serverSide = fun client src args ->
                        match args with
                            | a :: _ -> 
                                let str : string = Pickler.unpickleOfJson a 
                                str |> int |> changed |> Seq.singleton
                            | _ -> Seq.empty
                }

            div [ clazz "ui input"; style "width: 60pt"] [
                Incremental.input <|
                    AttributeMap.ofListCond [
                        yield always <| attribute "type" "number"
                        yield always <| attribute "step" "1"
                        if minValue > System.Int32.MinValue then yield always <| attribute "min" (string minValue)
                        if maxValue < System.Int32.MaxValue then yield always <| attribute "max" (string maxValue)

                        yield always <| attribute "placeholder" name
                        yield always <| attribute "size" "4"
                    
                        yield always <| ("oninput", changed)
                        yield always <| ("onchange", changed)

                        yield "value", value |> Mod.map (string >> AttributeValue.String >> Some)

                    ]
            ]

        let labeledIntegerInput (name : string) (minValue : int) (maxValue : int) (changed : int -> 'msg) (value : IMod<int>) =
            labeledIntegerInput' (AttributeMap.ofList [ clazz "ui small labeled input"; style "width: 60pt"]) (AttributeMap.ofList [ clazz "ui label" ]) None name minValue maxValue changed value

        let labeledFloatInput (name : string) (minValue : float) (maxValue : float) (step : float) (changed : float -> 'msg) (value : IMod<float>) =
            labeledFloatInput' (AttributeMap.ofList [ clazz "ui small labeled input"; style "width: 60pt"]) (AttributeMap.ofList [ clazz "ui label" ]) name minValue maxValue step changed value 

        let modal (id : string) (name : string) (ok : 'msg) (attributes : list<string * AttributeValue<'msg>>) (content : list<DomNode<'msg>>) =
            require Html.semui (
                onBoot "$('#__ID__').modal();" (
                    div [ yield clazz ("ui modal " + uniqueClass id); yield! attributes ] [
                        div [ clazz "header" ] [ text name ]
                        div [ clazz "content" ] content
                        div [ clazz "actions" ] [
                            div [ clazz "ui green approve button"; onClick (fun () -> ok) ] [text "OK"]
                            div [ clazz "ui red cancel button" ] [text "Cancel"]
                        ]
                    ]
                )
            )

        let onClickModal (id : string) =
            let clazz = uniqueClass id
            clientEvent "onclick" (sprintf "$('.%s').modal('show');" clazz)

        let longString (len : int) (str : string) =
            if str.Length > len then
                str.Substring(0, len - 3) + "..."
            else
                str

        let dropDown<'a, 'msg when 'a : comparison and 'a : equality> (att : list<string * AttributeValue<'msg>>) (current : IMod<'a>) (update : 'a -> 'msg) (names : Map<'a, string>) : DomNode<'msg> =
        
            let mutable back = Map.empty
            let forth = 
                names |> Map.map (fun a s -> 
                    let id = newId()
                    back <- Map.add id a back
                    id
                )
        
            let selectedValue = current |> Mod.map (fun c -> Map.find c forth)
        
            let boot = 
                String.concat "\r\n" [
                    sprintf "$('#__ID__').dropdown().dropdown('set selected', %d);" (Mod.force selectedValue)
                    "current.onmessage = function(v) { $('#__ID__').dropdown('set selected', v); };"
                ]

            onBoot' ["current", Mod.channel selectedValue] boot  (
                select ((onChange (fun str -> Map.find (str |> int) back |> update))::att) [
                    for (value, name) in Map.toSeq names do
                        let v = Map.find value forth
                        yield option [attribute "value" (string v)] [ text name ]
                ]
            )

        let allValues<'a when 'a : comparison> =
            FSharpType.GetUnionCases(typeof<'a>,true) |> Array.map (fun c -> unbox<'a>(FSharpValue.MakeUnion(c, [||], true)), c.Name) |> Map.ofArray


