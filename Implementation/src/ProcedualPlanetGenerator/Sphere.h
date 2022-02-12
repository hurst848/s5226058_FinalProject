#include "Engine/HGE.h"


namespace HGE
{
	struct Sphere
	{
	public:
		bool GenerateUVSphere(float radius, int numLat, int numLong);

		bool GenerateFibonacciSphere(int _samples, float _radius);

		std::vector<vec3> points;
	private:
		
	};

}