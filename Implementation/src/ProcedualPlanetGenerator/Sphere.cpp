#include "Sphere.h"

#include <vector>
#include <fstream>

#include <Engine/HGE.h>




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


		std::string fname = "FibOutput.xyz";

		std::cerr << "Reading " << std::flush;
		std::vector<Point> fpoints;
		if (!CGAL::IO::read_points(fname, std::back_inserter(fpoints)))
		{
			std::cerr << "Error: cannot read file" << std::endl;
			return EXIT_FAILURE;
		}
		std::cerr << "done: " << fpoints.size() << " points." << std::endl;

		std::cerr << "Reconstruction ";
		CGAL::Timer t;
		t.start();

		Reconstruction reconstruct(fpoints.begin(), fpoints.end());
		reconstruct.increase_scale(4);
		reconstruct.reconstruct_surface();
		//std::cerr << "done in " << t.time() << " sec." << std::endl;
		//t.reset();
		//std::ofstream out("sphere.off");
		//out << reconstruct;
		//std::cerr << "Writing result in " << t.time() << " sec." << std::endl;
		//std::cerr << "Done." << std::endl;
		//return EXIT_SUCCESS;
		//std::vector<Pwn> points;

		//if (!CGAL::IO::read_points("FibOutput.xyz", std::back_inserter(points),
		//	CGAL::parameters::point_map(CGAL::First_of_pair_property_map<Pwn>())
		//	.normal_map(CGAL::Second_of_pair_property_map<Pwn>())))
		//{
		//	std::cerr << "Error: cannot read input file!" << std::endl;
		//	return EXIT_FAILURE;
		//}
		//Polyhedron output_mesh;
		//double average_spacing = CGAL::compute_average_spacing<CGAL::Sequential_tag>
		//	(points, 6, CGAL::parameters::point_map(CGAL::First_of_pair_property_map<Pwn>()));



		////if (CGAL::poisson_surface_reconstruction_delaunay
		////(points.begin(), points.end(),
		////	CGAL::First_of_pair_property_map<Pwn>(),
		////	CGAL::Second_of_pair_property_map<Pwn>(),
		////	output_mesh, average_spacing))
		////{
		////	std::ofstream out("kitten_poisson-20-30-0.375.off");
		////	out << output_mesh;
		////}



		return true;
	}


	bool Sphere::GenerateNormalizedCube(int _subDivisions, float _radius)
	{


		return true;
	}

	bool Sphere::GenerateSpherifiedCube(int _subDivisions, float _radius)
	{
		return true;
	}
}