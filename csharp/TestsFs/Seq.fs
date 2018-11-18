[<RequireQualifiedAccess>]
module TestsFs.Seq
open System.Linq
type SeqChanges<'Existing, 'Updated>={
  ToChange:('Existing*'Updated) list
  ToAdd:'Updated list
  ToRemove:'Existing list}
let changes (existing:seq<'Existing>) (keyOfExisting:'Existing->'Key) 
            (updated:seq<'Updated>) (keyOfUpdated:'Updated->'Key) : SeqChanges<'Existing,'Updated>=
  let e = existing.ToArray()
  let u = updated.ToArray()
  let dicE = existing.ToDictionary(keyOfExisting, id)
  let dicU = updated.ToDictionary(keyOfUpdated, id)

  let setE = set dicE.Keys
  let setU = set dicU.Keys

  let toAdd = setU.Except(setE) |> Seq.map(fun key -> dicU.[key])|> Seq.toList
  let toRemove = setE.Except(setU) |> Seq.map(fun key -> dicE.[key])|> Seq.toList
  let toChange = setE.Intersect(setU) |> Seq.map( fun key-> (dicE.[key],dicU.[key]) ) |> Seq.toList
  {ToChange= toChange; ToAdd= toAdd; ToRemove= toRemove}
