#include "Engine/Maths.h"
#include "Engine/Component.h"

#include <glew.h>

#include <memory>
#include <string>


namespace HGE
{
	struct Chunk;
	struct Texture;
	struct Mesh;

	struct TerrainRenderer : Component
	{
		friend struct Entity;

	public:
		
		void Compile();
		
		template<typename T>
		void SetChunk(std::shared_ptr<T> _chunk)
		{
			chunk = _chunk;
		}


		bool Render;

	private:
		void onCreate();
		void onRender();
		
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
	};
}