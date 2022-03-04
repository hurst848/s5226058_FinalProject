#include <memory>
#include <vector>


namespace HGE
{
	struct Chunk
	{
	public:
		std::shared_ptr<Chunk> Initialize();
	


		bool GenerateChunk();



	private:
		unsigned long int maxHeight;
		unsigned long int minHeight;
		
	};
}