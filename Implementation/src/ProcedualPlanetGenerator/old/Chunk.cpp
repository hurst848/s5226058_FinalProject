#include "Chunk.h"

#include "MarchingCubes.h"
#include "TerrainRenderer.h"


#include "Engine/Transform.h"
#include "Engine/MeshRenderer.h"
#include "Engine/Mesh.h"
#include "Engine/Entity.h"


#include <iostream>

namespace HGE
{
	void Chunk::onCreate()
	{
		openvdb::initialize();

		std::weak_ptr<TerrainRenderer> renderer = GetEntity()->AddComponent<TerrainRenderer>();
		renderer.lock()->SetChunk(Self.lock());
		renderer.lock()->Render = true;
		
		mc = std::make_shared<MarchingCubes>();
		voxels = openvdb::FloatGrid::create();

		for (int i = 0; i < chunkSize; i++)
		{
			for (int j = 0; j < chunkSize; j++)
			{
				if (j % 2 == 0)
				{
					SetVoxelValue(ivec3(i, minHeight + 1, j), 1.0f);
				}
				else
				{
					SetVoxelValue(ivec3(i, minHeight, j), 1.0f);
				}
			}
		}

	}




	void Chunk::SetVoxelValue(ivec3 _position, float _value)
	{
		openvdb::FloatGrid::Accessor accessor = voxels->getAccessor();
		openvdb::Coord location(_position.x, _position.y, _position.z);
		accessor.setValue(location, _value);
	}

	float Chunk::GetVoxelValue(ivec3 _position)
	{
		openvdb::FloatGrid::Accessor accessor = voxels->getAccessor();
		openvdb::Coord location(_position.x, _position.y, _position.z);
		float rtrn = accessor.getValue(location);
		return rtrn;
	}
}