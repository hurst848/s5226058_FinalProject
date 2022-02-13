#include "Engine/HGE.h"

#include <CGAL/Exact_predicates_inexact_constructions_kernel.h>
#include <CGAL/Scale_space_surface_reconstruction_3.h>
#include <CGAL/IO/read_points.h>
#include <CGAL/Timer.h>

namespace HGE
{
	typedef CGAL::Exact_predicates_inexact_constructions_kernel     Kernel;
	typedef Kernel::Point_3                                         Point;
	typedef CGAL::Scale_space_surface_reconstruction_3<Kernel>      Reconstruction;
	typedef Reconstruction::Facet_const_iterator                    Facet_iterator;

	struct Sphere
	{
	public:
		bool GenerateUVSphere(float radius, int numLat, int numLong);

		bool GenerateFibonacciSphere(int _samples, float _radius);

		bool GenerateNormalizedCube(int _subDivisions, float _radius);

		bool GenerateSpherifiedCube(int _subDivisions, float _radius);

		bool GenerateIcosahedron();

		std::vector<vec3> points;
	private:
		
	};

}