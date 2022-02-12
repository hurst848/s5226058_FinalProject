#include "Sphere.h"

#include <vector>
#include <fstream>

#include <CGAL/Exact_predicates_inexact_constructions_kernel.h>
#include <CGAL/Polyhedron_3.h>
#include <CGAL/poisson_surface_reconstruction.h>
#include <CGAL/IO/read_points.h>

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
			float y = 1 - (i / float(_samples - 1)) * 2 * _radius;
			float radius = sqrt(1 - y * y) ;
			float theta = phi * i;

			float x = cos(theta) * radius;
			float z = sin(theta) * radius;
			points.push_back((vec3(x, y, z) ));
		}
		printf("DONE\n");
		return true;
	}
}