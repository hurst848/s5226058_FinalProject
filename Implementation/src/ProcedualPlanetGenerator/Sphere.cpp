#include "Sphere.h"

#include <Engine/HGE.h>

#include <boost/polygon/voronoi.hpp>

#include <vector>
#include <fstream>







namespace HGE
{
	bool Sphere::GenerateUVSphere(float radius, int numLat, int numLong)
	{

		points.resize(numLat * (numLong + 1) + 2);
		
		points.at(0) = vec3(0, radius, 0);
		points.at(0) = vec3(0, -radius, 0);

		float latSpacing = 1.0f / (numLat + 1.0f);
		float longSpacing = 1.0f / numLong;

		int v = 1;
		for (int lat = 0; lat < numLat; lat++)
		{
			for (int longi = 0; longi < numLong; longi++)
			{
				vec2 tmp = vec2(longi * longSpacing, 1.0f- (lat + 1) * latSpacing);

				float theta = tmp.x * 2.0f * 3.14159265359f;
				float phi = (tmp.y - 0.5f) * 3.14159265359f;

				float c = cos(phi);

				points.at(v) = vec3
				(
					c* cos(theta),
					sin(phi),
					c * sin(theta)
				) * radius;

				v++;
			}
		}

		std::ofstream stream("Output.xyz");
		for (int i = 0; i < points.size(); i++)
		{
			stream << points.at(i).x;
			stream << " ";
			stream << points.at(i).y;
			stream << " ";
			stream << points.at(i).z;
			stream << "\n";

		}
		stream.close();
		printf("DONE\n");



		return true;
	}

	bool Sphere::GenerateFibonacciSphere(int _samples, float _radius)
	{
		float phi = 3.14159265359f * (3.0f - sqrt(5.0f));
		for (int i = 0; i < _samples; i++)
		{
			float y = 1 - (i / float(_samples - 1)) * 2;
			float radius = sqrt(1 - y * y) ;
			float theta = phi * i;

			float x = cos(theta) * radius;
			float z = sin(theta) * radius;
			points.push_back((vec3(x, y, z) * _radius));
		}

		std::ofstream stream("FibOutput.xyz");
		for (int i = 0; i < points.size(); i++)
		{
			stream << points.at(i).x;
			stream << " ";
			stream << points.at(i).y;
			stream << " ";
			stream << points.at(i).z;
			stream << "\n";

		}
		stream.close();

		printf("DONE\n");





		return true;
	}


	bool Sphere::GenerateNormalizedCube(int _subDivisions, float _radius)
	{
		std::vector<std::shared_ptr<TerrainFace>> faces(6);
		vec3 directions[]
		{
			vec3(1,0,0), vec3(-1,0,0),
			vec3(0,1,0), vec3(0,-1,0),
			vec3(0,0,1), vec3(0,0,-1)
		};

		for (int i = 0; i < 6; i++)
		{
			faces.at(i) = TerrainFace().Initialize(_subDivisions, directions[i]);
		}

		return true;
	}

	bool Sphere::GenerateSpherifiedCube(int _subDivisions, float _radius)
	{
		return true;
	}

	std::shared_ptr<TerrainFace> TerrainFace::Initialize(int _res, vec3 _localUp)
	{
		std::shared_ptr<TerrainFace> rtrn = std::make_shared<TerrainFace>();

		rtrn->LocalUp = _localUp;
		rtrn->Resolution = _res;

		rtrn->AxisA = vec3(_localUp.y, _localUp.z, _localUp.x);	
		rtrn->AxisB = cross(_localUp, AxisA);

	}

	void TerrainFace::ContructMesh()
	{
		std::vector<vec3> verticies (Resolution * Resolution);
		std::vector<int> triangles((Resolution - 1) * (Resolution - 1) * 6);

		int triIndex = 0;

		for	(int i = 0; i < Resolution; i++) // Y
		{
			for	(int j = 0; j < Resolution; j++) // X
			{
				int itr = j + (i * Resolution);
				vec2 percent = vec2(j, i / (Resolution - 1));
				vec3 pointOnUnitCube = LocalUp + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
				verticies.at(itr) = pointOnUnitCube;

				if (i < Resolution - 1 && j < Resolution - 1)
				{
					triangles.at(triIndex) = i;
					triangles.at(triIndex + 1) = i + Resolution + 1;
					triangles.at(triIndex = 2) = i + Resolution;

					triangles.at(triIndex + 3) = i;
					triangles.at(triIndex + 4) = i + 1;
					triangles.at(triIndex + 5) = i + Resolution + 1;
					triIndex += 6;
				}
			}
		}

	}
}