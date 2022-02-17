
todo voxel mesh system
- how to assign blocks and voxel materials
  - should be synced and mostly automatic
  - can be done in editor and in code (and from source files?)
    - all are scriptable objects?
    - editor user creation and runtime creation
      - automatic editor creation?
  - voxel materials contains texture coords, materials, etc
  - block type contains voxelmatid (or contains the whole mat?)
  - atlas automatic name fetching
    - atlas is created from individual textures
    - has imagescale to be copied to material set?
    - name key and texcoord value
    - when updated, corresponding voxelmat needs to be called
    - this is a SO?
  - voxel material from name func?
  - BlockManager func create blocktype
    - pass voxmaterial, name, etc
  - custom block manager editor
    - make block button?

