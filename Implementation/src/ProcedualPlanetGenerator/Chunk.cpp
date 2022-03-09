#include "Chunk.h"

#include "MarchingCubes.h"
#include "TerrainRenderer.h"

namespace HGE
{
	std::shared_ptr<Chunk> Chunk::Initialize()
	{
		openvdb::initialize();

		std::shared_ptr<Chunk> rtrn = std::make_shared<Chunk>();

		rtrn->renderer = TerrainRenderer().Initialize();
		rtrn->marchingCubes = MarchingCubes().Initialize();

		rtrn->voxels = openvdb::FloatGrid::create();

		for (int i = 0; i < ChunkSize; i++)
		{
			for (int j = 0; j < ChunkSize; j++)
			{
				SetVoxelValue(ivec3(i, MinHeight, j), 1.0f, false);
			}
		}
	}


	float Chunk::GetVoxelValue(ivec3 _position)
	{
		openvdb::FloatGrid::Accessor accessor = voxels->getAccessor();
		openvdb::Coord location(_position.x, _position.y, _position.z);
		float rtrn = accessor.getValue(location);
		return rtrn;
	}
	void Chunk::SetVoxelValue(ivec3 _position, float _value, bool _regenerate)
	{
		openvdb::FloatGrid::Accessor accessor = voxels->getAccessor();
		openvdb::Coord location(_position.x, _position.y, _position.z);
		accessor.setValue(location, _value);
		if (_regenerate) { regenerateMesh(); }
	}

	void Chunk::Render()
	{

	}

	void Chunk::regenerateMesh()
	{
		mesh = MarchingCubes::Generate(Self.lock());
	}
}