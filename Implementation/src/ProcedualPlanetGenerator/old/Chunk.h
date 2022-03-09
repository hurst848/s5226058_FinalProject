
#include "Engine/Component.h"
#include "Engine/Maths.h"

#include <openvdb/openvdb.h>

#include <memory>
#include <vector>

//! Chunk takes up the space of a 100 X 100 area



namespace HGE
{
	struct Mesh;

	/*
	struct Chunk : Component, std::enable_shared_from_this<Chunk>
	{

		friend struct MarchingCubes;
		friend struct TerrainRenderer;
		friend struct Entity;
	public:

		std::shared_ptr<Chunk> Initialize();

		
		
	
		bool GenerateChunk();

		void SetVoxelValue(ivec3 _position, float _value);
		float GetVoxelValue(ivec3 _position);

		std::weak_ptr<Chunk> ChunkSelf;
	private:
		void onCreate();
		
		unsigned long int maxHeight = 100000;
		unsigned long int minHeight = 45000;
	
		openvdb::FloatGrid::Ptr voxels;

		const int chunkSize = 100;
		std::shared_ptr<Mesh> chunkMesh;

		std::shared_ptr<MarchingCubes> mc;
	};*/


	struct Chunk : Component
	{
		friend struct TerrainRenderer;
		friend struct MarchingCubes;
		friend struct Entity;
	public:

		void SetVoxelValue(ivec3 _position, float _value);
		float GetVoxelValue(ivec3 _position);

	private:

		void onCreate();

		unsigned long int maxHeight = 100000;
		unsigned long int minHeight =  45000;

		openvdb::FloatGrid::Ptr voxels;

		const int chunkSize;

		std::shared_ptr<Mesh> mesh;

		std::shared_ptr<MarchingCubes> mc;


	};
}