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

		std::shared_ptr<Shader> vertex = GetCore()->Resources->AddResource<Shader>("../shaders/BasicCubeSphere.vert");
		std::shared_ptr<Shader> fragment = GetCore()->Resources->AddResource<Shader>("../shaders/BasicCubeSphere.frag");
		
		for (int i = 0; i < 6; i++)
		{
			std::shared_ptr<ShaderProgram> Program = std::make_shared<ShaderProgram>();
			Program->AddShader(vertex);
			Program->AddShader(fragment);
			Program->Compile();

			faces.at(i) = TerrainFace().Initialize(_subDivisions, directions[i]);
			faces.at(i)->ContructMesh();

			std::shared_ptr<MeshRenderer> tmpRend = GetEntity()->AddComponent<MeshRenderer>();
			tmpRend->SetMesh(faces.at(i)->mesh);
			tmpRend->SetShader(Program);
			tmpRend->Render = true;
			
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

		return rtrn;
	}

	void TerrainFace::ContructMesh()
	{
		std::vector<vec3> verticies (Resolution * Resolution);
		std::vector<vec3> normals((Resolution - 1) * (Resolution - 1) * 6);
		std::vector<int> triangles((Resolution - 1) * (Resolution - 1) * 6);

		for (int i = 0; i < normals.size(); i++)
		{
			normals.at(i) = LocalUp;
		}

		int triIndex = 0;
		float increment = 1.0f / (float)Resolution;
		for	(int i = 0; i < Resolution; i++) // Y
		{
			for	(int j = 0; j < Resolution; j++) // X
			{



				/*vec3 pointOnUnitCube = LocalUp + (j*increment) * 2 * AxisA + (i*increment) * 2 * AxisB;
				vec3 pointOnUnitSphere = normalize(pointOnUnitCube);
				verticies.at(j + i * Resolution) = pointOnUnitSphere;
				int itr = j + i * Resolution;*/
				//vec2 percent = vec2(j, i) / (float)(Resolution - 1);
				//vec3 pointOnUnitCube = LocalUp + (percent.x - 0.5f) * 2 * AxisA + (percent.y - 0.5f) * 2 * AxisB;
				//vec3 pointOnUnitSphere = normalize(pointOnUnitCube);
				//verticies.at(itr) = pointOnUnitSphere;

				if (i != Resolution - 1 && j != Resolution - 1)
				{
					triangles.at(triIndex) = itr;
					triangles.at(triIndex + 1) = itr + Resolution + 1;
					triangles.at(triIndex + 2) = itr + Resolution;
					
					

					triangles.at(triIndex + 3) = itr;
					triangles.at(triIndex + 4) = itr + 1;
					triangles.at(triIndex + 5) = itr + Resolution + 1;
					
					triIndex += 6;
				}
			}
		}
		MeshBuilder builder;


		std::ofstream stream("FaceOutput.xyz");
		for (int i = 0; i < verticies.size(); i++)
		{
			stream << verticies.at(i).x;
			stream << " ";
			stream << verticies.at(i).y;
			stream << " ";
			stream << verticies.at(i).z;
			stream << "\n";

		}
		stream.close();

		std::string name = std::to_string((int)LocalUp.x) + "_" + std::to_string((int)LocalUp.y) + "_" + std::to_string((int)LocalUp.z) + "_" + "face";
		mesh = builder.CreateMesh(name, verticies, normals, triangles);

	}
}