#include "Face.h"

#include "Chunk.h"


namespace HGE
{
	std::shared_ptr<Face> Face::Initialize()
	{
		std::shared_ptr<Face> rtrn = std::make_shared<Face>();
		for (int i = 0; i < 490000; i++)
		{
			rtrn->Chunks.push_back(Chunk().Initialize());
		}
		rtrn->self = rtrn;

		return rtrn;

	}
}