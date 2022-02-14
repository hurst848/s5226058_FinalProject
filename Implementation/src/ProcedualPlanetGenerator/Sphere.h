#include "Engine/HGE.h"


namespace HGE
{

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

	struct TerrainFace : private MeshRenderer
	{
	public:
		std::shared_ptr<TerrainFace> Initialize(int _res, vec3 _localUp);

		void ContructMesh();

	
		int Resolution;
		vec3 LocalUp;
		vec3 AxisA;
		vec3 AxisB;
	};

}