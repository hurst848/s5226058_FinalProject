#include "MarchingCubes.h"


#include "Chunk.h"
#include "Engine/Mesh.h"
#include "Engine/MeshBuilder.h"

#include <fstream>

namespace HGE
{

	//! USING: http://paulbourke.net/geometry/polygonise/
	std::shared_ptr<Mesh> MarchingCubes::RunMarchingCubes(std::shared_ptr<Chunk> _chunk)
	{
		int numTriangles = 0;
		std::vector<vec3> triangles;
		std::shared_ptr<Mesh> rtrn = std::make_shared<Mesh>();

		
		// Get all active ("on") voxels by means of an iterator.
		std::vector<vec3> activeCoords;
		activeCoords.clear();
		for (openvdb::FloatGrid::ValueOnCIter iter = _chunk->voxels->cbeginValueOn(); iter; ++iter) {
			openvdb::Coord tmp = iter.getCoord();

			activeCoords.push_back(ivec3(tmp.x(), tmp.y(), tmp.z()));
			
			//std::cout << "COORD: " << tmp.x() << ", " << tmp.y() << ", " << tmp.z() << std::endl;
		}

		for (int i = 0; i < activeCoords.size(); i++)
		{
			vec3 currLoc = activeCoords.at(i);

			vec3 cube[] =
			{
				vec3(currLoc.x		, currLoc.y		, currLoc.z		),
				vec3(currLoc.x		, currLoc.y		, currLoc.z + 1	),
				vec3(currLoc.x + 1	, currLoc.y		, currLoc.z		),
				vec3(currLoc.x + 1	, currLoc.y		, currLoc.z + 1	),
				vec3(currLoc.x		, currLoc.y + 1	, currLoc.z		),
				vec3(currLoc.x		, currLoc.y + 1	, currLoc.z + 1	),
				vec3(currLoc.x + 1	, currLoc.y + 1	, currLoc.z		),
				vec3(currLoc.x + 1	, currLoc.y + 1	, currLoc.z + 1	)
			};

			vec3 vertList[12];

			//! Deturmines which verticies are on the surface
			int cubeIndex = 0;

			if (_chunk->GetVoxelValue(cube[0]) < isoValue) { cubeIndex |= 1; }
			if (_chunk->GetVoxelValue(cube[1]) < isoValue) { cubeIndex |= 2; }
			if (_chunk->GetVoxelValue(cube[2]) < isoValue) { cubeIndex |= 4; }
			if (_chunk->GetVoxelValue(cube[3]) < isoValue) { cubeIndex |= 8; }
			if (_chunk->GetVoxelValue(cube[4]) < isoValue) { cubeIndex |= 16; }
			if (_chunk->GetVoxelValue(cube[5]) < isoValue) { cubeIndex |= 32; }
			if (_chunk->GetVoxelValue(cube[6]) < isoValue) { cubeIndex |= 64; }
			if (_chunk->GetVoxelValue(cube[7]) < isoValue) { cubeIndex |= 128; }

			//! Find verticies where the surface intersects the cube
			if (edgeTable[cubeIndex] & 1) { vertList[0] = vertexInterpolation(cube[0], cube[1], _chunk); }
			if (edgeTable[cubeIndex] & 2) { vertList[1] = vertexInterpolation(cube[1], cube[2], _chunk); }
			if (edgeTable[cubeIndex] & 4) { vertList[2] = vertexInterpolation(cube[2], cube[3], _chunk); }
			if (edgeTable[cubeIndex] & 8) { vertList[3] = vertexInterpolation(cube[3], cube[0], _chunk); }
			if (edgeTable[cubeIndex] & 16) { vertList[4] = vertexInterpolation(cube[4], cube[5], _chunk); }
			if (edgeTable[cubeIndex] & 32) { vertList[5] = vertexInterpolation(cube[5], cube[6], _chunk); }
			if (edgeTable[cubeIndex] & 64) { vertList[6] = vertexInterpolation(cube[6], cube[7], _chunk); }
			if (edgeTable[cubeIndex] & 128) { vertList[7] = vertexInterpolation(cube[7], cube[4], _chunk); }
			if (edgeTable[cubeIndex] & 256) { vertList[8] = vertexInterpolation(cube[0], cube[4], _chunk); }
			if (edgeTable[cubeIndex] & 512) { vertList[9] = vertexInterpolation(cube[1], cube[5], _chunk); }
			if (edgeTable[cubeIndex] & 1024) { vertList[10] = vertexInterpolation(cube[2], cube[6], _chunk); }
			if (edgeTable[cubeIndex] & 2048) { vertList[11] = vertexInterpolation(cube[3], cube[7], _chunk); }

			for (int i = 0; triTable[cubeIndex][i] != -1; i += 3)
			{
				triangles.push_back(vertList[triTable[cubeIndex][i]]);
				triangles.push_back(vertList[triTable[cubeIndex][i + 1]]);
				triangles.push_back(vertList[triTable[cubeIndex][i + 2]]);
				numTriangles++;
			}
		}
		MeshBuilder builder;
		rtrn = builder.CreateMesh("chunk", triangles);

		std::ofstream stream("MarchingCubePoints.xyz");
		for (int i = 0; i < triangles.size(); i++)
		{
			stream << triangles.at(i).x;
			stream << " ";
			stream << triangles.at(i).y;
			stream << " ";
			stream << triangles.at(i).z;
			stream << "\n";

		}
		stream.close();
		printf("DONE\n");


		return rtrn;
	}


