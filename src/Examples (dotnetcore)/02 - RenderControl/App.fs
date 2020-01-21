﻿module RenderControl.App

open Aardvark.UI
open Aardvark.UI.Primitives

open Aardvark.Base
open FSharp.Data.Adaptive
open Aardvark.Base.Rendering
open RenderControl.Model


let initialCamera = { 
        FreeFlyController.initial with 
            view = CameraView.lookAt (V3d.III * 3.0) V3d.OOO V3d.OOI
    }

let update (model : Model) (msg : Message) =
    match msg with
        | Camera m -> 
            { model with cameraState = FreeFlyController.update model.cameraState m }
        | CenterScene -> 
            { model with cameraState = initialCamera }

let viewScene (model : AdaptiveModel) =
    Sg.box (AVal.constant C4b.Green) (AVal.constant Box3d.Unit)
     |> Sg.shader {
            do! DefaultSurfaces.trafo
            do! DefaultSurfaces.vertexColor
            do! DefaultSurfaces.simpleLighting
        }


let view (model : AdaptiveModel) =

    let renderControl =

        FreeFlyController.controlledControl 
            model.cameraState Camera 
            (Frustum.perspective 60.0 0.1 100.0 1.0 |> AVal.constant) 
            (
                AttributeMap.ofList [ 
                    //"onpointerdown", AttributeValue.Capture {
                    //    clientSide = fun send id -> send id []
                    //    serverSide = fun _ _ _ -> Log.warn "capture"; Stop, Seq.empty
                    //}
                        
                    //"onpointerdown", AttributeValue.Bubble {
                    //    clientSide = fun send id -> send id []
                    //    serverSide = fun _ _ _ -> Log.warn "bubble"; Continue, Seq.empty
                    //}

                    //Frontend.HTMLAttribute.OnPointerDown(fun e -> Log.warn "down %A %A %A" e.ClientLocation e.Shift e.Ndc; Continue, None).Captured.WithPointerCapture.ToAttribute()
                    ////Frontend.HTMLAttribute.OnPointerUp(fun e -> Log.warn "up %A" e.Ndc; Continue, None).Captured.WithPointerCapture.ToAttribute()

                    //Frontend.HTMLAttribute.OnPointerEnter(fun e -> Log.warn "enter %A %A %A" e.ClientLocation e.Shift e.Ndc; Continue, None).ToAttribute()
                    //Frontend.HTMLAttribute.OnPointerLeave(fun e -> Log.warn "leave %A %A %A" e.ClientLocation e.Shift e.Ndc; Continue, None).ToAttribute()

                    //Frontend.HTMLAttribute.OnPointerDown(fun e -> Log.warn "down %A" e; Continue, None).Captured.ToAttribute()


                    ////Frontend.HTMLAttribute.OnPointerClick(fun e -> Log.warn "click %A" e.Ndc; Continue, None).ToAttribute()
                    ////Frontend.HTMLAttribute.OnPointerDoubleClick(fun e -> Log.warn "double click %A" e.Ndc; Continue, None).ToAttribute()
                    //Frontend.HTMLAttribute.OnPointerWheel(fun e -> Log.warn "wheel %A" e; if e.Shift then Stop, None else Continue, None).Captured.ToAttribute()

                    style "width: 100%; grid-row: 2; height:100%"; 
                    attribute "showFPS" "true";         // optional, default is false
                    //attribute "showLoader" "false"    // optional, default is true
                    //attribute "data-renderalways" "1" // optional, default is incremental rendering
                    attribute "data-samples" "8"        // optional, default is 1
                ]
            ) 
            (viewScene model)


    div [style "display: grid; grid-template-rows: 40px 1fr; width: 100%; height: 100%" ] [
        div [style "grid-row: 1"] [
            text "Hello 3D"
            br []
            button [onClick (fun _ -> CenterScene)] [text "Center Scene"]
        ]
        renderControl
        br []
        text "use first person shooter WASD + mouse controls to control the 3d scene"
    ]

let threads (model : Model) = 
    FreeFlyController.threads model.cameraState |> ThreadPool.map Camera


let app =                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
    {
        unpersist = Unpersist.instance     
        threads = threads 
        initial = 
            { 
               cameraState = initialCamera
            }
        update = update 
        view = view
    }
