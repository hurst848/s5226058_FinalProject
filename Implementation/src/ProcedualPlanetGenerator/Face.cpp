#include "Face.h"

#include "Chunk.h"


namespace HGE
{
	std::shared_ptr<Face> Face::Initialize()
	{
		std::shared_ptr<Face> rtrn = std::make_shared<Face>();

		for (int i = 0; i < rtrn->chunkDimentions; i++)
		{
			for (int j = 0; j < rtrn->chunkDimentions; j++)
			{
				rtrn->chunks.push_back(Chunk().Initialize());
				//rtrn->chunks.at(i+j)->
			}
		}
		return rtrn;
	}
}