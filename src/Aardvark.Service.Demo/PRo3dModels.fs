﻿namespace PRo3DModels

open System
open Aardvark.Base
open Aardvark.Base.Incremental
open Aardvark.UI.Mutable
open Aardvark.UI
open FShade.Primitives

type Points = list<V3d>
type Segment = Points

type Projection = Linear = 0 | Viewpoint = 1 | Sky = 2
type Geometry = Point = 0 | Line = 1 | Polyline = 2 | Polygon = 3

[<DomainType>]
type Annotation = {
        //seqNumber : int
        geometry : Geometry
        points : Points
        segments : list<Segment>
        color : C4b
        thickness : NumericInput
        projection : Projection
        visible : bool
        text : string
    }

module InitValues = 
    let edge = [ V3d.IOI; V3d.III; V3d.OOI ]

    let initAnnotation = 
        {
            geometry = Geometry.Polyline
            points = edge
            segments = [ edge; edge; edge ]
            color = C4b.Red
            thickness = Numeric.init
            projection = Projection.Viewpoint
            visible = true
            text = "my favorite annotation"
        }