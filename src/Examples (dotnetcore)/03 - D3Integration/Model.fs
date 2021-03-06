namespace Model

open Aardvark.Base
open FSharp.Data.Adaptive
open Aardvark.UI
open Aardvark.UI.Primitives
open Adaptify

type Message = 
    | Generate
    | ChangeCount of Numeric.Action


[<ModelType>]
type Model = 
    {
        count : NumericInput
        data : list<float>
    }