	std::shared_ptr<Mesh> MarchingCubes::TraditionalRender(std::shared_ptr<Chunk> _chunk)
	{
		std::vector<vec3> triangles;
		std::shared_ptr<Mesh> rtrn = std::make_shared<Mesh>();

		std::vector<vec3> activeCoords;
		activeCoords.clear();
		for (openvdb::FloatGrid::ValueOnCIter iter = _chunk->voxels->cbeginValueOn(); iter; ++iter) {
			openvdb::Coord tmp = iter.getCoord();
			activeCoords.push_back(ivec3(tmp.x(), tmp.y(), tmp.z()));
		}

		for (int i = 0; i < activeCoords.size(); i++)
		{
			triangles.push_back
			(
				vec3
				(
					activeCoords.at(i).x,
					activeCoords.at(i).y,
					activeCoords.at(i).z
				)
			);

			triangles.push_back
			(
				vec3
				(
					activeCoords.at(i).x + 1,
					activeCoords.at(i).y,
					activeCoords.at(i).z + 1
				)
			);

			triangles.push_back
			(
				vec3
				(
					activeCoords.at(i).x,
					activeCoords.at(i).y,
					activeCoords.at(i).z + 1
				)
			);


			triangles.push_back
			(
				vec3
				(
					activeCoords.at(i).x,
					activeCoords.at(i).y,
					activeCoords.at(i).z
				)
			);

			triangles.push_back
			(
				vec3
				(
					activeCoords.at(i).x + 1,
					activeCoords.at(i).y,
					activeCoords.at(i).z + 1
				)
			);

			triangles.push_back
			(
				vec3
				(
					activeCoords.at(i).x + 1,
					activeCoords.at(i).y,
					activeCoords.at(i).z
				)
			);
		}

		MeshBuilder builder;
		rtrn = builder.CreateMesh("chunk", triangles);

		std::ofstream stream("MarchingCubePoints.xyz");
		for (int i = 0; i < triangles.size(); i++)
		{
			stream << triangles.at(i).x;
			stream << " ";
			stream << triangles.at(i).y;
			stream << " ";
			stream << triangles.at(i).z;
			stream << "\n";

		}
		stream.close();
		printf("DONE\n");


		return rtrn;
	}



	vec3 MarchingCubes::vertexInterpolation(vec3 _point1, vec3 _point2, std::shared_ptr<Chunk> _chunk)
	{
		vec3 rtrn(0,0,0);

		float mu;
		
		if (abs(isoValue - _chunk->GetVoxelValue(_point1)) < 0.00001f) { return _point1; }
		if (abs(isoValue - _chunk->GetVoxelValue(_point2)) < 0.00001f) { return _point2; }
		if (abs(_chunk->GetVoxelValue(_point1) - _chunk->GetVoxelValue(_point2)) < 0.00001f) { return _point1; }

		mu = (isoValue - _chunk->GetVoxelValue(_point1)) / (_chunk->GetVoxelValue(_point2) - _chunk->GetVoxelValue(_point1));

		rtrn.x = _point1.x + mu * (_point2.x - _point1.x);
		rtrn.y = _point1.y + mu * (_point2.y - _point1.y);
		rtrn.z = _point1.z + mu * (_point2.z - _point1.z);

		return rtrn;
	}

	void MarchingCubes::SetISOValue(float _ISO) { isoValue = _ISO; }
}