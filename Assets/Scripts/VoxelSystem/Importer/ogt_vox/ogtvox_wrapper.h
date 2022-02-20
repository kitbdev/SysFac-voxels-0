#pragma once

#include "ogt_vox.h"
// 1. load a .vox file off disk into a memory buffer.

// 2. construct a scene from the memory buffer:
// ogt_vox_scene* scene = ogt_vox_read_scene(buffer, buffer_size);

// 3. use the scene members to extract the information you need. eg.
// printf("# of layers: %u\n", scene->num_layers );

// 4. destroy the scene:
// ogt_vox_destroy_scene(scene);

class ManagedOgtVox
{

private:
      // HelloWorld hw;

public:
      ManagedOgtVox();

      ~ManagedOgtVox();

      void SayThis(wchar_t *phrase);
};
