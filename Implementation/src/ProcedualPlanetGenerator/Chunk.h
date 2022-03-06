
#include "Engine/Entity.h"
#include "Engine/Maths.h"

#include <openvdb/openvdb.h>

#include <memory>
#include <vector>

// 1 Chunk takes up the space of a 100 X 100 area

struct Mesh;

namespace HGE
{
	struct Chunk : Entity
	{
		friend struct MarchingCubes;
	public:

		std::shared_ptr<Chunk> Initialize();
		
	
		bool GenerateChunk();

		void SetVoxelValue(ivec3 _position, float _value);
		float GetVoxelValue(ivec3 _position);

	private:
		unsigned long int maxHeight = 1000;
		unsigned long int minHeight = 100;
	
		openvdb::FloatGrid::Ptr voxels;

		const int chunkSize = 2048;
		std::shared_ptr<Mesh> chunkMesh;
	};
}