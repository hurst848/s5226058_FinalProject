#include "TerrainRenderer.h"

#include "Chunk.h"

#include "Engine/Texture.h"
#include "Engine/Mesh.h"
#include "Engine/Environment.h"
#include "Engine/Camera.h"

#include <fstream>
#include <vector>
#include <iostream>

namespace HGE
{
	std::shared_ptr<TerrainRenderer> TerrainRenderer::Initialize()
	{
		std::shared_ptr<TerrainRenderer> rtrn = std::make_shared<TerrainRenderer>();
		
		rtrn->LoadShader("../src/ProcedualPlanetGenerator/PPG_Shaders/PPG_VertexShader.vert");
		rtrn->LoadShader("../src/ProcedualPlanetGenerator/PPG_Shaders/PPG_FragmentShader.frag");

		rtrn->texture = std::make_shared<Texture>();
		rtrn->texture->Load("../src/ProcedualPlanetGenerator/PPG_Textures/basicTerrainTexture.png");

		rtrn->projectionMatrix = perspective(radians(60.0f), (1.0f * 2560) / (1.0f * 1440), 0.1f, std::numeric_limits<float>::max());

		return rtrn;
	}

	void TerrainRenderer::SetChunk(std::shared_ptr<Chunk> _chunk)
	{
		chunk = _chunk;
	}

	void TerrainRenderer::Render()
	{
		mat4 modelMatrix(1.0f);

		modelMatrix = translate(modelMatrix, chunk.lock()->Position);

		modelMatrix = scale(modelMatrix, vec3(0.1f, 1.0f, 0.1f));

		modelMatrix = rotate(modelMatrix, radians(chunk.lock()->Rotation.x), vec3(0, 1, 0));
		modelMatrix = rotate(modelMatrix, radians(chunk.lock()->Rotation.y), vec3(1, 0, 0));
		modelMatrix = rotate(modelMatrix, radians(chunk.lock()->Rotation.z), vec3(0, 0, 1));

		glUseProgram(programID);
		glBindVertexArray(chunk.lock()->mesh->ID);
		glBindTexture(GL_TEXTURE_2D, texture->ID);

		glUniformMatrix4fv(UID_ModelMatrix, 1, GL_FALSE, value_ptr(modelMatrix));
		glUniformMatrix4fv(UID_ProjectionMatrix, 1, GL_FALSE, value_ptr(projectionMatrix));
		glUniformMatrix4fv(UID_ViewMatrix, 1, GL_FALSE, value_ptr(inverse(environment.lock()->mainCamera->GetViewMatrix())));
		glUniform3fv(UID_LightColour, 1, glm::value_ptr(vec3(0.95f, 0.91f, 0.61f)));
		glUniform3fv(UID_LightPosition, 1, glm::value_ptr(vec3(0, 100, 0)));
		glUniform1f(UID_LightAmbientStrength, 0.5f);
		glUniform1i(texture->ID, 1);


		glEnable(GL_DEPTH_TEST);
		//glEnable(GL_CULL_FACE);
		glDrawArrays(GL_TRIANGLES, 0, chunk.lock()->mesh->VertexCount);
		//glDisable(GL_CULL_FACE);
		glDisable(GL_DEPTH_TEST);

		glBindTexture(GL_TEXTURE_2D, 0);
		glBindVertexArray(0);
		glUseProgram(0);
	}

	void TerrainRenderer::Compile()
	{
		programID = glCreateProgram();

		glAttachShader(programID, vertexShaderID);
		glAttachShader(programID, fragmentShaderID);

		glBindAttribLocation(programID, 0, "a_Position");
		glBindAttribLocation(programID, 1, "a_TextureCoordinates");
		glBindAttribLocation(programID, 2, "a_Normal");

		glLinkProgram(programID);
		int success = 0;
		glGetProgramiv(programID, GL_LINK_STATUS, &success);
		if (!success) {
			GLint maxLength = 0;
			glGetProgramiv(programID, GL_INFO_LOG_LENGTH, &maxLength);
			std::vector<GLchar> errorLog(maxLength);
			glGetProgramInfoLog(programID, maxLength, &maxLength, &errorLog.at(0));
			std::cout << &errorLog.at(0) << std::endl;
			throw std::exception();
		}

		glDetachShader(programID, vertexShaderID);
		glDetachShader(programID, fragmentShaderID);

		//! Add uniform locations
		UID_ModelMatrix = glGetUniformLocation(programID, "u_ModelMatrix");
		UID_ProjectionMatrix = glGetUniformLocation(programID, "u_ProjectionMatrix");
		UID_ViewMatrix = glGetUniformLocation(programID, "u_ViewMatrix");
		UID_LightColour = glGetUniformLocation(programID, "u_LightColour");
		UID_LightPosition = glGetUniformLocation(programID, "u_LightPosition");
		UID_LightAmbientStrength = glGetUniformLocation(programID, "u_LightAmbientStrength");
		UID_TEXTURE = glGetUniformLocation(programID, "u_Texture");
	}
	void TerrainRenderer::LoadShader(std::string _path)
	{
		std::string line;
		std::ifstream vis(_path);
		std::string input;
		for (int i = 0; std::getline(vis, line); i++)
		{
			input += line;
		}

		const GLchar* rlcode = input.c_str();
		GLint success = 0;
		vis.close();
		std::cout << _path << "\n";
		if (_path.find(".vert") != std::string::npos)
		{
			vertexShaderID = glCreateShader(GL_VERTEX_SHADER);
			glShaderSource(vertexShaderID, 1, &rlcode, NULL);
			glCompileShader(vertexShaderID);
			glGetShaderiv(vertexShaderID, GL_COMPILE_STATUS, &success);
			if (!success)
			{
				GLint maxLength = 0;
				glGetShaderiv(vertexShaderID, GL_INFO_LOG_LENGTH, &maxLength);
				std::vector<GLchar> errorLog(maxLength);
				glGetShaderInfoLog(vertexShaderID, maxLength, &maxLength, &errorLog.at(0));
				std::cout << &errorLog.at(0) << std::endl;
				throw std::exception();
			}
		}
		else if (_path.find(".frag") != std::string::npos)
		{
			fragmentShaderID = glCreateShader(GL_FRAGMENT_SHADER);
			glShaderSource(fragmentShaderID, 1, &rlcode, NULL);
			glCompileShader(fragmentShaderID);
			glGetShaderiv(fragmentShaderID, GL_COMPILE_STATUS, &success);
			if (!success)
			{
				GLint maxLength = 0;
				glGetShaderiv(fragmentShaderID, GL_INFO_LOG_LENGTH, &maxLength);
				std::vector<GLchar> errorLog(maxLength);
				glGetShaderInfoLog(fragmentShaderID, maxLength, &maxLength, &errorLog.at(0));
				std::cout << &errorLog.at(0) << std::endl;
				throw std::exception();
			}
		}
	}
}