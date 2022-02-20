this is how data is loaded (or should be)

``` mermaid
sequenceDiagram
    %% participant g as generator
    participant l as loader
    participant vw as VoxelWorld
    participant c as chunks
    participant vs as voxels
    participant vd as voxeldata

    alt load import
    l->>vw: load import (data)
    vw->>c: create new
    vw->>c: populate (data)
    c->>vs: create (data)
    %% vw->>c: init
    c->>vs: init
    vs->>vd: init
    else load save
    l->>vw: load save
    vw->>c: create new
    vw->>vs: load voxels
    c->>vs: init
    vs->>vd: init
    else generate
    l->>vw: load empty
    %% l->>g: gen chunk
    %% g->>vw: load(data)
    vw->>c: create new
    c->>vs: create empty
    opt maybe optional?
    %% vw->>c: init
    c->>vs: init
    vs->>vd: init
    end
    l->>l: gen
    l->>c: set (data)
    %% vw->>c: populate (data)
    c->>vs: set (data)
    vs->>vd: init
    end
```