#include <memory>
#include <vector>


namespace HGE
{
	struct Chunk;

	struct Face
	{
	public:
		std::shared_ptr<Face> Initialize();




	private:
		std::weak_ptr<Face> self;

		std::vector<std::shared_ptr<Chunk>> Chunks;

		// 700 X 700 chunks


	};
}