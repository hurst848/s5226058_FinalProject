#include "Chunk.h"

#include "Engine/Transform.h"
#include "Engine/MeshRenderer.h"

#include <iostream>

namespace HGE
{

	std::shared_ptr<Chunk> Chunk::Initialize()
	{
		openvdb::initialize();

		std::shared_ptr<Chunk> rtrn = std::make_shared<Chunk>();

		rtrn->AddComponent<Transform>();
		rtrn->AddComponent<MeshRenderer>();

		rtrn->voxels = openvdb::FloatGrid::create();
		

		rtrn->minHeight = 100;
		rtrn->maxHeight = 1000;

		for (int i = 0; i < rtrn->chunkSize; i++)
		{
			for (int j = 0; j < rtrn->chunkSize; j++)
			{
				rtrn->SetVoxelValue(ivec3(i, rtrn->minHeight, j), 1.0f);
			}
		}
		
		

		std::cout << rtrn->GetVoxelValue(ivec3(0, rtrn->minHeight, 0)) << std::endl;

		return rtrn;

	}

	bool Chunk::GenerateChunk()
	{
		return true;
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