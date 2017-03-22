namespace Demo.TestApp

open System
open Aardvark.Base
open Aardvark.Base.Incremental

[<AutoOpen>]
module Mutable =

    [<StructuredFormatDisplay("{AsString}")>]
    type MModel private(__initial : Demo.TestApp.Model) =
        let mutable __current = __initial
        let _lastName = ResetMod(__initial.lastName)
        let _elements = ResetList(__initial.elements)
        let _hasD3Hate = ResetMod(__initial.hasD3Hate)
        let _boxScale = ResetMod(__initial.boxScale)
        let _boxHovered = ResetMod(__initial.boxHovered)
        let _dragging = ResetMod(__initial.dragging)
        
        member x.lastName = _lastName :> IMod<_>
        member x.elements = _elements :> alist<_>
        member x.hasD3Hate = _hasD3Hate :> IMod<_>
        member x.boxScale = _boxScale :> IMod<_>
        member x.boxHovered = _boxHovered :> IMod<_>
        member x.dragging = _dragging :> IMod<_>
        
        member x.Update(__model : Demo.TestApp.Model) =
            if not (Object.ReferenceEquals(__model, __current)) then
                __current <- __model
                _lastName.Update(__model.lastName)
                _elements.Update(__model.elements)
                _hasD3Hate.Update(__model.hasD3Hate)
                _boxScale.Update(__model.boxScale)
                _boxHovered.Update(__model.boxHovered)
                _dragging.Update(__model.dragging)
        
        static member Create(initial) = MModel(initial)
        
        override x.ToString() = __current.ToString()
        member private x.AsString = sprintf "%A" __current
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module MModel =
        let inline lastName (m : MModel) = m.lastName
        let inline elements (m : MModel) = m.elements
        let inline hasD3Hate (m : MModel) = m.hasD3Hate
        let inline boxScale (m : MModel) = m.boxScale
        let inline boxHovered (m : MModel) = m.boxHovered
        let inline dragging (m : MModel) = m.dragging
    
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module Model =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let lastName =
                { new Lens<Demo.TestApp.Model, Microsoft.FSharp.Core.Option<Microsoft.FSharp.Core.string>>() with
                    override x.Get(r) = r.lastName
                    override x.Set(r,v) = { r with lastName = v }
                    override x.Update(r,f) = { r with lastName = f r.lastName }
                }
            let elements =
                { new Lens<Demo.TestApp.Model, Aardvark.Base.plist<Microsoft.FSharp.Core.string>>() with
                    override x.Get(r) = r.elements
                    override x.Set(r,v) = { r with elements = v }
                    override x.Update(r,f) = { r with elements = f r.elements }
                }
            let hasD3Hate =
                { new Lens<Demo.TestApp.Model, Microsoft.FSharp.Core.bool>() with
                    override x.Get(r) = r.hasD3Hate
                    override x.Set(r,v) = { r with hasD3Hate = v }
                    override x.Update(r,f) = { r with hasD3Hate = f r.hasD3Hate }
                }
            let boxScale =
                { new Lens<Demo.TestApp.Model, Microsoft.FSharp.Core.float>() with
                    override x.Get(r) = r.boxScale
                    override x.Set(r,v) = { r with boxScale = v }
                    override x.Update(r,f) = { r with boxScale = f r.boxScale }
                }
            let boxHovered =
                { new Lens<Demo.TestApp.Model, Microsoft.FSharp.Core.bool>() with
                    override x.Get(r) = r.boxHovered
                    override x.Set(r,v) = { r with boxHovered = v }
                    override x.Update(r,f) = { r with boxHovered = f r.boxHovered }
                }
            let dragging =
                { new Lens<Demo.TestApp.Model, Microsoft.FSharp.Core.bool>() with
                    override x.Get(r) = r.dragging
                    override x.Set(r,v) = { r with dragging = v }
                    override x.Update(r,f) = { r with dragging = f r.dragging }
                }
    [<StructuredFormatDisplay("{AsString}")>]
    type MCameraControllerState private(__initial : Demo.TestApp.CameraControllerState) =
        let mutable __current = __initial
        let _view = ResetMod(__initial.view)
        let _dragStart = ResetMod(__initial.dragStart)
        let _look = ResetMod(__initial.look)
        let _zoom = ResetMod(__initial.zoom)
        let _pan = ResetMod(__initial.pan)
        
        member x.view = _view :> IMod<_>
        member x.dragStart = _dragStart :> IMod<_>
        member x.look = _look :> IMod<_>
        member x.zoom = _zoom :> IMod<_>
        member x.pan = _pan :> IMod<_>
        
        member x.Update(__model : Demo.TestApp.CameraControllerState) =
            if not (Object.ReferenceEquals(__model, __current)) then
                __current <- __model
                _view.Update(__model.view)
                _dragStart.Update(__model.dragStart)
                _look.Update(__model.look)
                _zoom.Update(__model.zoom)
                _pan.Update(__model.pan)
        
        static member Create(initial) = MCameraControllerState(initial)
        
        override x.ToString() = __current.ToString()
        member private x.AsString = sprintf "%A" __current
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module MCameraControllerState =
        let inline view (m : MCameraControllerState) = m.view
        let inline dragStart (m : MCameraControllerState) = m.dragStart
        let inline look (m : MCameraControllerState) = m.look
        let inline zoom (m : MCameraControllerState) = m.zoom
        let inline pan (m : MCameraControllerState) = m.pan
    
    
    
    
    [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
    module CameraControllerState =
        [<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
        module Lens =
            let view =
                { new Lens<Demo.TestApp.CameraControllerState, Aardvark.Base.CameraView>() with
                    override x.Get(r) = r.view
                    override x.Set(r,v) = { r with view = v }
                    override x.Update(r,f) = { r with view = f r.view }
                }
            let dragStart =
                { new Lens<Demo.TestApp.CameraControllerState, Aardvark.Base.V2i>() with
                    override x.Get(r) = r.dragStart
                    override x.Set(r,v) = { r with dragStart = v }
                    override x.Update(r,f) = { r with dragStart = f r.dragStart }
                }
            let look =
                { new Lens<Demo.TestApp.CameraControllerState, Microsoft.FSharp.Core.bool>() with
                    override x.Get(r) = r.look
                    override x.Set(r,v) = { r with look = v }
                    override x.Update(r,f) = { r with look = f r.look }
                }
            let zoom =
                { new Lens<Demo.TestApp.CameraControllerState, Microsoft.FSharp.Core.bool>() with
                    override x.Get(r) = r.zoom
                    override x.Set(r,v) = { r with zoom = v }
                    override x.Update(r,f) = { r with zoom = f r.zoom }
                }
            let pan =
                { new Lens<Demo.TestApp.CameraControllerState, Microsoft.FSharp.Core.bool>() with
                    override x.Get(r) = r.pan
                    override x.Set(r,v) = { r with pan = v }
                    override x.Update(r,f) = { r with pan = f r.pan }
                }
