#include "Engine/Maths.h"

#include <vector>
#include <memory>

namespace HGE
{
	struct Chunk;

	struct Face
	{
	public:
		std::shared_ptr<Face> Initialize();


		std::weak_ptr<Face> Self;


		vec3 Position;
		vec3 Rotation;

	private:
		vec3 faceNormal;
		int chunkDimentions = 10;

		std::vector<std::shared_ptr<Chunk>> chunks;

	};
}