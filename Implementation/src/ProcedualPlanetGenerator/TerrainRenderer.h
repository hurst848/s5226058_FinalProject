#include "Engine/Maths.h"

#include <glew.h>

#include <memory>
#include <string>

namespace HGE
{
	struct Chunk;
	struct Texture;
	struct GameEnvironment;

	struct TerrainRenderer
	{
	public:
		std::shared_ptr<TerrainRenderer> Initialize();

		void SetChunk(std::shared_ptr<Chunk> _chunk);

		void Render();


		std::weak_ptr<TerrainRenderer> Self;

	private:
		
		void Compile();
		void LoadShader(std::string _path);
		
		GLuint vertexShaderID;
		GLuint fragmentShaderID;
		GLuint programID;

		// Uniform IDs
		GLuint UID_ModelMatrix;
		GLuint UID_ProjectionMatrix;
		GLuint UID_ViewMatrix;
		GLuint UID_LightColour;
		GLuint UID_LightPosition;
		GLuint UID_LightAmbientStrength;
		GLuint UID_TEXTURE;

		mat4 projectionMatrix;

		std::weak_ptr<Chunk> chunk;

		std::shared_ptr<Texture> texture;

		std::weak_ptr<GameEnvironment> environment;
	};
}