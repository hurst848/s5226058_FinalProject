#include "Engine/Maths.h"

#include <openvdb/openvdb.h>

#include <memory>

namespace HGE
{
	struct Mesh;
	struct TerrainRenderer;
	struct MarchingCubes;


	struct Chunk
	{
		friend struct MarchingCubes;
		friend struct TerrainRenderer;

	public:

		std::shared_ptr<Chunk> Initialize();

		
		float GetVoxelValue(ivec3 _position);
		void SetVoxelValue(ivec3 _position, float _value, bool _regenerate);

		void Render();

		std::weak_ptr<Chunk> Self;

		unsigned long int MinHeight =  45000;
		unsigned long int MaxHeight = 100000;

		unsigned int ChunkSize = 100;

		vec3 Position;
		vec3 Rotation;

	private:
		void regenerateMesh();

		std::shared_ptr<TerrainRenderer> renderer;
		std::shared_ptr<MarchingCubes> marchingCubes;
		std::shared_ptr<Mesh> mesh;

		

		openvdb::FloatGrid::Ptr voxels;
	};
}