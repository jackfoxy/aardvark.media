﻿namespace Niobe

open System

open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.Base.Rendering
open Aardvark.SceneGraph

open Aardvark.Application
open Aardvark.UI
open Aardvark.UI.Primitives
open Niobe.Sketching

module App = 
     
  let update (model : Model) (msg : Message) =
      match msg with        
        | ToggleShadowVolumeVis -> {model with shadowVolumeVis = not model.shadowVolumeVis }
        | Camera m when model.picking = false ->
          { model with cameraState = FreeFlyController.update model.cameraState m; }
        | UpdateDockConfig cfg ->
          { model with dockConfig = cfg }
        | SketchingMessage a ->
          { model with sketching = SketchingApp.update model.sketching a }
        | HitSurface p when model.picking = true ->
          Log.line "hit %A" p
          { model with sketching = SketchingApp.update model.sketching (AddPoint p) }          
        | Message.KeyDown k -> 
          match k with
          | Keys.LeftCtrl -> 
            Log.line "start picking"
            { model with picking = true }
          | Keys.Enter -> { model with sketching = SketchingApp.update model.sketching (ClosePolygon) }
          | _ -> model
        | Message.KeyUp k ->
          match k with
          | Keys.LeftCtrl -> 
            Log.line "stop picking"
            { model with picking = false }
          | _ -> model
        | _ -> model
  
  let view (model : MModel) =
     
    let writeZFailFront =
        StencilMode(StencilOperationFunction.Keep, StencilOperationFunction.DecrementWrap, StencilOperationFunction.Keep, StencilCompareFunction.Always, 0, 0xffu)
    
    let writeZFailBack =
        StencilMode(StencilOperationFunction.Keep, StencilOperationFunction.IncrementWrap, StencilOperationFunction.Keep, StencilCompareFunction.Always, 0, 0xffu)
    
    let readZFail = 
        StencilMode(StencilOperationFunction.Keep, StencilOperationFunction.Keep, StencilOperationFunction.Replace, StencilCompareFunction.NotEqual, 0, 0xffu)

    let sceneSG = 
      Sg.box (Mod.constant C4b.Red) (Mod.constant Box3d.Unit)
        |> Sg.shader {
              do! DefaultSurfaces.trafo
              do! DefaultSurfaces.simpleLighting
          }  
        |> Sg.depthTest (Mod.constant DepthTestMode.Less)   // depth test active
        |> Sg.pass RenderPass.main
        |> Sg.requirePicking
            |> Sg.noEvents
            |> Sg.withEvents [
                Sg.onClick HitSurface
            ]

    let shadowvolume = 
      Sg.box (Mod.constant C4b.Blue) (Mod.constant (Box3d(V3d(0.0), V3d(0.5))))
        |> Sg.translate 0.75 0.75 0.8 
        |> Sg.shader {
              do! DefaultSurfaces.trafo
              do! DefaultSurfaces.vertexColor
          }

    let shodowvolumeVis =
      Sg.box (Mod.constant C4b.Blue) (Mod.constant (Box3d(V3d(0.0), V3d(0.5))))
        |> Sg.translate 0.75 0.75 0.8 
        |> Sg.depthTest (Mod.constant DepthTestMode.Less)   // depth test active
        |> Sg.onOff model.shadowVolumeVis
        |> Sg.pass RenderPass.main
        |> Sg.shader {
              do! DefaultSurfaces.trafo
              do! DefaultSurfaces.constantColor (C4f(1.0f, 1.0f, 0.0f, 0.3f))
          }

    let maskPass = RenderPass.after "mask" RenderPassOrder.Arbitrary RenderPass.main
    let areaPass = RenderPass.after "area" RenderPassOrder.Arbitrary maskPass
    let linePass = RenderPass.after "lines" RenderPassOrder.Arbitrary areaPass

    let lineSG = 
      SketchingApp.viewSg model.sketching 
       |> Sg.map SketchingMessage 
       |> Sg.pass linePass 
       |> Sg.depthTest (Mod.constant DepthTestMode.None)

    // Front = Clockwise
    // Back = CounterClockwise

    // TODO...ask harri || georg: EXT_stencil_tow_side -> to render mask in one pass instead of two seperate!

    let maskFrontSG = 
      shadowvolume
       |> Sg.pass maskPass
       |> Sg.stencilMode (Mod.constant (writeZFailFront))
       |> Sg.cullMode (Mod.constant (CullMode.Clockwise))
       |> Sg.writeBuffers' (Set.ofList [DefaultSemantic.Stencil])

    let maskBackSG = 
      shadowvolume
       |> Sg.pass maskPass
       |> Sg.stencilMode (Mod.constant (writeZFailBack))
       |> Sg.cullMode (Mod.constant (CullMode.CounterClockwise))
       |> Sg.writeBuffers' (Set.ofList [DefaultSemantic.Stencil])

    let areaSG =
      shadowvolume
       |> Sg.pass areaPass
       |> Sg.stencilMode (Mod.constant (readZFail))
       |> Sg.cullMode (Mod.constant CullMode.Clockwise)  // backface-culling
       |> Sg.depthTest (Mod.constant DepthTestMode.Less) // active depth-test to reduce clipp area 
       |> Sg.blendMode (Mod.constant BlendMode.Blend)
       |> Sg.writeBuffers' (Set.ofList [DefaultSemantic.Colors])

    let viewSg = 
      [
        sceneSG
        lineSG
        maskFrontSG
        maskBackSG
        areaSG
        shodowvolumeVis
      ] |> Sg.ofList

    let renderControl =
     FreeFlyController.controlledControl model.cameraState Camera (Frustum.perspective 60.0 0.01 1000.0 1.0 |> Mod.constant) 
       (AttributeMap.ofList [ 
         style "width: 100%; height:100%"; 
         attribute "showFPS" "false";       // optional, default is false
         attribute "useMapping" "true"
         attribute "data-renderalways" "false"
         attribute "data-samples" "4"
         onKeyDown (Message.KeyDown)
         onKeyUp (Message.KeyUp)
         //onBlur (fun _ -> Camera FreeFlyController.Message.Blur)
       ]) 
       (viewSg)
                        
    let dependencies = 
      Html.semui @ [        
        { name = "spectrum.js";  url = "spectrum.js";  kind = Script     }
        { name = "spectrum.css";  url = "spectrum.css";  kind = Stylesheet     }
      ] 

    page (fun request ->
      match Map.tryFind "page" request.queryParams with
      | Some "render" ->
        require Html.semui ( // we use semantic ui for our gui. the require function loads semui stuff such as stylesheets and scripts
            div [clazz "ui"; style "background: #1B1C1E"] [renderControl]
        )
      | Some "controls" -> 
        require dependencies (
          body [style "width: 100%; height:100%; background: transparent; overflow-y:visible"] [
             div[style "color:white; margin: 5px 15px 5px 5px"] [
               h3[][text "NIOBE"]
               p[][text "unified sketching"]
               p[][text "Hold Ctrl-Left to add Point"]
               p[][text "Press Enter to close Polygon"]
               p[][text "ShadowVolumeVisualization "; Html.SemUi.iconCheckBox model.shadowVolumeVis ToggleShadowVolumeVis]
               button[onClick (fun _ -> SketchingAction.Undo); style "background:white; color:black"][text "Undo"] |> UI.map SketchingMessage
               button[onClick (fun _ -> SketchingAction.Redo); style "background:white; color:black"][text "Redo"] |> UI.map SketchingMessage
               //button[onClick (fun _ -> SketchingAction.CreateShadowPolygon); style "background:white; color:black"][text "Shadow Volume"] |> UI.map SketchingMessage
               SketchingApp.viewGui model.sketching |> UI.map SketchingMessage
             ]
          ]
        )
      | Some other -> 
        let msg = sprintf "Unknown page: %A" other
        body [] [
            div [style "color: white; font-size: large; background-color: red; width: 100%; height: 100%"] [text msg]
        ]  
      | None -> 
        model.dockConfig
          |> docking [
            style "width:100%; height:100%; background:#F00"
            onLayoutChanged UpdateDockConfig ]
      )
   
  let threads (model : Model) = 
      ThreadPool.empty
  
  let initialCamera = { FreeFlyController.initial with view = CameraView.lookAt (V3d.III * 2.0) V3d.OOO V3d.OOI; }

  let initialModel : Model = 
    { 
      cameraState        = initialCamera                                
      threads            = FreeFlyController.threads initialCamera |> ThreadPool.map Camera                            
      dockConfig         =
        config {
            content (
                horizontal 10.0 [
                    element { id "render"; title "Render View"; weight 7.0 }
                    element { id "controls"; title "Controls"; weight 3.0 }                                                    
                ]
            )
            appName "OpcSelectionViewer"
            useCachedConfig true
        }
      picking = false
      sketching = Sketching.Initial.sketchingModel
      shadowVolumeVis = false
    }
  
  let app =                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       
      {
          unpersist = Unpersist.instance     
          threads = threads 
          initial = initialModel
          update = update 
          view = view
      }
